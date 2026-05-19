using System;
using UnityEngine;

namespace ZombieOverdrive.Core
{
    public class LevelSystem : MonoBehaviour
    {
        [SerializeField] private int startingLevel = 1;

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

        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentXp += amount;
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
            return Mathf.RoundToInt(10f + level * level * 1.2f + level * 8f);
        }
    }
}
