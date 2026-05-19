using UnityEngine;
using ZombieOverdrive.Pickups;

namespace ZombieOverdrive.Core
{
    public class PlayerCollector : MonoBehaviour
    {
        [SerializeField] private LayerMask pickupMask;

        private PlayerStats stats;
        private LevelSystem levelSystem;
        private PlayerHealth health;

        public void Initialize(PlayerStats playerStats, LevelSystem playerLevelSystem)
        {
            stats = playerStats;
            levelSystem = playerLevelSystem;
            health = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (stats == null)
            {
                return;
            }

            Collider2D[] pickups = Physics2D.OverlapCircleAll(transform.position, stats.magnetRange, pickupMask);
            for (int i = 0; i < pickups.Length; i++)
            {
                ExperiencePickup pickup = pickups[i].GetComponent<ExperiencePickup>();
                if (pickup != null)
                {
                    pickup.PullTo(transform, levelSystem);
                }

                ResourcePickup resource = pickups[i].GetComponent<ResourcePickup>();
                if (resource != null)
                {
                    resource.PullTo(transform, this);
                }
            }
        }

        public void AddRunGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int finalAmount = Mathf.Max(1, Mathf.RoundToInt(amount * (stats != null ? stats.goldMultiplier : 1f)));
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddRunGold(finalAmount);
            }
        }

        public void HealPercent(float percent)
        {
            if (health != null)
            {
                health.Heal(health.MaxHealth * Mathf.Clamp01(percent));
            }
        }

        public void PullAllPickups()
        {
            Collider2D[] pickups = Physics2D.OverlapCircleAll(transform.position, 40f, pickupMask);
            for (int i = 0; i < pickups.Length; i++)
            {
                ExperiencePickup xp = pickups[i].GetComponent<ExperiencePickup>();
                if (xp != null)
                {
                    xp.PullTo(transform, levelSystem);
                }

                ResourcePickup resource = pickups[i].GetComponent<ResourcePickup>();
                if (resource != null)
                {
                    resource.PullTo(transform, this);
                }
            }
        }
    }
}
