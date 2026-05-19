using UnityEngine;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Combat
{
    public class ShotgunWeapon : WeaponBase
    {
        [SerializeField] private GameObjectPool bulletPool;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float baseDamage = 24f;
        [SerializeField] private float baseCooldown = 1.25f;
        [SerializeField] private float spreadAngle = 42f;

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
            int pelletCount = Level >= 2 ? 7 : 5;
            if (Level >= 5)
            {
                pelletCount += 2;
            }

            int pierces = Level >= 5 ? 1 + Stats.bulletPierceBonus : Stats.bulletPierceBonus;
            float angleStep = pelletCount <= 1 ? 0f : spreadAngle / (pelletCount - 1);
            float startAngle = -spreadAngle * 0.5f;
            Vector3 origin = muzzle != null ? muzzle.position : transform.position;

            for (int i = 0; i < pelletCount; i++)
            {
                Vector2 direction = Rotate(AimDirection, startAngle + angleStep * i);
                Bullet bullet = bulletPool.Get<Bullet>(origin, Quaternion.identity);
                if (bullet != null)
                {
                    float damage = RollDamage(baseDamage * (1f + (Level - 1) * 0.12f));
                    bullet.Launch(direction, damage, pierces, 0.7f, Stats.projectileSpeedMultiplier * 0.85f, false);
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
