using UnityEngine;
using ZombieOverdrive.Core;

namespace ZombieOverdrive.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyType enemyType = EnemyType.Walker;
        [SerializeField] private float contactDamage = 10f;
        [SerializeField] private float attackInterval = 0.7f;
        [SerializeField] private float separationRadius = 0.45f;
        [SerializeField] private LayerMask enemyMask;
        [SerializeField] private GameObject acidProjectilePrefab;

        private Rigidbody2D body;
        private EnemyHealth health;
        private Transform target;
        private float moveSpeed;
        private float slowMultiplier = 1f;
        private float slowTimer;
        private Vector2 knockbackVelocity;
        private float attackTimer;
        private float rangedTimer;

        public EnemyType Type => enemyType;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<EnemyHealth>();
        }

        public void Initialize(Transform player, EnemyType type, float speed, float damage)
        {
            target = player;
            enemyType = type;
            moveSpeed = speed;
            contactDamage = damage;
            attackTimer = Random.Range(0f, attackInterval);
            rangedTimer = Random.Range(0.5f, 2.2f);
            slowMultiplier = 1f;
            slowTimer = 0f;
            knockbackVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (target == null || !health.IsAlive)
            {
                return;
            }

            Vector2 toTarget = (target.position - transform.position);
            Vector2 direction = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : Vector2.zero;
            Vector2 separation = CalculateSeparation();
            Vector2 movement = (direction + separation * 0.35f).normalized;

            float speed = moveSpeed * slowMultiplier;
            if (enemyType == EnemyType.Runner && toTarget.magnitude < 4f)
            {
                speed *= 1.25f;
            }

            if (enemyType == EnemyType.Spitter && toTarget.magnitude < 5f)
            {
                movement = (-direction + separation * 0.25f).normalized;
            }

            Vector2 displacement = movement * speed + knockbackVelocity;
            body.MovePosition(body.position + displacement * Time.fixedDeltaTime);
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, 7f * Time.fixedDeltaTime);
        }

        private void Update()
        {
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }

            if (slowTimer > 0f)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0f)
                {
                    slowMultiplier = 1f;
                }
            }

            if (enemyType == EnemyType.Spitter || enemyType == EnemyType.FinalBoss)
            {
                UpdateRangedAttack();
            }
        }

        public void ApplyKnockback(Vector2 force)
        {
            knockbackVelocity += force;
        }

        public void ApplySlow(float amount, float duration)
        {
            slowMultiplier = Mathf.Min(slowMultiplier, Mathf.Clamp01(1f - amount));
            slowTimer = Mathf.Max(slowTimer, duration);
        }

        private void UpdateRangedAttack()
        {
            if (target == null || acidProjectilePrefab == null)
            {
                return;
            }

            rangedTimer -= Time.deltaTime;
            if (rangedTimer > 0f)
            {
                return;
            }

            Vector2 direction = target.position - transform.position;
            if (direction.sqrMagnitude > 0.01f)
            {
                GameObject projectile = Instantiate(acidProjectilePrefab, transform.position, Quaternion.identity);
                AcidProjectile acid = projectile.GetComponent<AcidProjectile>();
                if (acid != null)
                {
                    float damage = enemyType == EnemyType.FinalBoss ? contactDamage * 0.8f : contactDamage * 0.65f;
                    acid.Launch(direction.normalized, damage);
                }
            }

            rangedTimer = enemyType == EnemyType.FinalBoss ? 1.35f : 2.8f;
        }

        private Vector2 CalculateSeparation()
        {
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius, enemyMask);
            Vector2 force = Vector2.zero;
            for (int i = 0; i < nearby.Length; i++)
            {
                if (nearby[i].gameObject == gameObject)
                {
                    continue;
                }

                Vector2 away = (Vector2)(transform.position - nearby[i].transform.position);
                float sqrMagnitude = Mathf.Max(away.sqrMagnitude, 0.01f);
                force += away.normalized / sqrMagnitude;
            }

            return force;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (attackTimer > 0f)
            {
                return;
            }

            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
                attackTimer = attackInterval;
            }
        }
    }
}
