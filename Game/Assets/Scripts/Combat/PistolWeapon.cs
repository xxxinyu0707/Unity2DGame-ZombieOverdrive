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
            int shots = IsEvolved ? 6 : Level >= 5 ? 2 : 1;
            int pierces = Stats.bulletPierceBonus + (Level >= 4 ? 1 : 0) + (Level >= 5 ? 1 : 0) + (IsEvolved ? 4 : 0);
            float damage = RollDamage(baseDamage * (1f + (Level - 1) * 0.14f) * (IsEvolved ? 1.45f : 1f));
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
                float angle = IsEvolved ? (i - (shots - 1) * 0.5f) * 7f : 0f;
                SpawnBullet(origin + (Vector3)(side * offset), Rotate(direction, angle), damage, pierces, IsEvolved);
            }
        }

        private void SpawnBullet(Vector3 position, Vector2 direction, float damage, int pierces, bool infinitePierce)
        {
            Bullet bullet = bulletPool.Get<Bullet>(position, Quaternion.identity);
            if (bullet != null)
            {
                bullet.Launch(direction, damage, pierces, IsEvolved ? 0.35f : 0.15f, Stats != null ? Stats.projectileSpeedMultiplier * (IsEvolved ? 1.25f : 1f) : 1f, infinitePierce);
            }
        }

        private float LevelCooldownFactor()
        {
            if (IsEvolved)
            {
                return 0.52f;
            }

            if (Level >= 5)
            {
                return 0.82f;
            }

            if (Level >= 2)
            {
                return 0.9f;
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
