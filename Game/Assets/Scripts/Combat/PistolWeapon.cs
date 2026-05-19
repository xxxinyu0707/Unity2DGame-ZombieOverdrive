using UnityEngine;
using ZombieOverdrive.Core;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Combat
{
    public class PistolWeapon : MonoBehaviour
    {
        [SerializeField] private GameObjectPool bulletPool;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float baseDamage = 30f;
        [SerializeField] private float baseCooldown = 0.45f;
        [SerializeField] private float parallelShotOffset = 0.18f;

        private PlayerStats stats;
        private PlayerMovement movement;
        private float cooldownTimer;

        public int Level { get; private set; } = 1;

        public void Initialize(PlayerStats playerStats, PlayerMovement playerMovement)
        {
            stats = playerStats;
            movement = playerMovement;
        }

        public void AddLevel()
        {
            Level = Mathf.Min(5, Level + 1);
        }

        private void Update()
        {
            if (stats == null || movement == null || bulletPool == null)
            {
                return;
            }

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Fire();
                cooldownTimer = baseCooldown / Mathf.Max(0.1f, stats.fireRateMultiplier) * LevelCooldownFactor();
            }
        }

        private void Fire()
        {
            Vector2 direction = movement.AimDirection;
            int shots = Level >= 4 ? 2 : 1;
            int pierces = stats.bulletPierceBonus + (Level >= 5 ? 2 : 0);
            float damage = baseDamage * stats.damageMultiplier * (1f + (Level - 1) * 0.12f);
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
                bullet.Launch(direction, damage, pierces);
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
