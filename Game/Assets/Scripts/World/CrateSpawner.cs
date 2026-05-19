using UnityEngine;
using ZombieOverdrive.Core;
using ZombieOverdrive.Pickups;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.World
{
    public class CrateSpawner : MonoBehaviour
    {
        [SerializeField] private GameObjectPool cratePool;
        [SerializeField] private GameObjectPool resourcePickupPool;
        [SerializeField] private LayerMask enemyMask;
        [SerializeField] private float spawnInterval = 30f;
        [SerializeField] private float spawnDistance = 10f;

        private GameManager manager;
        private float timer;

        public void Initialize(GameManager gameManager)
        {
            manager = gameManager;
            timer = 3f;
        }

        private void Update()
        {
            if (manager == null || manager.State != GameState.Playing || manager.Player == null)
            {
                return;
            }

            timer -= Time.deltaTime;
            if (timer > 0f)
            {
                return;
            }

            int count = Random.Range(2, 4);
            for (int i = 0; i < count; i++)
            {
                SpawnCrate();
            }

            timer = spawnInterval;
        }

        private void SpawnCrate()
        {
            if (cratePool == null)
            {
                return;
            }

            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector2.right;
            }

            Vector3 position = manager.Player.position + (Vector3)(direction * spawnDistance);
            DestructibleCrate crate = cratePool.Get<DestructibleCrate>(position, Quaternion.identity);
            if (crate != null)
            {
                crate.Initialize(resourcePickupPool, enemyMask);
            }
        }
    }
}
