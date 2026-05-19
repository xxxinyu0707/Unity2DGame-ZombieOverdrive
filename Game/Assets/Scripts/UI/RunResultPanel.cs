using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZombieOverdrive.UI
{
    public class RunResultPanel : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        private Action onRestart;
        private Action onQuit;

        public void Initialize(Action restart, Action quit)
        {
            onRestart = restart;
            onQuit = quit;
            restartButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => onRestart?.Invoke());
            quitButton.onClick.AddListener(() => onQuit?.Invoke());
            Hide();
        }

        public void Show(bool victory, int level, int kills, int runGold, int reward, float elapsedSeconds)
        {
            if (titleText != null)
            {
                titleText.text = victory ? "任务完成" : "行动终止";
                titleText.color = victory ? new Color(0.55f, 1f, 0.72f, 1f) : new Color(1f, 0.68f, 0.52f, 1f);
            }

            if (summaryText != null)
            {
                summaryText.text = "生存时间  " + FormatTime(elapsedSeconds)
                    + "\n等级  " + level
                    + "\n击杀  " + kills
                    + "\n局内金币  " + runGold
                    + "\n结算金币  " + reward;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return minutes.ToString("00") + ":" + secs.ToString("00");
        }
    }
}
