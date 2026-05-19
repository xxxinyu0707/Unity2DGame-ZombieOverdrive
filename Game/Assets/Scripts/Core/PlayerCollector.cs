using UnityEngine;
using ZombieOverdrive.Pickups;

namespace ZombieOverdrive.Core
{
    public class PlayerCollector : MonoBehaviour
    {
        [SerializeField] private LayerMask pickupMask;

        private PlayerStats stats;
        private LevelSystem levelSystem;

        public void Initialize(PlayerStats playerStats, LevelSystem playerLevelSystem)
        {
            stats = playerStats;
            levelSystem = playerLevelSystem;
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
            }
        }
    }
}
