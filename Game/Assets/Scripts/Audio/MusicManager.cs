using UnityEngine;
using ZombieOverdrive.Core;

namespace ZombieOverdrive.Audio
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] private bool musicEnabled = true;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.75f;
        [SerializeField] private float fadeSpeed = 1.8f;

        private const string VolumePrefKey = "ZombieOverdrive.MusicVolume";
        private const float MaxOutputVolume = 0.24f;

        private AudioSource activeSource;
        private AudioSource fadingSource;
        private AudioClip menuClip;
        private AudioClip battleClip;
        private AudioClip bossClip;
        private MusicMode currentMode = MusicMode.None;
        private float activeTargetVolume;

        public static MusicManager Instance { get; private set; }

        private enum MusicMode
        {
            None,
            Menu,
            Battle,
            Boss
        }

        private void Awake()
        {
            Instance = this;
            if (PlayerPrefs.HasKey(VolumePrefKey))
            {
                masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePrefKey));
            }
            else if (masterVolume > 0f && masterVolume <= MaxOutputVolume)
            {
                masterVolume = Mathf.Clamp01(masterVolume / MaxOutputVolume);
            }

            activeSource = CreateSource("Music A");
            fadingSource = CreateSource("Music B");
            menuClip = CreateMenuLoop();
            battleClip = CreateBattleLoop();
            bossClip = CreateBossLoop();
        }

        public float NormalizedVolume => Mathf.Clamp01(masterVolume);

        public void SetNormalizedVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumePrefKey, masterVolume);
            PlayerPrefs.Save();
        }

        private void Update()
        {
            if (!musicEnabled)
            {
                FadeTo(MusicMode.None);
            }
            else
            {
                FadeTo(GetDesiredMode());
            }

            float step = Time.unscaledDeltaTime * fadeSpeed;
            if (activeSource != null)
            {
                activeSource.volume = Mathf.MoveTowards(activeSource.volume, activeTargetVolume, step);
            }

            if (fadingSource != null && fadingSource.isPlaying)
            {
                fadingSource.volume = Mathf.MoveTowards(fadingSource.volume, 0f, step);
                if (fadingSource.volume <= 0.001f)
                {
                    fadingSource.Stop();
                }
            }
        }

        private MusicMode GetDesiredMode()
        {
            GameManager manager = GameManager.Instance;
            if (manager == null)
            {
                return MusicMode.None;
            }

            if (manager.State == GameState.MainMenu || manager.State == GameState.GameOver || manager.State == GameState.Victory)
            {
                return MusicMode.Menu;
            }

            if (manager.State == GameState.Playing || manager.State == GameState.Paused || manager.State == GameState.LevelUp)
            {
                return manager.ElapsedSeconds >= 360f ? MusicMode.Boss : MusicMode.Battle;
            }

            return MusicMode.None;
        }

        private void FadeTo(MusicMode mode)
        {
            if (mode == currentMode)
            {
                activeTargetVolume = GetModeVolume(mode);
                return;
            }

            currentMode = mode;
            AudioClip clip = GetClip(mode);
            activeTargetVolume = GetModeVolume(mode);
            AudioSource oldSource = activeSource;
            activeSource = fadingSource;
            fadingSource = oldSource;

            if (clip == null || activeSource == null)
            {
                activeTargetVolume = 0f;
                return;
            }

            activeSource.clip = clip;
            activeSource.volume = 0f;
            activeSource.loop = true;
            activeSource.Play();
        }

        private AudioClip GetClip(MusicMode mode)
        {
            switch (mode)
            {
                case MusicMode.Menu:
                    return menuClip;
                case MusicMode.Battle:
                    return battleClip;
                case MusicMode.Boss:
                    return bossClip;
                default:
                    return null;
            }
        }

        private float GetModeVolume(MusicMode mode)
        {
            float outputVolume = Mathf.Clamp01(masterVolume) * MaxOutputVolume;
            switch (mode)
            {
                case MusicMode.Menu:
                    return outputVolume * 0.58f;
                case MusicMode.Battle:
                    return outputVolume * 0.78f;
                case MusicMode.Boss:
                    return outputVolume;
                default:
                    return 0f;
            }
        }

        private AudioSource CreateSource(string sourceName)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.loop = true;
            source.volume = 0f;
            source.ignoreListenerPause = true;
            source.name = sourceName;
            return source;
        }

        private static AudioClip CreateMenuLoop()
        {
            return CreateLoop(
                "menu_8bit_loop",
                92f,
                16,
                new[] { 57, -1, 60, -1, 64, -1, 60, -1, 55, -1, 59, -1, 62, -1, 59, -1 },
                new[] { 45, -1, 45, -1, 43, -1, 43, -1, 40, -1, 40, -1, 43, -1, 43, -1 },
                0.28f,
                0.18f,
                0.04f);
        }

        private static AudioClip CreateBattleLoop()
        {
            return CreateLoop(
                "battle_8bit_loop",
                132f,
                16,
                new[] { 52, 55, 59, 55, 52, 55, 60, 55, 50, 53, 57, 53, 50, 53, 59, 53 },
                new[] { 40, 40, -1, 40, 43, 43, -1, 43, 38, 38, -1, 38, 43, 43, -1, 43 },
                0.3f,
                0.24f,
                0.1f);
        }

        private static AudioClip CreateBossLoop()
        {
            return CreateLoop(
                "boss_8bit_loop",
                146f,
                16,
                new[] { 48, 51, 55, 51, 48, 51, 58, 51, 47, 50, 53, 50, 47, 50, 58, 50 },
                new[] { 36, 36, 36, -1, 35, 35, 35, -1, 34, 34, 34, -1, 35, 35, 35, -1 },
                0.34f,
                0.28f,
                0.16f);
        }

        private static AudioClip CreateLoop(string name, float bpm, int steps, int[] melody, int[] bass, float melodyVolume, float bassVolume, float drumVolume)
        {
            const int sampleRate = 22050;
            float stepSeconds = 60f / bpm;
            float duration = stepSeconds * steps;
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                int step = Mathf.Clamp(Mathf.FloorToInt(time / stepSeconds), 0, steps - 1);
                float localTime = time - step * stepSeconds;
                float localT = Mathf.Clamp01(localTime / stepSeconds);

                float sample = 0f;
                int melodyNote = melody[step % melody.Length];
                if (melodyNote >= 0)
                {
                    sample += Square(MidiToFrequency(melodyNote), time, 0.5f) * NoteEnvelope(localT) * melodyVolume;
                }

                int bassNote = bass[step % bass.Length];
                if (bassNote >= 0)
                {
                    sample += Square(MidiToFrequency(bassNote), time, 0.42f) * NoteEnvelope(localT) * bassVolume;
                }

                if (drumVolume > 0f)
                {
                    sample += Drum(step, localT, time) * drumVolume;
                }

                data[i] = Mathf.Clamp(sample, -0.85f, 0.85f);
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Square(float frequency, float time, float duty)
        {
            return Mathf.Repeat(time * frequency, 1f) < duty ? 1f : -1f;
        }

        private static float MidiToFrequency(int note)
        {
            return 440f * Mathf.Pow(2f, (note - 69) / 12f);
        }

        private static float NoteEnvelope(float t)
        {
            float attack = Mathf.Clamp01(t / 0.04f);
            float release = Mathf.Clamp01((1f - t) / 0.16f);
            return Mathf.Min(attack, release);
        }

        private static float Drum(int step, float localT, float time)
        {
            float kick = (step % 4 == 0) ? Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(120f, 58f, localT) * time) * Mathf.Pow(1f - localT, 6f) : 0f;
            float hat = (step % 2 == 1) ? Noise(time) * Mathf.Pow(1f - localT, 12f) * 0.45f : 0f;
            return kick + hat;
        }

        private static float Noise(float time)
        {
            float value = Mathf.Sin((time * 127.1f + 31.7f) * 43758.5453f);
            return Mathf.Repeat(value, 1f) * 2f - 1f;
        }
    }
}
