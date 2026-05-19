using UnityEngine;

namespace ZombieOverdrive.Core
{
    [System.Serializable]
    public class PlayerStats
    {
        public float baseMoveSpeed = 4f;
        public float maxHealth = 100f;
        public float damageMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public float magnetRange = 2.5f;
        public int bulletPierceBonus;

        public float MoveSpeed => baseMoveSpeed;

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

        public void AddMagnetRange(float amount)
        {
            magnetRange += amount;
        }
    }
}
