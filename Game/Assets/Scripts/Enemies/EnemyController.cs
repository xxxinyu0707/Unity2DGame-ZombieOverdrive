using UnityEngine;
using System.Collections;
using ZombieOverdrive.Combat;
using ZombieOverdrive.Core;

namespace ZombieOverdrive.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyType enemyType = EnemyType.Walker;
        [SerializeField] private float contactDamage = 10f;
        [SerializeField] private float attackInterval = 0.55f;
        [SerializeField] private float separationRadius = 0.45f;
        [SerializeField] private LayerMask enemyMask;
        [SerializeField] private GameObject acidProjectilePrefab;

        private Rigidbody2D body;
        private EnemyHealth health;
        private Transform target;
        private float moveSpeed;
        private float slowMultiplier = 1f;
        private float slowTimer;
        private float stunTimer;
        private float burningTimer;
        private Vector2 knockbackVelocity;
        private float attackTimer;
        private float rangedTimer;
        private float bossSkillTimer;
        private bool charging;
        private Vector2 chargeDirection;
        private float chargeTimer;
        private bool enraged;

        public EnemyType Type => enemyType;
        public bool IsImpaired => slowTimer > 0f || stunTimer > 0f || burningTimer > 0f;

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
            attackInterval = type == EnemyType.Tanker || type == EnemyType.MutantBoss || type == EnemyType.FinalBoss ? 0.8f : 0.55f;
            attackTimer = Random.Range(0f, attackInterval);
            rangedTimer = Random.Range(0.5f, 2.2f);
            bossSkillTimer = type == EnemyType.MutantBoss ? 3.5f : 4.5f;
            slowMultiplier = 1f;
            slowTimer = 0f;
            stunTimer = 0f;
            burningTimer = 0f;
            knockbackVelocity = Vector2.zero;
            charging = false;
            chargeTimer = 0f;
            enraged = false;
        }

        private void FixedUpdate()
        {
            if (target == null || !health.IsAlive)
            {
                return;
            }

            Vector2 toTarget = (target.position - transform.position);
            Vector2 direction = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : Vector2.zero;
            if (charging)
            {
                body.MovePosition(body.position + chargeDirection * moveSpeed * 4.5f * Time.fixedDeltaTime);
                chargeTimer -= Time.fixedDeltaTime;
                if (chargeTimer <= 0f)
                {
                    charging = false;
                }

                return;
            }

            Vector2 separation = CalculateSeparation();
            Vector2 movement = (direction + separation * 0.35f).normalized;

            float speed = moveSpeed * slowMultiplier;
            if (stunTimer > 0f)
            {
                speed = 0f;
            }

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

            if (stunTimer > 0f)
            {
                stunTimer -= Time.deltaTime;
            }

            if (burningTimer > 0f)
            {
                burningTimer -= Time.deltaTime;
            }

            if (enemyType == EnemyType.Spitter || enemyType == EnemyType.FinalBoss)
            {
                UpdateRangedAttack();
            }

            if (enemyType == EnemyType.MutantBoss || enemyType == EnemyType.FinalBoss)
            {
                UpdateBossSkills();
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

        public void ApplyStun(float duration)
        {
            stunTimer = Mathf.Max(stunTimer, duration);
        }

        public void MarkBurning(float duration)
        {
            burningTimer = Mathf.Max(burningTimer, duration);
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

        private void UpdateBossSkills()
        {
            if (target == null)
            {
                return;
            }

            if (enemyType == EnemyType.FinalBoss && !enraged && health.CurrentHealth / Mathf.Max(1f, health.MaxHealth) <= 0.3f)
            {
                enraged = true;
                CombatVisuals.SpawnExplosion(transform.position, new Color(0.85f, 0.15f, 1f, 0.7f), 2.8f, 0.25f);
                moveSpeed *= 1.35f;
            }

            bossSkillTimer -= Time.deltaTime * (enraged ? 1.45f : 1f);
            if (bossSkillTimer > 0f)
            {
                return;
            }

            if (enemyType == EnemyType.MutantBoss)
            {
                if (Random.value < 0.55f)
                {
                    StartCoroutine(MutantCharge());
                }
                else
                {
                    StartCoroutine(GroundRupture());
                }

                bossSkillTimer = 4.6f;
            }
            else
            {
                float roll = Random.value;
                if (roll < 0.36f)
                {
                    BloodNova();
                }
                else if (roll < 0.72f)
                {
                    StartCoroutine(AcidRain());
                }
                else
                {
                    StartCoroutine(GroundRupture());
                }

                bossSkillTimer = enraged ? 2.4f : 3.6f;
            }
        }

        private IEnumerator MutantCharge()
        {
            if (target == null)
            {
                yield break;
            }

            chargeDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
            if (chargeDirection.sqrMagnitude < 0.01f)
            {
                chargeDirection = Vector2.right;
            }

            CombatVisuals.SpawnTelegraphLine(transform.position, transform.position + (Vector3)(chargeDirection * 9f), 0.16f, 0.85f, new Color(1f, 0.18f, 0.12f, 0.72f));
            yield return new WaitForSeconds(0.85f);
            charging = true;
            chargeTimer = 0.55f;
            CombatVisuals.SpawnExplosion(transform.position, new Color(1f, 0.24f, 0.12f, 0.55f), 0.8f, 0.12f);
        }

        private IEnumerator GroundRupture()
        {
            Vector2 baseDirection = target != null ? ((Vector2)target.position - (Vector2)transform.position).normalized : Vector2.down;
            if (baseDirection.sqrMagnitude < 0.01f)
            {
                baseDirection = Vector2.down;
            }

            for (int i = -1; i <= 1; i++)
            {
                Vector2 direction = Rotate(baseDirection, i * 22f);
                CombatVisuals.SpawnTelegraphLine(transform.position, transform.position + (Vector3)(direction * 8f), 0.11f, 0.55f, new Color(1f, 0.42f, 0.12f, 0.58f));
            }

            yield return new WaitForSeconds(0.55f);

            for (int i = -1; i <= 1; i++)
            {
                Vector2 direction = Rotate(baseDirection, i * 22f);
                FireShockwave(direction, enemyType == EnemyType.FinalBoss ? contactDamage * 0.9f : contactDamage * 0.75f);
            }
        }

        private void FireShockwave(Vector2 direction, float damage)
        {
            CombatVisuals.SpawnTelegraphLine(transform.position, transform.position + (Vector3)(direction * 8f), 0.18f, 0.14f, new Color(1f, 0.75f, 0.26f, 0.8f));
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 0.55f, direction, 8f);
            for (int i = 0; i < hits.Length; i++)
            {
                PlayerHealth player = hits[i].collider.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }
        }

        private void BloodNova()
        {
            int count = enraged ? 18 : 12;
            CombatVisuals.SpawnExplosion(transform.position, new Color(0.85f, 0.05f, 0.12f, 0.65f), 1.45f, 0.18f);
            for (int i = 0; i < count; i++)
            {
                Vector2 direction = Rotate(Vector2.right, i * (360f / count) + Time.time * 35f);
                SpawnAcid(direction, contactDamage * 0.55f);
            }
        }

        private IEnumerator AcidRain()
        {
            if (target == null)
            {
                yield break;
            }

            int count = enraged ? 8 : 5;
            Vector3[] positions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                positions[i] = target.position + (Vector3)(Random.insideUnitCircle * (enraged ? 4.2f : 3.2f));
                CombatVisuals.SpawnTelegraphCircle(positions[i], 1.05f, 0.75f, new Color(0.65f, 1f, 0.22f, 0.58f));
            }

            yield return new WaitForSeconds(0.75f);

            for (int i = 0; i < positions.Length; i++)
            {
                DamageZone.SpawnPlayerZone(positions[i], 1.05f, contactDamage * 0.75f, enraged ? 3.5f : 2.8f, new Color(0.5f, 1f, 0.18f, 0.42f));
            }
        }

        private void SpawnAcid(Vector2 direction, float damage)
        {
            if (acidProjectilePrefab == null)
            {
                return;
            }

            GameObject projectile = Instantiate(acidProjectilePrefab, transform.position, Quaternion.identity);
            AcidProjectile acid = projectile.GetComponent<AcidProjectile>();
            if (acid != null)
            {
                acid.Launch(direction.normalized, damage);
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

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos).normalized;
        }
    }
}
