using UnityEngine;
using ZombieOverdrive.Combat;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.UI;

namespace ZombieOverdrive.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Run")]
        [SerializeField] private float runDurationSeconds = 600f;

        [Header("Scene References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerCollector playerCollector;
        [SerializeField] private LevelSystem levelSystem;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private PistolWeapon pistolWeapon;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private GameHud hud;
        [SerializeField] private UpgradePanel upgradePanel;

        private readonly PlayerStats playerStats = new PlayerStats();
        private float elapsedSeconds;

        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; } = GameState.Playing;
        public Transform Player => playerMovement != null ? playerMovement.transform : null;
        public int KillCount { get; private set; }
        public float ElapsedSeconds => elapsedSeconds;

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            playerMovement.Initialize(playerStats);
            playerHealth.Initialize(playerStats);
            playerCollector.Initialize(playerStats, levelSystem);
            upgradeSystem.Initialize(playerStats, pistolWeapon, playerHealth);
            pistolWeapon.Initialize(playerStats, playerMovement);
            waveSpawner.Initialize(this);

            playerHealth.HealthChanged += OnHealthChanged;
            playerHealth.Died += OnPlayerDied;
            levelSystem.ExperienceChanged += OnExperienceChanged;
            levelSystem.LevelChanged += OnLevelChanged;
            levelSystem.LevelUpAvailable += OnLevelUpAvailable;
            EnemyHealth.EnemyKilled += OnEnemyKilled;

            hud.SetLevel(levelSystem.Level);
            hud.SetKills(KillCount);
            hud.SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            hud.SetExperience(0, LevelSystem.CalculateRequiredXp(levelSystem.Level));
            hud.SetTimer(elapsedSeconds, runDurationSeconds);
            hud.SetMessage("");
        }

        private void OnDestroy()
        {
            EnemyHealth.EnemyKilled -= OnEnemyKilled;
        }

        private void Update()
        {
            if (State != GameState.Playing)
            {
                if (State == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
                {
                    TogglePause();
                }

                if ((State == GameState.GameOver || State == GameState.Victory) && Input.GetKeyDown(KeyCode.R))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
                }

                return;
            }

            elapsedSeconds += Time.deltaTime;
            hud.SetTimer(elapsedSeconds, runDurationSeconds);

            if (elapsedSeconds >= runDurationSeconds)
            {
                WinRun();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                State = GameState.Paused;
                Time.timeScale = 0f;
                hud.SetMessage("Paused - Press ESC to resume");
            }
            else if (State == GameState.Paused)
            {
                State = GameState.Playing;
                Time.timeScale = 1f;
                hud.SetMessage("");
            }
        }

        public void ResumeFromUpgrade(UpgradeOption option)
        {
            upgradeSystem.Apply(option);
            upgradePanel.Hide();
            State = GameState.Playing;
            Time.timeScale = 1f;
            hud.SetMessage("");
        }

        private void OnLevelUpAvailable()
        {
            State = GameState.LevelUp;
            Time.timeScale = 0f;
            upgradePanel.Show(upgradeSystem.RollOptions(), ResumeFromUpgrade);
            hud.SetMessage("Level Up");
        }

        private void OnHealthChanged(float current, float max)
        {
            hud.SetHealth(current, max);
        }

        private void OnExperienceChanged(int current, int required, int level)
        {
            hud.SetExperience(current, required);
        }

        private void OnLevelChanged(int level)
        {
            hud.SetLevel(level);
        }

        private void OnEnemyKilled(EnemyHealth enemy)
        {
            KillCount++;
            hud.SetKills(KillCount);
        }

        private void OnPlayerDied()
        {
            State = GameState.GameOver;
            Time.timeScale = 0f;
            hud.SetMessage("Game Over - Press R to restart");
        }

        private void WinRun()
        {
            State = GameState.Victory;
            Time.timeScale = 0f;
            hud.SetMessage("Victory - Press R to restart");
        }
    }
}
