using System.Collections.Generic;
using UnityEngine;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.World;

namespace ZombieOverdrive.Combat
{
    public class TeslaWeapon : WeaponBase
    {
        [SerializeField] private float baseDamage = 28f;
        [SerializeField] private float baseCooldown = 0.98f;
        [SerializeField] private float chainRadius = 3.75f;
        [SerializeField] private LayerMask enemyMask;

        private readonly Collider2D[] hits = new Collider2D[32];
        private readonly Collider2D[] crateHits = new Collider2D[32];
        private readonly HashSet<EnemyHealth> struck = new HashSet<EnemyHealth>();
        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Tesla;

        private void Update()
        {
            if (!CanAttack)
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
            int primaryBolts = IsEvolved ? 2 : 1;
            for (int bolt = 0; bolt < primaryBolts; bolt++)
            {
                FireSingleChain(bolt);
            }

            ShockNearbyCrates(transform.position, chainRadius * AreaMultiplier * (IsEvolved ? 1.5f : 1f));
            if (IsEvolved)
            {
                CombatVisuals.SpawnRing(transform.position, new Color(0.48f, 0.92f, 1f, 0.6f), chainRadius * AreaMultiplier * 0.55f, 0.18f);
            }
        }

        private void FireSingleChain(int boltIndex)
        {
            Vector2 origin = transform.position;
            bool requireCone = !IsEvolved || boltIndex == 0;
            EnemyHealth current = FindNearestEnemy(origin, (IsEvolved ? 8.5f : 7f) * AreaMultiplier, struck, requireCone);
            int maxTargets = IsEvolved ? 12 : Level >= 2 ? 5 : 3;
            if (Level >= 4)
            {
                maxTargets += IsEvolved ? 3 : 2;
            }

            for (int i = 0; i < maxTargets && current != null; i++)
            {
                float damage = RollDamage(baseDamage * (1f + (Level - 1) * 0.16f) * (i == 0 && Level >= 2 ? 1.35f : 1f) * (IsEvolved ? 1.48f : 1f));
                current.TakeDamage(damage);
                EnemyController controller = current.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.ApplySlow(IsEvolved ? 0.65f : Level >= 3 ? 0.5f : 0.25f, IsEvolved ? 1.15f : 0.75f);
                    if (Level >= 3 && Random.value < (IsEvolved ? 0.45f : 0.15f))
                    {
                        controller.ApplyStun(IsEvolved ? 0.42f : 0.22f);
                    }
                }

                DrawArc(origin, current.transform.position, IsEvolved);
                struck.Add(current);
                if (IsEvolved)
                {
                    CombatVisuals.SpawnRing(current.transform.position, new Color(0.36f, 0.95f, 1f, 0.38f), 0.32f + i * 0.018f, 0.1f);
                }

                if ((IsEvolved || Level >= 4) && Random.value < (IsEvolved ? 0.82f : 0.3f))
                {
                    EnemyHealth fork = FindNearestEnemy(current.transform.position, chainRadius * AreaMultiplier * 1.15f, struck, false);
                    if (fork != null)
                    {
                        fork.TakeDamage(damage * 0.72f);
                        DrawArc(current.transform.position, fork.transform.position, true);
                        EnemyController forkController = fork.GetComponent<EnemyController>();
                        if (forkController != null)
                        {
                            forkController.ApplySlow(0.45f, 0.8f);
                        }
                    }
                }

                if (Level >= 5 && !current.IsAlive)
                {
                    ChainBurst(current.transform.position, damage * (IsEvolved ? 0.85f : 0.45f));
                }

                origin = current.transform.position;
                current = FindNearestEnemy(origin, chainRadius * AreaMultiplier * (IsEvolved ? 1.25f : 1f), struck, false);
            }
        }

        private void ChainBurst(Vector3 center, float damage)
        {
            CombatVisuals.SpawnExplosion(center, new Color(0.35f, 0.95f, 1f, 0.55f), IsEvolved ? 0.82f : 0.48f, 0.12f);
            int count = Physics2D.OverlapCircleNonAlloc(center, IsEvolved ? 1.25f : 0.75f, hits, enemyMask);
            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = hits[i].GetComponent<EnemyHealth>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.TakeDamage(damage);
                    EnemyController controller = enemy.GetComponent<EnemyController>();
                    if (controller != null)
                    {
                        controller.ApplySlow(0.35f, 0.5f);
                    }
                }
            }
        }

        private void ShockNearbyCrates(Vector2 origin, float radius)
        {
            int count = Physics2D.OverlapCircleNonAlloc(origin, radius, crateHits, enemyMask);
            for (int i = 0; i < count; i++)
            {
                DestructibleCrate crate = crateHits[i].GetComponent<DestructibleCrate>();
                if (crate != null)
                {
                    crate.TakeDamage(baseDamage * (IsEvolved ? 1.35f : 0.9f));
                }
            }
        }

        private static void DrawArc(Vector3 start, Vector3 end, bool evolved)
        {
            int points = evolved ? 10 : 7;
            LineRenderer line = CombatVisuals.CreateTransientPolyline(
                evolved ? "雷暴链弧" : "Tesla Arc",
                points,
                evolved ? new Color(0.95f, 1f, 1f, 1f) : new Color(0.55f, 1f, 1f, 1f),
                evolved ? new Color(0.2f, 0.65f, 1f, 0.62f) : new Color(0.1f, 0.55f, 1f, 0.35f),
                evolved ? 0.2f : 0.12f,
                evolved ? 0.18f : 0.13f);

            Vector3 delta = end - start;
            Vector3 normal = new Vector3(-delta.y, delta.x, 0f).normalized;
            for (int i = 0; i < points; i++)
            {
                float t = points <= 1 ? 0f : i / (float)(points - 1);
                float jitter = i == 0 || i == points - 1 ? 0f : Random.Range(evolved ? -0.32f : -0.18f, evolved ? 0.32f : 0.18f);
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
