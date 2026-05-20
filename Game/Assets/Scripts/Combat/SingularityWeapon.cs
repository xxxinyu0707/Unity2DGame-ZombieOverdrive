using UnityEngine;

namespace ZombieOverdrive.Combat
{
    public class SingularityWeapon : WeaponBase
    {
        [SerializeField] private GameObject orbPrefab;
        [SerializeField] private float baseDamage = 10.5f;
        [SerializeField] private float baseCooldown = 4.6f;
        [SerializeField] private float baseRadius = 2.6f;
        [SerializeField] private LayerMask enemyMask;

        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Singularity;

        private void Update()
        {
            if (!CanAttack || orbPrefab == null)
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
            int count = IsEvolved ? 2 : Level >= 5 ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                Vector2 direction = Rotate(AimDirection, Random.Range(-8f, 8f));
                CombatVisuals.SpawnRing(transform.position + (Vector3)(direction * 0.45f), new Color(0.55f, 0.35f, 1f, 0.8f), 0.35f, 0.18f);
                GameObject orbObject = Instantiate(orbPrefab, transform.position, Quaternion.identity);
                SingularityOrb orb = orbObject.GetComponent<SingularityOrb>();
                if (orb != null)
                {
                    float radius = baseRadius * AreaMultiplier * (Level >= 2 ? 1.25f : 1f) * (IsEvolved ? 1.38f : 1f);
                    float lifetime = (Level >= 4 ? 4.4f : 3.35f) * (Stats != null ? Stats.durationMultiplier : 1f) * (IsEvolved ? 1.2f : 1f);
                    orb.Launch(direction, RollDamage(baseDamage * (1f + (Level - 1) * 0.16f) * (IsEvolved ? 1.45f : 1f)), radius, IsEvolved ? 0.7f : 0.42f, lifetime, enemyMask);
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
