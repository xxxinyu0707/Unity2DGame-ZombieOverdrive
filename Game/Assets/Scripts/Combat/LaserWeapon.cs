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

        public override WeaponId Id => WeaponId.Laser;

        private void Update()
        {
            if (!IsUnlocked || Stats == null || Movement == null)
            {
                return;
            }

            FireBeam();
        }

        private void FireBeam()
        {
            float width = beamWidth * AreaMultiplier * (Level >= 2 ? 1.35f : 1f);
            int count = Physics2D.CircleCastNonAlloc(transform.position, width, AimDirection, hits, range, enemyMask);
            float damage = RollDamage(baseDamagePerSecond * (1f + (Level - 1) * 0.18f)) * Time.deltaTime;

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
    }
}
