using UnityEngine;
using ZombieOverdrive.Enemies;

namespace ZombieOverdrive.Combat
{
    public class LaserWeapon : WeaponBase
    {
        [SerializeField] private float baseDamagePerSecond = 32f;
        [SerializeField] private float range = 10f;
        [SerializeField] private float beamWidth = 0.45f;
        [SerializeField] private LayerMask enemyMask;

        private readonly RaycastHit2D[] hits = new RaycastHit2D[64];
        private LineRenderer beam;
        private LineRenderer coreBeam;
        private LineRenderer[] pulseLines;

        public override WeaponId Id => WeaponId.Laser;

        private void Update()
        {
            if (!IsUnlocked || Stats == null || Movement == null)
            {
                if (beam != null)
                {
                    beam.enabled = false;
                }

                return;
            }

            FireBeam();
        }

        protected override void OnInitialized()
        {
            beam = CreateBeamLine("Laser Outer Beam");
            ConfigureBeam(beam, new Color(1f, 0.24f, 0.1f, 0.45f), new Color(1f, 0.95f, 0.25f, 0.08f), 0.34f, 0.2f, 12);
            coreBeam = CreateBeamLine("Laser Core Beam");
            ConfigureBeam(coreBeam, new Color(1f, 0.95f, 0.55f, 0.95f), new Color(1f, 0.25f, 0.08f, 0.35f), 0.08f, 0.04f, 13);
            pulseLines = new LineRenderer[3];
            for (int i = 0; i < pulseLines.Length; i++)
            {
                pulseLines[i] = CreateBeamLine("Laser Pulse " + (i + 1));
                ConfigureBeam(pulseLines[i], new Color(1f, 0.8f, 0.28f, 0.55f), new Color(1f, 0.2f, 0.05f, 0.02f), 0.035f, 0.015f, 14);
            }

            beam.enabled = false;
            coreBeam.enabled = false;
            SetPulsesEnabled(false);
        }

        private LineRenderer CreateBeamLine(string lineName)
        {
            GameObject lineObject = new GameObject(lineName);
            lineObject.transform.SetParent(transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            return line;
        }

        private void FireBeam()
        {
            float width = beamWidth * AreaMultiplier * (Level >= 2 ? 1.35f : 1f);
            int count = Physics2D.CircleCastNonAlloc(transform.position, width, AimDirection, hits, range, enemyMask);
            float damage = RollDamage(baseDamagePerSecond * (1f + (Level - 1) * 0.18f)) * Time.deltaTime;
            if (beam != null)
            {
                beam.enabled = true;
                coreBeam.enabled = true;
                Vector3 start = transform.position + (Vector3)(AimDirection * 0.55f);
                Vector3 end = transform.position + (Vector3)(AimDirection * range);
                float levelScale = (Level >= 2 ? 1.25f : 1f) * (IsEvolved ? 1.45f : 1f);
                SetAnimatedBeam(beam, start, end, 9, Time.time * 18f, 0.08f * levelScale);
                SetAnimatedBeam(coreBeam, start, end, 8, Time.time * 26f + 1.5f, 0.025f);
                beam.startWidth = (0.24f + Mathf.PingPong(Time.time * 1.8f, 0.18f)) * levelScale;
                beam.endWidth = 0.08f * levelScale;
                coreBeam.startWidth = (0.05f + Mathf.PingPong(Time.time * 4f, 0.05f)) * levelScale;
                coreBeam.endWidth = 0.025f * levelScale;
                UpdatePulseLines(start, end, levelScale);
            }

            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = hits[i].collider.GetComponent<EnemyHealth>();
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                enemy.TakeDamage(damage);
            }
        }

        private void LateUpdate()
        {
            if (!IsUnlocked && beam != null)
            {
                beam.enabled = false;
                if (coreBeam != null)
                {
                    coreBeam.enabled = false;
                }

                SetPulsesEnabled(false);
            }
        }

        private void UpdatePulseLines(Vector3 start, Vector3 end, float levelScale)
        {
            SetPulsesEnabled(true);
            Vector3 direction = (end - start).normalized;
            Vector3 normal = new Vector3(-direction.y, direction.x, 0f);
            float length = Vector3.Distance(start, end);
            for (int i = 0; i < pulseLines.Length; i++)
            {
                float travel = Mathf.Repeat(Time.time * (4.2f + i * 0.55f) + i * 0.28f, 1f);
                float segmentStart = Mathf.Clamp01(travel - 0.11f);
                float segmentEnd = Mathf.Clamp01(travel + 0.06f);
                Vector3 offset = normal * Mathf.Sin((Time.time * 9f + i) * 1.7f) * 0.05f * levelScale;
                pulseLines[i].SetPosition(0, start + direction * (length * segmentStart) + offset);
                pulseLines[i].SetPosition(1, start + direction * (length * segmentEnd) - offset);
                pulseLines[i].startWidth = 0.035f * levelScale;
                pulseLines[i].endWidth = 0.01f * levelScale;
            }
        }

        private void SetPulsesEnabled(bool enabled)
        {
            if (pulseLines == null)
            {
                return;
            }

            for (int i = 0; i < pulseLines.Length; i++)
            {
                if (pulseLines[i] != null)
                {
                    pulseLines[i].enabled = enabled;
                }
            }
        }

        private static void ConfigureBeam(LineRenderer line, Color startColor, Color endColor, float startWidth, float endWidth, int sortingOrder)
        {
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startColor = startColor;
            line.endColor = endColor;
            line.startWidth = startWidth;
            line.endWidth = endWidth;
            line.sortingOrder = sortingOrder;
            line.enabled = false;
        }

        private static void SetAnimatedBeam(LineRenderer line, Vector3 start, Vector3 end, int points, float phase, float wobble)
        {
            line.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 normal = new Vector3(-direction.y, direction.x, 0f);
            for (int i = 0; i < points; i++)
            {
                float t = i / (float)(points - 1);
                float edgeFade = Mathf.Sin(t * Mathf.PI);
                float wave = Mathf.Sin(phase + t * Mathf.PI * 6f) * wobble * edgeFade;
                line.SetPosition(i, Vector3.Lerp(start, end, t) + normal * wave);
            }
        }
    }
}
