using UnityEngine;
using ZombieOverdrive.Core;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Enemies
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Pools")]
        [SerializeField] private GameObjectPool walkerPool;
        [SerializeField] private GameObjectPool runnerPool;
        [SerializeField] private GameObjectPool spitterPool;
        [SerializeField] private GameObjectPool tankerPool;
        [SerializeField] private GameObjectPool mutantBossPool;
        [SerializeField] private GameObjectPool finalBossPool;
        [SerializeField] private GameObjectPool experiencePool;

        [Header("Spawn")]
        [SerializeField] private float spawnDistance = 9.5f;
        [SerializeField] private float spawnInterval = 0.45f;
        [SerializeField] private int maxEnemies = 120;
        [SerializeField] private int openingBurstCount = 6;
        [SerializeField] private float openingBurstDistance = 7.2f;

        private GameManager manager;
        private float spawnTimer;
        private int aliveEnemies;
        private bool spawnedOpeningBurst;
        private bool spawnedMidBoss;
        private bool spawnedFinalBoss;
        private bool finalBossDefeated;

        public int AliveEnemies => aliveEnemies;
        public void Initialize(GameManager gameManager)
        {
            manager = gameManager;
            spawnTimer = 0f;
            aliveEnemies = 0;
            spawnedOpeningBurst = false;
            spawnedMidBoss = false;
            spawnedFinalBoss = false;
            finalBossDefeated = false;

            EnemyHealth.EnemyKilled -= OnEnemyKilled;
            EnemyHealth.EnemyKilled += OnEnemyKilled;

            if (!HasRequiredPools())
            {
                Debug.LogError("WaveSpawner is missing one or more pool references.");
            }
        }

        private void OnDestroy()
        {
            EnemyHealth.EnemyKilled -= OnEnemyKilled;
        }

        private void Update()
        {
            if (manager == null || manager.State != GameState.Playing || manager.Player == null)
            {
                return;
            }

            UpdateWaveSettings(manager.ElapsedSeconds);
            TrySpawnBoss(manager.ElapsedSeconds);

            if (!spawnedOpeningBurst)
            {
                spawnedOpeningBurst = true;
                SpawnOpeningBurst();
                spawnTimer = Mathf.Min(spawnTimer, spawnInterval * 0.5f);
            }

            if (spawnedFinalBoss)
            {
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && aliveEnemies < maxEnemies)
            {
                SpawnEnemy(ChooseEnemyType(manager.ElapsedSeconds));
                spawnTimer = spawnInterval;
            }
        }

        private void UpdateWaveSettings(float elapsed)
        {
            if (elapsed < 120f)
            {
                maxEnemies = 30;
                spawnInterval = 0.42f;
            }
            else if (elapsed < 240f)
            {
                maxEnemies = 60;
                spawnInterval = 0.32f;
            }
            else if (elapsed < 360f)
            {
                maxEnemies = 100;
                spawnInterval = 0.24f;
            }
            else if (elapsed < 480f)
            {
                maxEnemies = 150;
                spawnInterval = 0.18f;
            }
            else
            {
                maxEnemies = 220;
                spawnInterval = 0.12f;
            }
        }

        private EnemyType ChooseEnemyType(float elapsed)
        {
            float roll = Random.value;
            if (elapsed >= 360f && roll < 0.16f)
            {
                return EnemyType.Tanker;
            }

            if (elapsed >= 240f && roll < 0.28f)
            {
                return EnemyType.Spitter;
            }

            if (elapsed >= 120f && roll < 0.45f)
            {
                return EnemyType.Runner;
            }

            return EnemyType.Walker;
        }

        private void SpawnEnemy(EnemyType type)
        {
            SpawnEnemyAt(type, GetSpawnPosition());
        }

        private bool SpawnEnemyAt(EnemyType type, Vector2 spawnPosition)
        {
            GameObjectPool pool = GetPool(type);
            if (pool == null)
            {
                return false;
            }

            EnemyController enemy = pool.Get<EnemyController>(spawnPosition, Quaternion.identity);
            if (enemy == null)
            {
                return false;
            }

            float elapsedMinutes = manager.ElapsedSeconds / 60f;
            float hpScale = Mathf.Pow(1f + 0.25f * elapsedMinutes, 1.5f);
            float speedScale = Mathf.Pow(1f + 0.08f * elapsedMinutes, 0.5f);
            GetBaseStats(type, out float hp, out float speed, out float damage, out int xp);

            enemy.Initialize(manager.Player, type, speed * speedScale, damage);
            bool boss = type == EnemyType.MutantBoss || type == EnemyType.FinalBoss;
            enemy.GetComponent<EnemyHealth>().Initialize(hp * hpScale, xp, experiencePool, boss);
            aliveEnemies++;
            return true;
        }

        private void SpawnOpeningBurst()
        {
            int count = Mathf.Max(1, openingBurstCount);
            for (int i = 0; i < count && aliveEnemies < maxEnemies; i++)
            {
                float angle = Mathf.PI * 2f * i / count + Random.Range(-0.18f, 0.18f);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 position = (Vector2)manager.Player.position + direction * openingBurstDistance;
                SpawnEnemyAt(EnemyType.Walker, position);
            }
        }

        private void TrySpawnBoss(float elapsed)
        {
            if (!spawnedMidBoss && elapsed >= 360f)
            {
                spawnedMidBoss = true;
                SpawnEnemy(EnemyType.MutantBoss);
            }

            if (!spawnedFinalBoss && elapsed >= 600f)
            {
                spawnedFinalBoss = true;
                aliveEnemies = 0;
                SpawnEnemy(EnemyType.FinalBoss);
            }
        }

        public bool FinalBossDefeated => finalBossDefeated;

        private Vector2 GetSpawnPosition()
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector2.right;
            }

            return (Vector2)manager.Player.position + direction * spawnDistance;
        }

        private bool HasRequiredPools()
        {
            return walkerPool != null
                && runnerPool != null
                && spitterPool != null
                && tankerPool != null
                && mutantBossPool != null
                && finalBossPool != null
                && experiencePool != null;
        }

        private GameObjectPool GetPool(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Runner:
                    return runnerPool;
                case EnemyType.Spitter:
                    return spitterPool;
                case EnemyType.Tanker:
                    return tankerPool;
                case EnemyType.MutantBoss:
                    return mutantBossPool;
                case EnemyType.FinalBoss:
                    return finalBossPool;
                default:
                    return walkerPool;
            }
        }

        private void GetBaseStats(EnemyType type, out float hp, out float speed, out float damage, out int xp)
        {
            switch (type)
            {
                case EnemyType.Runner:
                    hp = 36f;
                    speed = 3.05f;
                    damage = 8f;
                    xp = 5;
                    break;
                case EnemyType.Spitter:
                    hp = 72f;
                    speed = 1.35f;
                    damage = 10f;
                    xp = 5;
                    break;
                case EnemyType.Tanker:
                    hp = 420f;
                    speed = 0.95f;
                    damage = 20f;
                    xp = 20;
                    break;
                case EnemyType.MutantBoss:
                    hp = 2600f;
                    speed = 1.35f;
                    damage = 26f;
                    xp = 120;
                    break;
                case EnemyType.FinalBoss:
                    hp = 10000f;
                    speed = 1.05f;
                    damage = 34f;
                    xp = 400;
                    break;
                default:
                    hp = 44f;
                    speed = 1.35f;
                    damage = 7f;
                    xp = 1;
                    break;
            }
        }

        private void OnEnemyKilled(EnemyHealth enemy)
        {
            aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
            if (enemy != null)
            {
                EnemyController controller = enemy.GetComponent<EnemyController>();
                if (controller != null && controller.Type == EnemyType.FinalBoss)
                {
                    finalBossDefeated = true;
                }
            }
        }
    }
}
