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

        private Poolable poolable;
        private Vector2 direction;
        private float timer;
        private int remainingPierces;

        public float Damage { get; private set; }

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        public void Launch(Vector2 launchDirection, float damage, int pierces)
        {
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : Vector2.right;
            Damage = damage;
            remainingPierces = pierces;
            timer = lifetime;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
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

            enemy.TakeDamage(Damage);
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
