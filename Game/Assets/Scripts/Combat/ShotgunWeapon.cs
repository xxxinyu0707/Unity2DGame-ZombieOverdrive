using UnityEngine;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Combat
{
    public class ShotgunWeapon : WeaponBase
    {
        [SerializeField] private GameObjectPool bulletPool;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float baseDamage = 8.5f;
        [SerializeField] private float baseCooldown = 1.85f;
        [SerializeField] private float spreadAngle = 58f;
        [SerializeField] private LayerMask enemyMask;

        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Shotgun;

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
                cooldownTimer = baseCooldown / FireRateMultiplier * (IsEvolved ? 0.5f : Level >= 4 ? 0.62f : 1f);
            }
        }

        private void Fire()
        {
            int pelletCount = IsEvolved ? 24 : Level >= 2 ? 7 : 5;

            int pierces = Level >= 5 ? 1 + Stats.bulletPierceBonus : Stats.bulletPierceBonus;
            if (IsEvolved)
            {
                pierces += 4;
            }

            Vector3 origin = transform.position + (Vector3)(AimDirection * 0.65f);
            CombatVisuals.SpawnMuzzleFlash(origin, AimDirection, IsEvolved ? new Color(1f, 0.22f, 0.16f, 1f) : new Color(1f, 0.56f, 0.16f, 0.95f), IsEvolved ? 0.72f : 0.48f);
            if (IsEvolved)
            {
                CombatVisuals.SpawnRing(origin, new Color(1f, 0.32f, 0.18f, 0.48f), 0.85f, 0.13f);
            }

            for (int i = 0; i < pelletCount; i++)
            {
                float spread = IsEvolved ? 360f : Level >= 2 ? spreadAngle * 0.85f : spreadAngle;
                float randomAngle = IsEvolved
                    ? i * (360f / pelletCount) + Random.Range(-5f, 5f)
                    : Random.Range(-spread * 0.5f, spread * 0.5f);
                Vector2 direction = Rotate(AimDirection, randomAngle);
                Bullet bullet = bulletPool.Get<Bullet>(origin, Quaternion.identity);
                if (bullet != null)
                {
                    float damage = RollDamage(baseDamage * (1f + (Level - 1) * 0.08f) * (IsEvolved ? 1.08f : 1f));
                    float knockback = IsEvolved ? 1.1f : Level >= 4 ? 0.72f : 0.45f;
                    bullet.Launch(direction, damage, pierces, knockback, Stats.projectileSpeedMultiplier * (IsEvolved ? 0.95f : 0.8f), false);
                    bullet.ConfigureVisual(
                        IsEvolved ? new Color(1f, 0.28f, 0.12f, 1f) : new Color(1f, 0.58f, 0.18f, 1f),
                        IsEvolved ? new Vector2(0.34f, 0.26f) : new Vector2(0.24f, 0.2f),
                        Random.Range(-16f, 16f));
                    if (Level >= 3)
                    {
                        bullet.ConfigureFirePatch(IsEvolved ? 0.9f : 0.48f, damage * (IsEvolved ? 1.45f : 0.95f), IsEvolved ? 2.4f : 1.2f, enemyMask);
                    }

                    if (IsEvolved)
                    {
                        bullet.ConfigureBurst(0.65f, 0.45f, enemyMask);
                    }
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
