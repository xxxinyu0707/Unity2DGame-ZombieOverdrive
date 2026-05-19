using UnityEngine;
using ZombieOverdrive.Pickups;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.World
{
    [RequireComponent(typeof(Collider2D))]
    public class DestructibleCrate : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 65f;
        [SerializeField] private GameObjectPool pickupPool;
        [SerializeField] private LayerMask enemyMask;

        private Poolable poolable;
        private float health;

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        public void Initialize(GameObjectPool resourcePool, LayerMask enemyLayerMask)
        {
            pickupPool = resourcePool;
            enemyMask = enemyLayerMask;
            health = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || health <= 0f)
            {
                return;
            }

            health -= amount;
            if (health <= 0f)
            {
                Break();
            }
        }

        private void Break()
        {
            DropReward();
            ZombieOverdrive.Combat.CombatVisuals.SpawnRing(transform.position, new Color(1f, 0.75f, 0.3f, 0.75f), 0.8f, 0.16f);
            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void DropReward()
        {
            if (pickupPool == null)
            {
                return;
            }

            ResourcePickupType type = RollReward();
            ResourcePickup pickup = pickupPool.Get<ResourcePickup>(transform.position, Quaternion.identity);
            if (pickup != null)
            {
                pickup.Configure(type, Random.Range(18, 36), enemyMask);
            }
        }

        private ResourcePickupType RollReward()
        {
            float roll = Random.value;
            if (roll < 0.16f)
            {
                return ResourcePickupType.Bomb;
            }

            if (roll < 0.34f)
            {
                return ResourcePickupType.Magnet;
            }

            if (roll < 0.58f)
            {
                return ResourcePickupType.Chicken;
            }

            return ResourcePickupType.Gold;
        }
    }
}
