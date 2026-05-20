using UnityEngine;
using ZombieOverdrive.Core;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.World;

namespace ZombieOverdrive.Combat
{
    public class DamageZone : MonoBehaviour
    {
        private enum TargetMode
        {
            Enemies,
            Player
        }

        private static readonly Collider2D[] Hits = new Collider2D[96];

        private TargetMode targetMode;
        private LayerMask targetMask;
        private float radius;
        private float damagePerSecond;
        private float duration;
        private float tickInterval;
        private float tickTimer;
        private float slowAmount;
        private float slowDuration;
        private LineRenderer ring;

        public static void SpawnEnemyZone(Vector3 position, float zoneRadius, float dps, float seconds, LayerMask mask, Color color, float slow = 0f)
        {
            DamageZone zone = Create(position, zoneRadius, dps, seconds, color);
            zone.targetMode = TargetMode.Enemies;
            zone.targetMask = mask;
            zone.slowAmount = slow;
            zone.slowDuration = 0.35f;
        }

        public static void SpawnPlayerZone(Vector3 position, float zoneRadius, float dps, float seconds, Color color)
        {
            DamageZone zone = Create(position, zoneRadius, dps, seconds, color);
            zone.targetMode = TargetMode.Player;
            zone.targetMask = default;
        }

        private static DamageZone Create(Vector3 position, float zoneRadius, float dps, float seconds, Color color)
        {
            GameObject zoneObject = new GameObject("Damage Zone");
            zoneObject.transform.position = position;
            DamageZone zone = zoneObject.AddComponent<DamageZone>();
            zone.radius = Mathf.Max(0.1f, zoneRadius);
            zone.damagePerSecond = Mathf.Max(0f, dps);
            zone.duration = Mathf.Max(0.05f, seconds);
            zone.tickInterval = 0.25f;
            zone.tickTimer = 0f;
            zone.CreateRing(color);
            return zone;
        }

        private void Update()
        {
            duration -= Time.deltaTime;
            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                tickTimer = tickInterval;
                TickDamage();
            }

            if (duration <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void TickDamage()
        {
            float damage = damagePerSecond * tickInterval;
            if (targetMode == TargetMode.Player)
            {
                DamagePlayer(damage);
            }
            else
            {
                DamageEnemies(damage);
            }
        }

        private void DamageEnemies(float damage)
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, Hits, targetMask);
            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = Hits[i].GetComponent<EnemyHealth>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.TakeDamage(damage);
                    EnemyController controller = Hits[i].GetComponent<EnemyController>();
                    if (controller != null)
                    {
                        controller.MarkBurning(0.45f);
                        if (slowAmount > 0f)
                        {
                            controller.ApplySlow(slowAmount, slowDuration);
                        }
                    }
                }

                DestructibleCrate crate = Hits[i].GetComponent<DestructibleCrate>();
                if (crate != null)
                {
                    crate.TakeDamage(damage);
                }
            }
        }

        private void DamagePlayer(float damage)
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, Hits);
            for (int i = 0; i < count; i++)
            {
                PlayerHealth player = Hits[i].GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    return;
                }
            }
        }

        private void CreateRing(Color color)
        {
            ring = gameObject.AddComponent<LineRenderer>();
            ring.material = new Material(Shader.Find("Sprites/Default"));
            ring.loop = true;
            ring.useWorldSpace = false;
            ring.positionCount = 32;
            ring.startColor = color;
            ring.endColor = new Color(color.r, color.g, color.b, Mathf.Min(color.a, 0.25f));
            ring.startWidth = 0.04f;
            ring.endWidth = 0.04f;
            ring.sortingOrder = 11;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = Mathf.PI * 2f * i / ring.positionCount;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            }
        }
    }
}
