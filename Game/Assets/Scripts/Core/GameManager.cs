using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieOverdrive.Combat;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.UI;
using ZombieOverdrive.World;

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
        [SerializeField] private WeaponBase[] weapons;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private CrateSpawner crateSpawner;
        [SerializeField] private GameHud hud;
        [SerializeField] private MainMenuPanel mainMenu;
        [SerializeField] private UpgradePanel upgradePanel;
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private ZombieOverdrive.UI.UpgradeIconLibrary iconLibrary;

        private readonly PlayerStats playerStats = new PlayerStats();
        private float elapsedSeconds;
        private bool runSettled;

        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; } = GameState.MainMenu;
        public Transform Player => playerMovement != null ? playerMovement.transform : null;
        public int KillCount { get; private set; }
        public int RunGold { get; private set; }
        public float ElapsedSeconds => elapsedSeconds;

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            MetaProgression.ApplyTo(playerStats);
            playerMovement.Initialize(playerStats);
            playerHealth.Initialize(playerStats);
            levelSystem.Initialize(playerStats);
            playerCollector.Initialize(playerStats, levelSystem);
            if (weapons == null || weapons.Length == 0)
            {
                weapons = playerMovement.GetComponents<WeaponBase>();
            }

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    weapons[i].Initialize(playerStats, playerMovement);
                }
            }

            upgradeSystem.Initialize(playerStats, weapons, playerHealth);
            waveSpawner.Initialize(this);
            if (crateSpawner != null)
            {
                crateSpawner.Initialize(this);
            }
            if (pauseMenu != null)
            {
                pauseMenu.Initialize(ResumeFromPauseMenu, RestartRun, QuitGame);
            }
            if (mainMenu != null)
            {
                mainMenu.Initialize(StartRun, QuitGame);
                mainMenu.Show();
            }

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
            hud.SetGold(RunGold);
            hud.SetMessage("点击开始进入战斗");
            Time.timeScale = 0f;
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= OnHealthChanged;
                playerHealth.Died -= OnPlayerDied;
            }

            if (levelSystem != null)
            {
                levelSystem.ExperienceChanged -= OnExperienceChanged;
                levelSystem.LevelChanged -= OnLevelChanged;
                levelSystem.LevelUpAvailable -= OnLevelUpAvailable;
            }

            EnemyHealth.EnemyKilled -= OnEnemyKilled;
        }

        private void Update()
        {
            if (State == GameState.MainMenu)
            {
                return;
            }

            if (State != GameState.Playing)
            {
                if (State == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
                {
                    ResumeFromPauseMenu();
                }

                if ((State == GameState.GameOver || State == GameState.Victory) && Input.GetKeyDown(KeyCode.R))
                {
                    RestartRun();
                }

                return;
            }

            elapsedSeconds += Time.deltaTime;
            hud.SetTimer(elapsedSeconds, runDurationSeconds);

            if (elapsedSeconds >= runDurationSeconds && waveSpawner != null && waveSpawner.FinalBossDefeated)
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
                hud.SetMessage("");
                if (pauseMenu != null)
                {
                    UpdatePauseSlots();
                    pauseMenu.Show(upgradeSystem.BuildStatusText(levelSystem.Level, KillCount, elapsedSeconds));
                }
            }
            else if (State == GameState.Paused)
            {
                ResumeFromPauseMenu();
            }
        }

        public void ResumeFromPauseMenu()
        {
            if (State != GameState.Paused)
            {
                return;
            }

            State = GameState.Playing;
            Time.timeScale = 1f;
            hud.SetMessage("");
            if (pauseMenu != null)
            {
                pauseMenu.Hide();
            }
        }

        public void RestartRun()
        {
            if (State == GameState.Playing || State == GameState.Paused || State == GameState.LevelUp)
            {
                SettleRun(false);
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            if (State == GameState.Playing || State == GameState.Paused || State == GameState.LevelUp)
            {
                SettleRun(false);
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ResumeFromUpgrade(UpgradeOption option)
        {
            upgradeSystem.Apply(option);
            upgradePanel.Hide();
            State = GameState.Playing;
            Time.timeScale = 1f;
            hud.SetMessage("");
        }

        public void StartRun()
        {
            if (State != GameState.MainMenu)
            {
                return;
            }

            State = GameState.Playing;
            Time.timeScale = 1f;
            runSettled = false;
            if (mainMenu != null)
            {
                mainMenu.Hide();
            }

            hud.SetMessage("");
        }

        public void AddRunGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            RunGold += amount;
            hud.SetGold(RunGold);
        }

        private void OnLevelUpAvailable()
        {
            State = GameState.LevelUp;
            Time.timeScale = 0f;
            playerHealth.Heal(playerHealth.MaxHealth * playerStats.levelUpHealPercent);
            upgradePanel.Show(upgradeSystem.RollOptions(), ResumeFromUpgrade);
            hud.SetMessage("升级");
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
            if (playerHealth.TryConsumeRevive())
            {
                hud.SetMessage("已复活");
                return;
            }

            State = GameState.GameOver;
            Time.timeScale = 0f;
            int banked = SettleRun(false);
            hud.SetMessage("游戏结束 - 结算金币 " + banked + " - 按 R 重新开始");
        }

        private void WinRun()
        {
            State = GameState.Victory;
            Time.timeScale = 0f;
            int banked = SettleRun(true);
            hud.SetMessage("胜利 - 结算金币 " + banked + " - 按 R 重新开始");
        }

        private int SettleRun(bool victory)
        {
            if (runSettled)
            {
                return 0;
            }

            runSettled = true;
            int reward = RunGold + Mathf.FloorToInt(KillCount * 0.35f);
            if (victory)
            {
                reward += 1000;
            }

            MetaProgression.AddGold(reward);
            return reward;
        }

        private void UpdatePauseSlots()
        {
            if (pauseMenu == null || upgradeSystem == null || iconLibrary == null)
            {
                return;
            }

            Sprite[] activeIcons = new Sprite[upgradeSystem.MaxActiveWeapons];
            string[] activeTexts = new string[upgradeSystem.MaxActiveWeapons];
            for (int i = 0; i < upgradeSystem.MaxActiveWeapons; i++)
            {
                if (i < upgradeSystem.ActiveSlots.Count)
                {
                    WeaponId weaponId = upgradeSystem.ActiveSlots[i];
                    bool evolved = upgradeSystem.IsWeaponEvolved(weaponId);
                    activeIcons[i] = iconLibrary.Get(evolved ? UpgradeSystem.GetEvolutionIconId(weaponId) : UpgradeSystem.GetWeaponIconId(weaponId));
                    activeTexts[i] = UpgradeSystem.GetWeaponName(weaponId) + " 等级 " + upgradeSystem.GetWeaponLevel(weaponId) + (evolved ? " 超进化" : "");
                }
            }

            Sprite[] passiveIcons = new Sprite[upgradeSystem.MaxPassiveSkills];
            string[] passiveTexts = new string[upgradeSystem.MaxPassiveSkills];
            for (int i = 0; i < upgradeSystem.MaxPassiveSkills; i++)
            {
                if (i < upgradeSystem.PassiveSlots.Count)
                {
                    UpgradeType passive = upgradeSystem.PassiveSlots[i];
                    passiveIcons[i] = iconLibrary.Get(UpgradeSystem.GetPassiveIconId(passive));
                    passiveTexts[i] = UpgradeSystem.GetPassiveName(passive) + " 等级 " + upgradeSystem.GetPassiveSkillLevel(passive);
                }
            }

            pauseMenu.SetSlots(activeIcons, activeTexts, passiveIcons, passiveTexts);
        }
    }
}
