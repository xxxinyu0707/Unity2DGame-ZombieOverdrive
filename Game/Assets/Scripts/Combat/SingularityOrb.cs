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
        private bool evolved;
        private float maxHealthPercentDamage;
        private bool absorbProjectiles;
        private bool collapseOnEnd;
        private bool ending;

        public void Launch(Vector2 launchDirection, float orbDamage, float pullRadius, float pullForce, float lifetime, LayerMask mask)
        {
            Launch(launchDirection, orbDamage, pullRadius, pullForce, lifetime, mask, false);
        }

        public void Launch(Vector2 launchDirection, float orbDamage, float pullRadius, float pullForce, float lifetime, LayerMask mask, bool evolvedOrb)
        {
            Launch(launchDirection, orbDamage, pullRadius, pullForce, lifetime, mask, evolvedOrb, 0f, false, false);
        }

        public void Launch(Vector2 launchDirection, float orbDamage, float pullRadius, float pullForce, float lifetime, LayerMask mask, bool evolvedOrb, float percentDamage, bool absorbBullets, bool collapse)
        {
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : Vector2.right;
            damage = orbDamage;
            radius = pullRadius;
            pullStrength = pullForce;
            timer = lifetime;
            tickTimer = 0f;
            enemyMask = mask;
            evolved = evolvedOrb;
            maxHealthPercentDamage = Mathf.Max(0f, percentDamage);
            absorbProjectiles = absorbBullets;
            collapseOnEnd = collapse;
            ending = false;
            gameObject.SetActive(true);
            EnsureRing();
            UpdateRingShape();
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            transform.Rotate(0f, 0f, (evolved ? 220f : 130f) * Time.deltaTime);
            timer -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer = evolved ? tickInterval * 0.72f : tickInterval;
                PullAndDamage();
            }

            if (timer <= 0f)
            {
                EndOrb();
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
            ring.startWidth = 0.035f;
            ring.endWidth = 0.035f;
            ring.sortingOrder = 6;
        }

        private void UpdateRingShape()
        {
            if (ring == null)
            {
                return;
            }

            ring.startColor = evolved ? new Color(0.8f, 0.35f, 1f, 0.42f) : new Color(0.42f, 0.2f, 1f, 0.22f);
            ring.endColor = ring.startColor;
            ring.startWidth = evolved ? 0.06f : 0.035f;
            ring.endWidth = evolved ? 0.06f : 0.035f;
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

                float percentDamage = maxHealthPercentDamage > 0f ? enemy.MaxHealth * maxHealthPercentDamage : 0f;
                enemy.TakeDamage(damage + percentDamage);
                EnemyController controller = Hits[i].GetComponent<EnemyController>();
                if (controller != null)
                {
                    Vector2 pull = ((Vector2)transform.position - (Vector2)Hits[i].transform.position).normalized * pullStrength;
                    controller.ApplyKnockback(pull);
                    if (evolved)
                    {
                        controller.ApplySlow(0.35f, 0.45f);
                    }
                }
            }

            if (absorbProjectiles)
            {
                AbsorbProjectiles();
            }

            if (evolved)
            {
                CombatVisuals.SpawnRing(transform.position, new Color(0.8f, 0.35f, 1f, 0.28f), radius * 0.42f, 0.08f);
            }
        }

        private void AbsorbProjectiles()
        {
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, radius);
            for (int i = 0; i < nearby.Length; i++)
            {
                AcidProjectile acid = nearby[i].GetComponent<AcidProjectile>();
                if (acid != null)
                {
                    Destroy(acid.gameObject);
                    CombatVisuals.SpawnRing(acid.transform.position, new Color(0.55f, 1f, 0.35f, 0.35f), 0.22f, 0.08f);
                }
            }
        }

        private void EndOrb()
        {
            if (ending)
            {
                return;
            }

            ending = true;
            if (collapseOnEnd)
            {
                CombatVisuals.SpawnExplosion(transform.position, new Color(0.85f, 0.35f, 1f, 0.75f), radius * 0.8f, 0.18f);
                int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius * 1.05f, Hits, enemyMask);
                for (int i = 0; i < count; i++)
                {
                    EnemyHealth enemy = Hits[i].GetComponent<EnemyHealth>();
                    if (enemy != null && enemy.IsAlive)
                    {
                        enemy.TakeDamage(damage * 3f + enemy.MaxHealth * 0.025f);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}
