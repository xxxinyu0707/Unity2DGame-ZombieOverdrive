using UnityEngine;

namespace ZombieOverdrive.Combat
{
    public class SingularityWeapon : WeaponBase
    {
        [SerializeField] private GameObject orbPrefab;
        [SerializeField] private float baseDamage = 12f;
        [SerializeField] private float baseCooldown = 4.2f;
        [SerializeField] private float baseRadius = 2.6f;
        [SerializeField] private LayerMask enemyMask;

        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Singularity;

        private void Update()
        {
            if (!IsUnlocked || Stats == null || Movement == null || orbPrefab == null)
            {
                return;
            }

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                FireOrb();
                cooldownTimer = baseCooldown / FireRateMultiplier;
            }
        }

        private void FireOrb()
        {
            int count = Level >= 5 ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                Vector2 direction = Rotate(AimDirection, Random.Range(-8f, 8f));
                GameObject orbObject = Instantiate(orbPrefab, transform.position, Quaternion.identity);
                SingularityOrb orb = orbObject.GetComponent<SingularityOrb>();
                if (orb != null)
                {
                    float radius = baseRadius * AreaMultiplier * (Level >= 2 ? 1.3f : 1f);
                    float lifetime = (Level >= 4 ? 4.8f : 3.5f) * (Stats != null ? Stats.durationMultiplier : 1f);
                    orb.Launch(direction, RollDamage(baseDamage * (1f + (Level - 1) * 0.2f)), radius, 0.45f, lifetime, enemyMask);
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
