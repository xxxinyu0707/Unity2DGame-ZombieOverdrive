using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZombieOverdrive.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        private Action onResume;
        private Action onRestart;
        private Action onQuit;

        public void Initialize(Action resume, Action restart, Action quit)
        {
            onResume = resume;
            onRestart = restart;
            onQuit = quit;

            resumeButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(() => onResume?.Invoke());
            restartButton.onClick.AddListener(() => onRestart?.Invoke());
            quitButton.onClick.AddListener(() => onQuit?.Invoke());
            Hide();
        }

        public void Show(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
