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

        private Rigidbody2D body;
        private EnemyHealth health;
        private Transform target;
        private float moveSpeed;
        private float attackTimer;

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

            float speed = moveSpeed;
            if (enemyType == EnemyType.Runner && toTarget.magnitude < 4f)
            {
                speed *= 1.25f;
            }

            body.MovePosition(body.position + movement * speed * Time.fixedDeltaTime);
        }

        private void Update()
        {
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }
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
