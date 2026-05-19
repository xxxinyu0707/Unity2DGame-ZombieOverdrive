using System.Collections.Generic;
using UnityEngine;

namespace ZombieOverdrive.Audio
{
    public enum GameSound
    {
        Pickup,
        Gold,
        Heal,
        Magnet,
        Bomb,
        CrateBreak,
        Hit,
        LevelUp,
        Evolution,
        Menu,
        GameOver,
        Victory
    }

    [RequireComponent(typeof(AudioSource))]
    public class GameAudio : MonoBehaviour
    {
        private readonly Dictionary<GameSound, AudioClip> clips = new Dictionary<GameSound, AudioClip>();
        private AudioSource source;

        public static GameAudio Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0.55f;
            BuildClips();
        }

        public static void Play(GameSound sound, float volume = 1f)
        {
            if (Instance == null || Instance.source == null)
            {
                return;
            }

            if (Instance.clips.TryGetValue(sound, out AudioClip clip) && clip != null)
            {
                Instance.source.PlayOneShot(clip, Mathf.Clamp01(volume));
            }
        }

        private void BuildClips()
        {
            clips[GameSound.Pickup] = CreateTone("pickup", 660f, 0.055f, Wave.Sine, 0.5f, 1040f);
            clips[GameSound.Gold] = CreateTone("gold", 780f, 0.08f, Wave.Sine, 0.55f, 1240f);
            clips[GameSound.Heal] = CreateTone("heal", 440f, 0.16f, Wave.Sine, 0.55f, 660f);
            clips[GameSound.Magnet] = CreateTone("magnet", 260f, 0.22f, Wave.Sine, 0.45f, 560f);
            clips[GameSound.Bomb] = CreateNoise("bomb", 0.34f, 0.65f);
            clips[GameSound.CrateBreak] = CreateNoise("crate_break", 0.18f, 0.35f);
            clips[GameSound.Hit] = CreateNoise("hit", 0.08f, 0.24f);
            clips[GameSound.LevelUp] = CreateArpeggio("level_up", new[] { 523f, 659f, 784f, 1046f }, 0.26f, 0.62f);
            clips[GameSound.Evolution] = CreateArpeggio("evolution", new[] { 392f, 587f, 784f, 1174f, 1568f }, 0.42f, 0.7f);
            clips[GameSound.Menu] = CreateTone("menu", 520f, 0.08f, Wave.Triangle, 0.38f, 620f);
            clips[GameSound.GameOver] = CreateArpeggio("game_over", new[] { 330f, 247f, 196f }, 0.32f, 0.5f);
            clips[GameSound.Victory] = CreateArpeggio("victory", new[] { 523f, 659f, 784f, 1046f, 1318f }, 0.42f, 0.65f);
        }

        private enum Wave
        {
            Sine,
            Triangle
        }

        private static AudioClip CreateTone(string name, float startFrequency, float duration, Wave wave, float volume, float endFrequency)
        {
            const int sampleRate = 22050;
            int samples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[samples];
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                phase += frequency / sampleRate;
                float raw = wave == Wave.Triangle
                    ? 1f - 4f * Mathf.Abs(Mathf.Round(phase - 0.25f) - (phase - 0.25f))
                    : Mathf.Sin(phase * Mathf.PI * 2f);
                data[i] = raw * volume * Envelope(t);
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateArpeggio(string name, float[] frequencies, float duration, float volume)
        {
            const int sampleRate = 22050;
            int samples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[samples];
            int noteCount = Mathf.Max(1, frequencies.Length);

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                int note = Mathf.Clamp(Mathf.FloorToInt(t * noteCount), 0, noteCount - 1);
                float localT = Mathf.Repeat(t * noteCount, 1f);
                float sine = Mathf.Sin(2f * Mathf.PI * frequencies[note] * i / sampleRate);
                data[i] = sine * volume * Envelope(localT) * (1f - t * 0.28f);
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateNoise(string name, float duration, float volume)
        {
            const int sampleRate = 22050;
            int samples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[samples];
            uint seed = 2166136261u;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                seed = seed * 16777619u + 1013904223u;
                float random = ((seed >> 8) & 0xffff) / 32768f - 1f;
                data[i] = random * volume * Mathf.Pow(1f - t, 1.8f);
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Envelope(float t)
        {
            float attack = Mathf.Clamp01(t / 0.08f);
            float release = Mathf.Clamp01((1f - t) / 0.18f);
            return Mathf.Min(attack, release);
        }
    }
}
