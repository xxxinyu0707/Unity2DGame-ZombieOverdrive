using UnityEngine;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Combat
{
    public class PistolWeapon : WeaponBase
    {
        [SerializeField] private GameObjectPool bulletPool;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float baseDamage = 38f;
        [SerializeField] private float baseCooldown = 0.42f;
        [SerializeField] private float parallelShotOffset = 0.18f;
        [SerializeField] private LayerMask enemyMask;

        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Pistol;

        private void Update()
        {
            if (!CanAttack || bulletPool == null)
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
            int shots = IsEvolved ? 8 : Level >= 4 ? 2 : 1;
            int pierces = Stats.bulletPierceBonus + (Level >= 5 ? 2 : 0) + (IsEvolved ? 6 : 0);
            float levelDamage = Level >= 2 ? 1.15f : 1f;
            float damage = RollDamage(baseDamage * levelDamage * (1f + Mathf.Max(0, Level - 2) * 0.08f) * (IsEvolved ? 1.55f : 1f));
            Vector3 origin = transform.position + (Vector3)(direction * 0.6f);
            Vector2 side = new Vector2(-direction.y, direction.x);
            CombatVisuals.SpawnMuzzleFlash(origin, direction, IsEvolved ? new Color(0.55f, 0.9f, 1f, 1f) : new Color(1f, 0.86f, 0.35f, 0.95f), IsEvolved ? 0.42f : 0.26f);
            if (IsEvolved)
            {
                CombatVisuals.SpawnRing(origin, new Color(0.45f, 0.9f, 1f, 0.5f), 0.55f, 0.12f);
            }

            for (int i = 0; i < shots; i++)
            {
                float offset = shots == 1 ? 0f : (i - (shots - 1) * 0.5f) * parallelShotOffset;
                float angle = IsEvolved ? (i - (shots - 1) * 0.5f) * 6f : 0f;
                SpawnBullet(origin + (Vector3)(side * offset), Rotate(direction, angle), damage, pierces, IsEvolved);
            }
        }

        private void SpawnBullet(Vector3 position, Vector2 direction, float damage, int pierces, bool infinitePierce)
        {
            Bullet bullet = bulletPool.Get<Bullet>(position, Quaternion.identity);
            if (bullet != null)
            {
                bullet.Launch(direction, damage, pierces, IsEvolved ? 0.35f : 0.15f, Stats != null ? Stats.projectileSpeedMultiplier * (IsEvolved ? 1.25f : 1f) : 1f, infinitePierce);
                if (Level >= 3 && !IsEvolved)
                {
                    bullet.ConfigureSplit(bulletPool, 2, 45f, 0.5f);
                }

                if (Level >= 5)
                {
                    bullet.ConfigureImpairedBonus(2f);
                }

                if (IsEvolved)
                {
                    bullet.ConfigureSplit(bulletPool, 2, 28f, 0.45f);
                    bullet.ConfigureBurst(0.42f, 0.38f, enemyMask);
                    bullet.ConfigureImpairedBonus(2.2f);
                }
            }
        }

        private float LevelCooldownFactor()
        {
            if (IsEvolved)
            {
                return 0.42f;
            }

            if (Level >= 4)
            {
                return 0.72f;
            }

            if (Level >= 2)
            {
                return 0.78f;
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
