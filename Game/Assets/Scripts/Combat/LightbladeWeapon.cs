using UnityEngine;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.World;

namespace ZombieOverdrive.Combat
{
    public class LightbladeWeapon : WeaponBase
    {
        [SerializeField] private float baseDamage = 44f;
        [SerializeField] private float baseCooldown = 1.02f;
        [SerializeField] private float baseRadius = 1.95f;
        [SerializeField] private float arcDegrees = 120f;
        [SerializeField] private LayerMask enemyMask;
        [SerializeField] private Sprite swordSprite;

        private readonly Collider2D[] hits = new Collider2D[64];
        private float cooldownTimer;
        private SpriteRenderer swordRenderer;
        private float slashVisualTimer;
        private float slashVisualDuration;
        private float slashStartAngle;
        private float slashArc;
        private float slashRadius;

        public override WeaponId Id => WeaponId.Lightblade;

        private void Update()
        {
            UpdateSwordVisual();

            if (!CanAttack)
            {
                return;
            }

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Slash();
                cooldownTimer = baseCooldown / FireRateMultiplier * (Level >= 3 ? 0.86f : 1f);
            }
        }

        protected override void OnInitialized()
        {
            GameObject swordObject = new GameObject("Lightblade Sword Visual");
            swordObject.transform.SetParent(transform, false);
            swordRenderer = swordObject.AddComponent<SpriteRenderer>();
            swordRenderer.sprite = swordSprite;
            swordRenderer.sortingOrder = 14;
            swordRenderer.enabled = false;
        }

        private void Slash()
        {
            float radius = baseRadius * AreaMultiplier * (Level >= 2 ? 1.2f : 1f);
            float arc = IsEvolved ? 360f : Level >= 5 ? 220f : arcDegrees;
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, hits, enemyMask);
            DrawSlash(radius, arc);
            ShowSwordSlash(radius, arc);

            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = hits[i].GetComponent<EnemyHealth>();
                DestructibleCrate crate = hits[i].GetComponent<DestructibleCrate>();
                if (crate != null)
                {
                    crate.TakeDamage(baseDamage);
                }

                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector2 toEnemy = enemy.transform.position - transform.position;
                if (Vector2.Angle(AimDirection, toEnemy) > arc * 0.5f)
                {
                    continue;
                }

                enemy.TakeDamage(RollDamage(baseDamage * (1f + (Level - 1) * 0.13f) * (IsEvolved ? 1.32f : 1f)));
                EnemyController controller = enemy.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.ApplyKnockback(toEnemy.normalized * 0.75f);
                }
            }
        }

        private void ShowSwordSlash(float radius, float arc)
        {
            if (swordRenderer == null)
            {
                return;
            }

            slashVisualDuration = IsEvolved ? 0.22f : 0.16f;
            slashVisualTimer = slashVisualDuration;
            slashArc = arc;
            slashRadius = radius;
            slashStartAngle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg - arc * 0.5f;
            swordRenderer.enabled = true;
            swordRenderer.color = IsEvolved ? new Color(1f, 0.45f, 0.55f, 0.95f) : new Color(0.9f, 1f, 1f, 0.95f);
        }

        private void UpdateSwordVisual()
        {
            if (swordRenderer == null || !swordRenderer.enabled)
            {
                return;
            }

            slashVisualTimer -= Time.deltaTime;
            if (slashVisualTimer <= 0f)
            {
                swordRenderer.enabled = false;
                return;
            }

            float t = 1f - slashVisualTimer / Mathf.Max(0.01f, slashVisualDuration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            float angle = slashStartAngle + slashArc * eased;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            swordRenderer.transform.position = transform.position + (Vector3)(direction * (slashRadius * 0.55f));
            swordRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle - 35f);
            swordRenderer.transform.localScale = Vector3.one * ((IsEvolved ? 1.2f : 0.85f) + slashRadius * 0.18f);
            Color color = IsEvolved ? new Color(1f, 0.45f, 0.55f, 1f) : new Color(0.9f, 1f, 1f, 1f);
            color.a = Mathf.Lerp(0.15f, 0.95f, slashVisualTimer / slashVisualDuration);
            swordRenderer.color = color;
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
