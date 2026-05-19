using UnityEngine;

namespace ZombieOverdrive.Enemies
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyDamageVisual : MonoBehaviour
    {
        [SerializeField] private Sprite healthySprite;
        [SerializeField] private Sprite woundedSprite;
        [SerializeField] private Sprite criticalSprite;
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.42f, 0.42f, 1f);

        private SpriteRenderer spriteRenderer;
        private EnemyHealth health;
        private Color baseColor;
        private float flashTimer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            health = GetComponent<EnemyHealth>();
            baseColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }

            health.HealthChanged += OnHealthChanged;
            ResetVisual();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
            }
        }

        public void Configure(Sprite healthy, Sprite wounded, Sprite critical)
        {
            healthySprite = healthy;
            woundedSprite = wounded != null ? wounded : healthy;
            criticalSprite = critical != null ? critical : woundedSprite;
            ResetVisual();
        }

        private void Update()
        {
            if (flashTimer <= 0f || spriteRenderer == null)
            {
                return;
            }

            flashTimer -= Time.deltaTime;
            spriteRenderer.color = flashTimer > 0f ? Color.Lerp(baseColor, hitFlashColor, flashTimer / 0.08f) : baseColor;
        }

        private void OnHealthChanged(float current, float max)
        {
            if (spriteRenderer == null || max <= 0f)
            {
                return;
            }

            float ratio = current / max;
            if (ratio <= 0.34f && criticalSprite != null)
            {
                spriteRenderer.sprite = criticalSprite;
            }
            else if (ratio <= 0.67f && woundedSprite != null)
            {
                spriteRenderer.sprite = woundedSprite;
            }
            else if (healthySprite != null)
            {
                spriteRenderer.sprite = healthySprite;
            }

            flashTimer = 0.08f;
        }

        private void ResetVisual()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            baseColor = spriteRenderer.color;
            if (healthySprite != null)
            {
                spriteRenderer.sprite = healthySprite;
            }

            spriteRenderer.color = baseColor;
            flashTimer = 0f;
        }
    }
}
