using UnityEngine;
using ZombieOverdrive.Core;

namespace ZombieOverdrive.Enemies
{
    [RequireComponent(typeof(Collider2D))]
    public class AcidProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 5.5f;
        [SerializeField] private float lifetime = 4f;

        private Vector2 direction;
        private float damage;
        private float timer;

        public void Launch(Vector2 launchDirection, float projectileDamage)
        {
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : Vector2.down;
            damage = projectileDamage;
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
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player == null)
            {
                return;
            }

            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
