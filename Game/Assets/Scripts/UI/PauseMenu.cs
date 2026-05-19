using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZombieOverdrive.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Image[] activeSlotIcons;
        [SerializeField] private Text[] activeSlotTexts;
        [SerializeField] private Image[] passiveSlotIcons;
        [SerializeField] private Text[] passiveSlotTexts;
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

        public void SetSlots(Sprite[] activeIcons, string[] activeTexts, Sprite[] passiveIcons, string[] passiveTexts)
        {
            ApplySlots(activeSlotIcons, activeSlotTexts, activeIcons, activeTexts);
            ApplySlots(passiveSlotIcons, passiveSlotTexts, passiveIcons, passiveTexts);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static void ApplySlots(Image[] icons, Text[] labels, Sprite[] sprites, string[] texts)
        {
            int count = labels != null ? labels.Length : 0;
            for (int i = 0; i < count; i++)
            {
                bool hasText = texts != null && i < texts.Length && !string.IsNullOrWhiteSpace(texts[i]);
                if (labels[i] != null)
                {
                    labels[i].text = hasText ? texts[i] : "空槽";
                }

                if (icons != null && i < icons.Length && icons[i] != null)
                {
                    Sprite sprite = sprites != null && i < sprites.Length ? sprites[i] : null;
                    icons[i].sprite = sprite;
                    icons[i].enabled = sprite != null;
                    icons[i].color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.12f);
                }
            }
        }
    }
}
