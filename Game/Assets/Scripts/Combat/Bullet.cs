using UnityEngine;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.Utility;
using ZombieOverdrive.World;

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
        private GameObjectPool sourcePool;
        private int splitCount;
        private float splitAngle;
        private float splitDamageMultiplier;
        private bool splitOnFirstEnemyHit;
        private bool hasSplit;
        private bool burstOnHit;
        private float burstRadius;
        private float burstDamageMultiplier;
        private LayerMask burstMask;
        private bool leaveFireOnHit;
        private float fireRadius;
        private float fireDps;
        private float fireDuration;
        private LayerMask fireMask;
        private bool bonusVsImpaired;
        private float impairedDamageMultiplier;

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
            sourcePool = null;
            splitCount = 0;
            splitAngle = 0f;
            splitDamageMultiplier = 0f;
            splitOnFirstEnemyHit = false;
            hasSplit = false;
            burstOnHit = false;
            burstRadius = 0f;
            burstDamageMultiplier = 0f;
            burstMask = default;
            leaveFireOnHit = false;
            fireRadius = 0f;
            fireDps = 0f;
            fireDuration = 0f;
            fireMask = default;
            bonusVsImpaired = false;
            impairedDamageMultiplier = 1f;
            timer = lifetime;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            transform.localScale = Vector3.one;
            speedMultiplier = Mathf.Max(0.1f, speedMultiplier);
            currentSpeed = speed * speedMultiplier;
        }

        public void ConfigureSplit(GameObjectPool pool, int count, float angleDegrees, float damageMultiplier)
        {
            sourcePool = pool;
            splitCount = Mathf.Max(0, count);
            splitAngle = Mathf.Max(0f, angleDegrees);
            splitDamageMultiplier = Mathf.Max(0f, damageMultiplier);
            splitOnFirstEnemyHit = sourcePool != null && splitCount > 0 && splitDamageMultiplier > 0f;
        }

        public void ConfigureBurst(float radius, float damageMultiplier, LayerMask mask)
        {
            burstOnHit = radius > 0f && damageMultiplier > 0f;
            burstRadius = radius;
            burstDamageMultiplier = damageMultiplier;
            burstMask = mask;
        }

        public void ConfigureFirePatch(float radius, float dps, float seconds, LayerMask mask)
        {
            leaveFireOnHit = radius > 0f && dps > 0f && seconds > 0f;
            fireRadius = radius;
            fireDps = dps;
            fireDuration = seconds;
            fireMask = mask;
        }

        public void ConfigureImpairedBonus(float multiplier)
        {
            bonusVsImpaired = multiplier > 1f;
            impairedDamageMultiplier = Mathf.Max(1f, multiplier);
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
            bool armed = Vector2.Distance(startPosition, transform.position) >= armDistance;
            if (!armed)
            {
                return;
            }

            DestructibleCrate crate = other.GetComponent<DestructibleCrate>();
            if (crate != null)
            {
                crate.TakeDamage(Damage);
                ConsumePierce();
                return;
            }

            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy == null)
            {
                return;
            }

            float finalDamage = Damage;
            EnemyController enemyController = other.GetComponent<EnemyController>();
            if (bonusVsImpaired && enemyController != null && enemyController.IsImpaired)
            {
                finalDamage *= impairedDamageMultiplier;
                CombatVisuals.SpawnRing(enemy.transform.position, new Color(0.6f, 0.95f, 1f, 0.55f), 0.34f, 0.09f);
            }

            enemy.TakeDamage(finalDamage);
            if (knockback > 0f)
            {
                if (enemyController != null)
                {
                    enemyController.ApplyKnockback(direction * knockback);
                }
            }

            if (splitOnFirstEnemyHit && !hasSplit)
            {
                hasSplit = true;
                SpawnSplitBullets();
            }

            if (burstOnHit)
            {
                Burst(other.transform.position);
            }

            if (leaveFireOnHit)
            {
                DamageZone.SpawnEnemyZone(other.transform.position, fireRadius, fireDps, fireDuration, fireMask, new Color(1f, 0.35f, 0.08f, 0.45f));
            }

            ConsumePierce();
        }

        private void SpawnSplitBullets()
        {
            if (sourcePool == null || splitCount <= 0)
            {
                return;
            }

            for (int i = 0; i < splitCount; i++)
            {
                float centered = splitCount == 1 ? 0f : i - (splitCount - 1) * 0.5f;
                float angle = centered * splitAngle;
                Bullet split = sourcePool.Get<Bullet>(transform.position, Quaternion.identity);
                if (split != null)
                {
                    split.Launch(Rotate(direction, angle), Damage * splitDamageMultiplier, 0, knockback * 0.45f, Mathf.Max(0.8f, currentSpeed / speed), false);
                }
            }
        }

        private void Burst(Vector3 center)
        {
            CombatVisuals.SpawnExplosion(center, new Color(1f, 0.72f, 0.22f, 0.65f), burstRadius, 0.14f);
            Collider2D[] burstHits = Physics2D.OverlapCircleAll(center, burstRadius, burstMask);
            for (int i = 0; i < burstHits.Length; i++)
            {
                EnemyHealth enemy = burstHits[i].GetComponent<EnemyHealth>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.TakeDamage(Damage * burstDamageMultiplier);
                }

                DestructibleCrate crate = burstHits[i].GetComponent<DestructibleCrate>();
                if (crate != null)
                {
                    crate.TakeDamage(Damage * burstDamageMultiplier);
                }
            }
        }

        private void ConsumePierce()
        {
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

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos).normalized;
        }
    }
}
