using UnityEngine;

namespace ZombieOverdrive.Core
{
    [System.Serializable]
    public class PlayerStats
    {
        public float baseMoveSpeed = 4.2f;
        public float maxHealth = 120f;
        public float damageMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public float projectileSpeedMultiplier = 1f;
        public float areaMultiplier = 1f;
        public float durationMultiplier = 1f;
        public float magnetRange = 2.75f;
        public float damageReduction;
        public float healthRegenPerSecond;
        public float xpMultiplier = 1f;
        public float goldMultiplier = 1f;
        public float criticalChance;
        public float criticalDamageMultiplier = 1.75f;
        public float levelUpHealPercent = 0.14f;
        public float reviveHealthPercent = 0.45f;
        public float pickupLuck = 1f;
        public int bulletPierceBonus;
        public int reviveCharges;

        public float MoveSpeed => baseMoveSpeed;

        public float RollDamage(float baseDamage)
        {
            float damage = baseDamage * damageMultiplier;
            if (criticalChance > 0f && Random.value < criticalChance)
            {
                damage *= criticalDamageMultiplier;
            }

            return damage;
        }

        public float ModifyIncomingDamage(float amount)
        {
            float reduction = Mathf.Clamp(damageReduction, 0f, 0.7f);
            return amount * (1f - reduction);
        }

        public void AddMoveSpeed(float percent)
        {
            baseMoveSpeed *= 1f + percent;
        }

        public void AddMaxHealth(float amount)
        {
            maxHealth += amount;
        }

        public void AddDamage(float percent)
        {
            damageMultiplier *= 1f + percent;
        }

        public void AddFireRate(float percent)
        {
            fireRateMultiplier *= 1f + percent;
        }

        public void AddProjectileSpeed(float percent)
        {
            projectileSpeedMultiplier *= 1f + percent;
        }

        public void AddArea(float percent)
        {
            areaMultiplier *= 1f + percent;
        }

        public void AddDuration(float percent)
        {
            durationMultiplier *= 1f + percent;
        }

        public void AddMagnetRange(float amount)
        {
            magnetRange += amount;
        }

        public void AddDamageReduction(float amount)
        {
            damageReduction = Mathf.Clamp(damageReduction + amount, 0f, 0.7f);
        }

        public void AddHealthRegen(float amount)
        {
            healthRegenPerSecond += amount;
        }

        public void AddXpGain(float percent)
        {
            xpMultiplier *= 1f + percent;
        }

        public void AddGoldGain(float percent)
        {
            goldMultiplier *= 1f + percent;
        }

        public void AddCritical(float chance, float damageBonus)
        {
            criticalChance = Mathf.Clamp01(criticalChance + chance);
            criticalDamageMultiplier += damageBonus;
        }

        public void AddPickupLuck(float percent)
        {
            pickupLuck *= 1f + percent;
        }
    }
}
