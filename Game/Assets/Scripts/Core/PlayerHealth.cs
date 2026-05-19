using System;
using UnityEngine;

namespace ZombieOverdrive.Core
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float invincibleSecondsAfterHit = 0.5f;

        private float currentHealth;
        private float invincibleTimer;
        private PlayerStats stats;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => stats != null ? stats.maxHealth : 100f;

        public void Initialize(PlayerStats playerStats)
        {
            stats = playerStats;
            currentHealth = stats.maxHealth;
            HealthChanged?.Invoke(currentHealth, stats.maxHealth);
        }

        private void Update()
        {
            if (invincibleTimer > 0f)
            {
                invincibleTimer -= Time.deltaTime;
            }

            if (stats != null && stats.healthRegenPerSecond > 0f && currentHealth > 0f && currentHealth < MaxHealth)
            {
                Heal(stats.healthRegenPerSecond * Time.deltaTime);
            }
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || invincibleTimer > 0f || currentHealth <= 0f)
            {
                return;
            }

            float finalDamage = stats != null ? stats.ModifyIncomingDamage(amount) : amount;
            currentHealth = Mathf.Max(0f, currentHealth - finalDamage);
            invincibleTimer = invincibleSecondsAfterHit;
            HealthChanged?.Invoke(currentHealth, MaxHealth);

            if (currentHealth <= 0f)
            {
                Died?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        public bool TryConsumeRevive()
        {
            if (stats == null || stats.reviveCharges <= 0)
            {
                return false;
            }

            stats.reviveCharges--;
            currentHealth = MaxHealth * 0.45f;
            invincibleTimer = 2.5f;
            HealthChanged?.Invoke(currentHealth, MaxHealth);
            return true;
        }
    }
}
