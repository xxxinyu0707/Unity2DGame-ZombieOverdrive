using UnityEngine;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Combat
{
    public class ShotgunWeapon : WeaponBase
    {
        [SerializeField] private GameObjectPool bulletPool;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float baseDamage = 11f;
        [SerializeField] private float baseCooldown = 1.55f;
        [SerializeField] private float spreadAngle = 52f;

        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Shotgun;

        private void Update()
        {
            if (!IsUnlocked || Stats == null || Movement == null || bulletPool == null)
            {
                return;
            }

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Fire();
                cooldownTimer = baseCooldown / FireRateMultiplier * (Level >= 4 ? 0.68f : 1f);
            }
        }

        private void Fire()
        {
            int pelletCount = Level >= 2 ? 6 : 5;
            if (Level >= 5)
            {
                pelletCount += 1;
            }

            int pierces = Level >= 5 ? 1 + Stats.bulletPierceBonus : Stats.bulletPierceBonus;
            Vector3 origin = transform.position + (Vector3)(AimDirection * 0.65f);

            for (int i = 0; i < pelletCount; i++)
            {
                float randomAngle = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
                Vector2 direction = Rotate(AimDirection, randomAngle);
                Bullet bullet = bulletPool.Get<Bullet>(origin, Quaternion.identity);
                if (bullet != null)
                {
                    float damage = RollDamage(baseDamage * (1f + (Level - 1) * 0.1f));
                    bullet.Launch(direction, damage, pierces, 0.45f, Stats.projectileSpeedMultiplier * 0.8f, false);
                }
            }
        }

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos).normalized;
        }
    }
}
