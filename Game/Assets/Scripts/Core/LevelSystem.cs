using System;
using UnityEngine;

namespace ZombieOverdrive.Core
{
    public class LevelSystem : MonoBehaviour
    {
        [SerializeField] private int startingLevel = 1;

        private PlayerStats stats;
        private int currentXp;
        private int xpToNextLevel;

        public event Action<int, int, int> ExperienceChanged;
        public event Action<int> LevelChanged;
        public event Action LevelUpAvailable;

        public int Level { get; private set; }

        private void Awake()
        {
            Level = startingLevel;
            xpToNextLevel = CalculateRequiredXp(Level);
        }

        public void Initialize(PlayerStats playerStats)
        {
            stats = playerStats;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            float multiplier = stats != null ? stats.xpMultiplier : 1f;
            currentXp += Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));
            while (currentXp >= xpToNextLevel)
            {
                currentXp -= xpToNextLevel;
                Level++;
                xpToNextLevel = CalculateRequiredXp(Level);
                LevelChanged?.Invoke(Level);
                LevelUpAvailable?.Invoke();
            }

            ExperienceChanged?.Invoke(currentXp, xpToNextLevel, Level);
        }

        public static int CalculateRequiredXp(int level)
        {
            if (level <= 1)
            {
                return 4;
            }

            if (level <= 5)
            {
                return 4 + (level - 1) * 3;
            }

            if (level <= 12)
            {
                return Mathf.RoundToInt(18f + (level - 5) * 4.5f);
            }

            return Mathf.RoundToInt(50f + (level - 12) * 8f + Mathf.Pow(level - 12, 1.35f) * 2.5f);
        }
    }
}
