using System;
using UnityEngine;
using ZombieOverdrive.Pickups;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int xpValue = 1;
        [SerializeField] private float deathKnockback = 0.1f;
        [SerializeField] private bool isBoss;

        private Poolable poolable;
        private GameObjectPool xpPool;
        private GameObjectPool resourcePool;
        private LayerMask enemyMask;
        private float currentHealth;
        private float maxHealth;

        public static event Action<EnemyHealth> EnemyKilled;
        public event Action<float, float> HealthChanged;

        public bool IsAlive => gameObject.activeInHierarchy && currentHealth > 0f;
        public bool IsBoss => isBoss;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        public void Initialize(float health, int droppedXp, GameObjectPool experiencePool)
        {
            Initialize(health, droppedXp, experiencePool, false);
        }

        public void Initialize(float health, int droppedXp, GameObjectPool experiencePool, bool boss)
        {
            Initialize(health, droppedXp, experiencePool, boss, null, default);
        }

        public void Initialize(float health, int droppedXp, GameObjectPool experiencePool, bool boss, GameObjectPool resourcePickupPool, LayerMask enemyLayerMask)
        {
            maxHealth = health;
            currentHealth = maxHealth;
            xpValue = droppedXp;
            xpPool = experiencePool;
            resourcePool = resourcePickupPool;
            enemyMask = enemyLayerMask;
            isBoss = boss;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || currentHealth <= 0f)
            {
                return;
            }

            currentHealth -= amount;
            HealthChanged?.Invoke(Mathf.Max(0f, currentHealth), maxHealth);
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            DropExperience();
            DropGold();
            EnemyKilled?.Invoke(this);

            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void DropExperience()
        {
            if (xpPool == null)
            {
                return;
            }

            int drops = isBoss ? 4 : 1;
            int valuePerDrop = isBoss ? Mathf.Max(1, xpValue / drops) : xpValue;
            for (int i = 0; i < drops; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * (isBoss ? 1.2f : deathKnockback);
                ExperiencePickup pickup = xpPool.Get<ExperiencePickup>(transform.position + (Vector3)offset, Quaternion.identity);
                if (pickup != null)
                {
                    pickup.SetValue(valuePerDrop);
                }
            }
        }

        private void DropGold()
        {
            if (resourcePool == null)
            {
                return;
            }

            float chance = isBoss ? 1f : UnityEngine.Random.value;
            if (!isBoss && chance > 0.12f)
            {
                return;
            }

            int value = isBoss ? UnityEngine.Random.Range(180, 260) : UnityEngine.Random.Range(3, 9);
            ResourcePickup pickup = resourcePool.Get<ResourcePickup>(transform.position, Quaternion.identity);
            if (pickup != null)
            {
                pickup.Configure(ResourcePickupType.Gold, value, enemyMask);
            }
        }
    }
}
