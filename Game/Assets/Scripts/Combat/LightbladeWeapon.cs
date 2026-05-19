using UnityEngine;
using ZombieOverdrive.Enemies;

namespace ZombieOverdrive.Combat
{
    public class LightbladeWeapon : WeaponBase
    {
        [SerializeField] private float baseDamage = 52f;
        [SerializeField] private float baseCooldown = 0.9f;
        [SerializeField] private float baseRadius = 2.1f;
        [SerializeField] private float arcDegrees = 120f;
        [SerializeField] private LayerMask enemyMask;

        private readonly Collider2D[] hits = new Collider2D[64];
        private float cooldownTimer;

        public override WeaponId Id => WeaponId.Lightblade;

        private void Update()
        {
            if (!IsUnlocked || Stats == null || Movement == null)
            {
                return;
            }

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Slash();
                cooldownTimer = baseCooldown / FireRateMultiplier * (Level >= 3 ? 0.78f : 1f);
            }
        }

        private void Slash()
        {
            float radius = baseRadius * AreaMultiplier * (Level >= 2 ? 1.25f : 1f);
            float arc = Level >= 5 ? 240f : arcDegrees;
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, hits, enemyMask);
            DrawSlash(radius, arc);

            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = hits[i].GetComponent<EnemyHealth>();
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector2 toEnemy = enemy.transform.position - transform.position;
                if (Vector2.Angle(AimDirection, toEnemy) > arc * 0.5f)
                {
                    continue;
                }

                enemy.TakeDamage(RollDamage(baseDamage * (1f + (Level - 1) * 0.16f)));
                EnemyController controller = enemy.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.ApplyKnockback(toEnemy.normalized * 0.75f);
                }
            }
        }

        private void DrawSlash(float radius, float arc)
        {
            int segments = 16;
            GameObject lineObject = new GameObject("Lightblade Slash");
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.positionCount = segments + 1;
            line.startColor = new Color(0.75f, 1f, 1f, 0.9f);
            line.endColor = new Color(0.75f, 1f, 1f, 0.25f);
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            line.sortingOrder = 9;

            float baseAngle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg - arc * 0.5f;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (baseAngle + arc * i / segments) * Mathf.Deg2Rad;
                Vector3 point = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                line.SetPosition(i, point);
            }

            Destroy(lineObject, 0.12f);
        }
    }
}
