using UnityEngine;
using ZombieOverdrive.Enemies;

namespace ZombieOverdrive.Combat
{
    public class LightbladeWeapon : WeaponBase
    {
        [SerializeField] private float baseDamage = 52f;
        [SerializeField] private float baseCooldown = 0.9f;
        [SerializeField] private float baseRadius = 2.1f;
        [SerializeField] private float arcDegrees = 120f;
        [SerializeField] private LayerMask enemyMask;

        private readonly Collider2D[] hits = new Collider2D[64];
        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Lightblade;

        private void Update()
        {
            if (!IsUnlocked || Stats == null || Movement == null)
            {
                return;
            }

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Slash();
                cooldownTimer = baseCooldown / FireRateMultiplier * (Level >= 3 ? 0.78f : 1f);
            }
        }

        private void Slash()
        {
            float radius = baseRadius * AreaMultiplier * (Level >= 2 ? 1.25f : 1f);
            float arc = Level >= 5 ? 240f : arcDegrees;
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, hits, enemyMask);

            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = hits[i].GetComponent<EnemyHealth>();
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector2 toEnemy = enemy.transform.position - transform.position;
                if (Vector2.Angle(AimDirection, toEnemy) > arc * 0.5f)
                {
                    continue;
                }

                enemy.TakeDamage(RollDamage(baseDamage * (1f + (Level - 1) * 0.16f)));
                EnemyController controller = enemy.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.ApplyKnockback(toEnemy.normalized * 0.75f);
                }
            }
        }
    }
}
