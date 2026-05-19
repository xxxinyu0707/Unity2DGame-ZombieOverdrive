using UnityEngine;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.World;

namespace ZombieOverdrive.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class SingularityOrb : MonoBehaviour
    {
        [SerializeField] private float speed = 4.5f;
        [SerializeField] private float tickInterval = 0.35f;

        private static readonly Collider2D[] Hits = new Collider2D[64];
        private Vector2 direction;
        private float damage;
        private float radius;
        private float pullStrength;
        private float timer;
        private float tickTimer;
        private LayerMask enemyMask;
        private LineRenderer ring;

        public void Launch(Vector2 launchDirection, float orbDamage, float pullRadius, float pullForce, float lifetime, LayerMask mask)
        {
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : Vector2.right;
            damage = orbDamage;
            radius = pullRadius;
            pullStrength = pullForce;
            timer = lifetime;
            tickTimer = 0f;
            enemyMask = mask;
            gameObject.SetActive(true);
            EnsureRing();
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            transform.Rotate(0f, 0f, 130f * Time.deltaTime);
            timer -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer = tickInterval;
                PullAndDamage();
            }

            if (timer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void EnsureRing()
        {
            if (ring != null)
            {
                return;
            }

            GameObject ringObject = new GameObject("Pull Radius");
            ringObject.transform.SetParent(transform, false);
            ring = ringObject.AddComponent<LineRenderer>();
            ring.material = new Material(Shader.Find("Sprites/Default"));
            ring.loop = true;
            ring.useWorldSpace = false;
            ring.positionCount = 36;
            ring.startColor = new Color(0.42f, 0.2f, 1f, 0.22f);
            ring.endColor = new Color(0.42f, 0.2f, 1f, 0.22f);
            ring.startWidth = 0.035f;
            ring.endWidth = 0.035f;
            ring.sortingOrder = 6;

            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = Mathf.PI * 2f * i / ring.positionCount;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            }
        }

        private void PullAndDamage()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, Hits, enemyMask);
            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = Hits[i].GetComponent<EnemyHealth>();
                DestructibleCrate crate = Hits[i].GetComponent<DestructibleCrate>();
                if (crate != null)
                {
                    crate.TakeDamage(damage);
                }

                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                enemy.TakeDamage(damage);
                EnemyController controller = Hits[i].GetComponent<EnemyController>();
                if (controller != null)
                {
                    Vector2 pull = ((Vector2)transform.position - (Vector2)Hits[i].transform.position).normalized * pullStrength;
                    controller.ApplyKnockback(pull);
                }
            }
        }
    }
}
