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

        private Poolable poolable;
        private GameObjectPool xpPool;
        private float currentHealth;
        private float maxHealth;

        public static event Action<EnemyHealth> EnemyKilled;

        public bool IsAlive => gameObject.activeInHierarchy && currentHealth > 0f;

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        public void Initialize(float health, int droppedXp, GameObjectPool experiencePool)
        {
            maxHealth = health;
            currentHealth = maxHealth;
            xpValue = droppedXp;
            xpPool = experiencePool;
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || currentHealth <= 0f)
            {
                return;
            }

            currentHealth -= amount;
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            DropExperience();
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

            Vector2 offset = UnityEngine.Random.insideUnitCircle * deathKnockback;
            ExperiencePickup pickup = xpPool.Get<ExperiencePickup>(transform.position + (Vector3)offset, Quaternion.identity);
            if (pickup != null)
            {
                pickup.SetValue(xpValue);
            }
        }
    }
}
