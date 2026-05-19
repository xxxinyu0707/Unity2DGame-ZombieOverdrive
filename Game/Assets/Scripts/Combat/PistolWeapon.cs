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
            int shots = IsEvolved ? 4 : Level >= 4 ? 2 : 1;
            int pierces = Stats.bulletPierceBonus + (Level >= 5 ? 2 : 0) + (IsEvolved ? 2 : 0);
            float damage = RollDamage(baseDamage * (1f + (Level - 1) * 0.16f) * (IsEvolved ? 1.35f : 1f));
            Vector3 origin = transform.position + (Vector3)(direction * 0.6f);
            Vector2 side = new Vector2(-direction.y, direction.x);
            CombatVisuals.SpawnMuzzleFlash(origin, direction, new Color(1f, 0.86f, 0.35f, 0.95f), 0.26f);

            for (int i = 0; i < shots; i++)
            {
                float offset = shots == 1 ? 0f : (i - (shots - 1) * 0.5f) * parallelShotOffset;
                float angle = IsEvolved ? (i - (shots - 1) * 0.5f) * 4f : 0f;
                SpawnBullet(origin + (Vector3)(side * offset), Rotate(direction, angle), damage, pierces);
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
            if (IsEvolved)
            {
                return 0.58f;
            }

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

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos).normalized;
        }
    }
}
