using System.Collections.Generic;
using UnityEngine;
using ZombieOverdrive.Enemies;

namespace ZombieOverdrive.Combat
{
    public class TeslaWeapon : WeaponBase
    {
        [SerializeField] private float baseDamage = 28f;
        [SerializeField] private float baseCooldown = 1f;
        [SerializeField] private float chainRadius = 3.5f;
        [SerializeField] private LayerMask enemyMask;

        private readonly Collider2D[] hits = new Collider2D[32];
        private readonly HashSet<EnemyHealth> struck = new HashSet<EnemyHealth>();
        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Tesla;

        private void Update()
        {
            if (!IsUnlocked || Stats == null || Movement == null)
            {
                return;
            }

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                FireChain();
                cooldownTimer = baseCooldown / FireRateMultiplier;
            }
        }

        private void FireChain()
        {
            struck.Clear();
            Vector2 origin = transform.position;
            EnemyHealth current = FindNearestEnemy(origin, 7f * AreaMultiplier, null, true);
            int maxTargets = IsEvolved ? 10 : Level >= 2 ? 5 : 3;
            if (Level >= 4)
            {
                maxTargets += 2;
            }

            for (int i = 0; i < maxTargets && current != null; i++)
            {
                float damage = RollDamage(baseDamage * (i == 0 && Level >= 2 ? 1.35f : 1f) * (IsEvolved ? 1.18f : 1f));
                current.TakeDamage(damage);
                EnemyController controller = current.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.ApplySlow(Level >= 3 ? 0.5f : 0.25f, 0.75f);
                }

                DrawArc(origin, current.transform.position);
                if (IsEvolved && i > 0 && Random.value < 0.45f)
                {
                    EnemyHealth fork = FindNearestEnemy(current.transform.position, chainRadius * AreaMultiplier * 0.9f, struck, false);
                    if (fork != null)
                    {
                        fork.TakeDamage(damage * 0.55f);
                        DrawArc(current.transform.position, fork.transform.position);
                    }
                }

                struck.Add(current);
                origin = current.transform.position;
                current = FindNearestEnemy(origin, chainRadius * AreaMultiplier, struck, false);
            }
        }

        private static void DrawArc(Vector3 start, Vector3 end)
        {
            const int points = 7;
            LineRenderer line = CombatVisuals.CreateTransientPolyline(
                "Tesla Arc",
                points,
                new Color(0.55f, 1f, 1f, 1f),
                new Color(0.1f, 0.55f, 1f, 0.35f),
                0.07f,
                0.09f);

            Vector3 delta = end - start;
            Vector3 normal = new Vector3(-delta.y, delta.x, 0f).normalized;
            for (int i = 0; i < points; i++)
            {
                float t = points <= 1 ? 0f : i / (float)(points - 1);
                float jitter = i == 0 || i == points - 1 ? 0f : Random.Range(-0.18f, 0.18f);
                line.SetPosition(i, Vector3.Lerp(start, end, t) + normal * jitter);
            }
        }

        private EnemyHealth FindNearestEnemy(Vector2 origin, float radius, HashSet<EnemyHealth> ignored, bool requireAimCone)
        {
            int count = Physics2D.OverlapCircleNonAlloc(origin, radius, hits, enemyMask);
            EnemyHealth nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = hits[i].GetComponent<EnemyHealth>();
                if (enemy == null || !enemy.IsAlive || ignored != null && ignored.Contains(enemy))
                {
                    continue;
                }

                Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
                if (requireAimCone && Vector2.Angle(AimDirection, toEnemy) > 80f)
                {
                    continue;
                }

                float distance = toEnemy.sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }
    }
}
