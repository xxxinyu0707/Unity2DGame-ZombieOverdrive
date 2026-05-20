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
        private bool layoutFixed;

        public void Initialize(Action restart, Action quit)
        {
            UIFontProvider.ApplyToChildren(gameObject);
            onRestart = restart;
            onQuit = quit;
            EnsureLayout();
            restartButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => onRestart?.Invoke());
            quitButton.onClick.AddListener(() => onQuit?.Invoke());

            // Add dynamic outline scaling and hover effects to the result buttons
            UIInteractiveEffect.AddTo(restartButton, new Color(0.12f, 0.75f, 0.42f, 1f), new Color(1f, 1f, 1f, 0.08f));
            UIInteractiveEffect.AddTo(quitButton, new Color(1f, 0.09f, 0.27f, 1f), new Color(1f, 1f, 1f, 0.08f));

            Hide();
        }

        public void Show(bool victory, int level, int kills, int runGold, int reward, float elapsedSeconds)
        {
            EnsureLayout();
            if (titleText != null)
            {
                titleText.text = victory ? "★ 任务完成 ★" : "☠ 行动终止 ☠";
                titleText.color = victory ? new Color(0.23f, 0.85f, 0.45f, 1f) : new Color(0.93f, 0.26f, 0.31f, 1f);
            }

            if (summaryText != null)
            {
                summaryText.text = $"生存时间:  <color=#00e5ff><b>{FormatTime(elapsedSeconds)}</b></color>\n"
                    + $"最终等级:  <color=#29b6f6><b>{level}</b></color>\n"
                    + $"击杀数量:  <color=#ff7043><b>{kills}</b></color>\n"
                    + $"局内金币:  <color=#ffd54f><b>{runGold}</b></color> <color=#b0bec5>(汇率 x1)</color>\n"
                    + $"总结算金币:  <color=#ffd54f><b>{reward}</b></color> 💰";
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

        private void EnsureLayout()
        {
            if (layoutFixed)
            {
                return;
            }

            RectTransform frame = FindDirectRect(transform, "Frame");
            if (frame == null)
            {
                return;
            }

            SetRect(frame, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 520f), new Vector2(0.5f, 0.5f));
            Transform topRule = frame.Find("Top Rule");
            if (topRule != null)
            {
                SetRect((RectTransform)topRule, new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(680f, 6f), new Vector2(0.5f, 1f));
            }

            if (titleText != null)
            {
                RectTransform title = titleText.GetComponent<RectTransform>();
                title.SetParent(frame, false);
                SetRect(title, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(520f, 60f), new Vector2(0.5f, 1f));
                titleText.fontSize = 42;
                titleText.verticalOverflow = VerticalWrapMode.Truncate;
            }

            RectTransform summaryBox = GetOrCreatePanel(frame, "Summary Box", new Vector2(0f, -126f), new Vector2(500f, 190f));
            if (summaryText != null)
            {
                RectTransform summary = summaryText.GetComponent<RectTransform>();
                summary.SetParent(summaryBox, false);
                SetRect(summary, new Vector2(0f, 1f), new Vector2(24f, -18f), new Vector2(452f, 154f), new Vector2(0f, 1f));
                summaryText.fontSize = 24;
                summaryText.verticalOverflow = VerticalWrapMode.Truncate;
            }

            LayoutButton(restartButton, frame, new Vector2(-120f, -198f));
            LayoutButton(quitButton, frame, new Vector2(120f, -198f));
            layoutFixed = true;
        }

        private static RectTransform GetOrCreatePanel(RectTransform frame, string name, Vector2 position, Vector2 size)
        {
            RectTransform panel = FindDirectRect(frame, name);
            if (panel == null)
            {
                GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
                panelObject.transform.SetParent(frame, false);
                panel = panelObject.GetComponent<RectTransform>();
                panelObject.GetComponent<Image>().color = new Color(0.07f, 0.085f, 0.12f, 0.95f);
            }

            SetRect(panel, new Vector2(0.5f, 1f), position, size, new Vector2(0.5f, 1f));
            return panel;
        }

        private static void LayoutButton(Button button, RectTransform parent, Vector2 position)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetRect(rect, new Vector2(0.5f, 0.5f), position, new Vector2(188f, 64f), new Vector2(0.5f, 0.5f));
        }

        private static RectTransform FindDirectRect(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            Transform child = parent.Find(name);
            return child != null ? child as RectTransform : null;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
