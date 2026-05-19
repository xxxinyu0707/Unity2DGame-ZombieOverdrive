using UnityEngine;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour, DamageDealer
    {
        [SerializeField] private float speed = 14f;
        [SerializeField] private float lifetime = 2.5f;
        [SerializeField] private float armDistance = 0.22f;

        private Poolable poolable;
        private Vector3 startPosition;
        private Vector2 direction;
        private float timer;
        private int remainingPierces;
        private float knockback;
        private bool piercingThroughAll;

        public float Damage { get; private set; }

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        public void Launch(Vector2 launchDirection, float damage, int pierces)
        {
            Launch(launchDirection, damage, pierces, 0f, 1f, false);
        }

        public void Launch(Vector2 launchDirection, float damage, int pierces, float knockbackForce, float speedMultiplier, bool infinitePierce)
        {
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : Vector2.right;
            startPosition = transform.position;
            Damage = damage;
            remainingPierces = pierces;
            knockback = knockbackForce;
            piercingThroughAll = infinitePierce;
            timer = lifetime;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            transform.localScale = Vector3.one;
            speedMultiplier = Mathf.Max(0.1f, speedMultiplier);
            currentSpeed = speed * speedMultiplier;
        }

        private float currentSpeed;

        private void Update()
        {
            transform.position += (Vector3)(direction * currentSpeed * Time.deltaTime);
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Release();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy == null)
            {
                return;
            }

            if (Vector2.Distance(startPosition, transform.position) < armDistance)
            {
                return;
            }

            enemy.TakeDamage(Damage);
            if (knockback > 0f)
            {
                EnemyController controller = other.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.ApplyKnockback(direction * knockback);
                }
            }

            if (piercingThroughAll)
            {
                return;
            }

            remainingPierces--;
            if (remainingPierces < 0)
            {
                Release();
            }
        }

        private void Release()
        {
            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
