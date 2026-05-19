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
            EnemyHealth current = FindNearestEnemy(origin, 7f * AreaMultiplier, null);
            int maxTargets = Level >= 2 ? 5 : 3;
            if (Level >= 4)
            {
                maxTargets += 2;
            }

            for (int i = 0; i < maxTargets && current != null; i++)
            {
                float damage = RollDamage(baseDamage * (i == 0 && Level >= 2 ? 1.35f : 1f));
                current.TakeDamage(damage);
                EnemyController controller = current.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.ApplySlow(Level >= 3 ? 0.5f : 0.25f, 0.75f);
                }

                struck.Add(current);
                origin = current.transform.position;
                current = FindNearestEnemy(origin, chainRadius * AreaMultiplier, struck);
            }
        }

        private EnemyHealth FindNearestEnemy(Vector2 origin, float radius, HashSet<EnemyHealth> ignored)
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

                float distance = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
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
