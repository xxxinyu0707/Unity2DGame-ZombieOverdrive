using UnityEngine;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Combat
{
    public class PistolWeapon : WeaponBase
    {
        [SerializeField] private GameObjectPool bulletPool;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float baseDamage = 42f;
        [SerializeField] private float baseCooldown = 0.36f;
        [SerializeField] private float parallelShotOffset = 0.18f;

        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Pistol;

        private void Update()
        {
            if (Stats == null || Movement == null || bulletPool == null || !IsUnlocked)
            {
                return;
            }

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Fire();
                cooldownTimer = baseCooldown / FireRateMultiplier * LevelCooldownFactor();
            }
        }

        private void Fire()
        {
            Vector2 direction = AimDirection;
            int shots = Level >= 4 ? 2 : 1;
            int pierces = Stats.bulletPierceBonus + (Level >= 5 ? 2 : 0);
            float damage = RollDamage(baseDamage * (1f + (Level - 1) * 0.16f));
            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            Vector2 side = new Vector2(-direction.y, direction.x);

            for (int i = 0; i < shots; i++)
            {
                float offset = shots == 1 ? 0f : (i == 0 ? -parallelShotOffset : parallelShotOffset);
                SpawnBullet(origin + (Vector3)(side * offset), direction, damage, pierces);
            }
        }

        private void SpawnBullet(Vector3 position, Vector2 direction, float damage, int pierces)
        {
            Bullet bullet = bulletPool.Get<Bullet>(position, Quaternion.identity);
            if (bullet != null)
            {
                bullet.Launch(direction, damage, pierces, 0.15f, Stats != null ? Stats.projectileSpeedMultiplier : 1f, false);
            }
        }

        private float LevelCooldownFactor()
        {
            if (Level >= 4)
            {
                return 0.8f;
            }

            if (Level >= 2)
            {
                return 0.85f;
            }

            return 1f;
        }
    }
}
