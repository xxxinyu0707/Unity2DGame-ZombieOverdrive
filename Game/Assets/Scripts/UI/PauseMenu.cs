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
        private bool layoutFixed;

        public void Initialize(Action resume, Action restart, Action quit)
        {
            onResume = resume;
            onRestart = restart;
            onQuit = quit;
            EnsureLayout();

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
            EnsureLayout();
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
                    labels[i].fontSize = 17;
                    labels[i].verticalOverflow = VerticalWrapMode.Truncate;
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

            SetRect(frame, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1180f, 720f), new Vector2(0.5f, 0.5f));
            Transform topRule = frame.Find("Top Rule");
            if (topRule != null)
            {
                SetRect((RectTransform)topRule, new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(1180f, 6f), new Vector2(0.5f, 1f));
            }

            RectTransform title = FindDirectRect(transform, "Title");
            if (title != null)
            {
                title.SetParent(frame, false);
                SetRect(title, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(420f, 54f), new Vector2(0.5f, 1f));
                Text text = title.GetComponent<Text>();
                if (text != null)
                {
                    text.fontSize = 38;
                    text.verticalOverflow = VerticalWrapMode.Truncate;
                }
            }

            RectTransform slotPanel = GetOrCreatePanel(frame, "Slot Panel", new Vector2(34f, -86f), new Vector2(390f, 510f));
            RectTransform infoPanel = GetOrCreatePanel(frame, "Status Panel", new Vector2(456f, -86f), new Vector2(690f, 510f));
            MoveTitle("Active Slots Title", slotPanel, new Vector2(22f, -18f));
            MoveTitle("Passive Slots Title", slotPanel, new Vector2(22f, -266f));
            EnsurePanelTitle(infoPanel, "Status Title", "作战信息", new Vector2(24f, -18f));

            if (statusText != null)
            {
                RectTransform statusRect = statusText.GetComponent<RectTransform>();
                statusRect.SetParent(infoPanel, false);
                SetRect(statusRect, new Vector2(0f, 1f), new Vector2(24f, -62f), new Vector2(642f, 416f), new Vector2(0f, 1f));
                statusText.fontSize = 17;
                statusText.verticalOverflow = VerticalWrapMode.Truncate;
            }

            LayoutSlots(activeSlotIcons, activeSlotTexts, slotPanel, -62f);
            LayoutSlots(passiveSlotIcons, passiveSlotTexts, slotPanel, -310f);
            LayoutButton(resumeButton, frame, new Vector2(-240f, -304f));
            LayoutButton(restartButton, frame, new Vector2(0f, -304f));
            LayoutButton(quitButton, frame, new Vector2(240f, -304f));
            layoutFixed = true;
        }

        private void MoveTitle(string name, RectTransform parent, Vector2 position)
        {
            RectTransform rect = FindDirectRect(transform, name);
            if (rect == null)
            {
                rect = FindDirectRect(parent, name);
            }

            if (rect == null)
            {
                return;
            }

            rect.SetParent(parent, false);
            SetRect(rect, new Vector2(0f, 1f), position, new Vector2(330f, 34f), new Vector2(0f, 1f));
            Text text = rect.GetComponent<Text>();
            if (text != null)
            {
                text.fontSize = 22;
                text.verticalOverflow = VerticalWrapMode.Truncate;
            }
        }

        private static void LayoutSlots(Image[] icons, Text[] labels, RectTransform parent, float startY)
        {
            int count = labels != null ? labels.Length : 0;
            for (int i = 0; i < count; i++)
            {
                RectTransform slot = labels[i] != null ? labels[i].transform.parent as RectTransform : null;
                if (slot == null)
                {
                    continue;
                }

                slot.SetParent(parent, false);
                SetRect(slot, new Vector2(0f, 1f), new Vector2(22f, startY - i * 64f), new Vector2(346f, 56f), new Vector2(0f, 1f));
                RectTransform labelRect = labels[i].GetComponent<RectTransform>();
                SetRect(labelRect, new Vector2(0f, 0.5f), new Vector2(66f, 0f), new Vector2(264f, 46f), new Vector2(0f, 0.5f));

                if (icons != null && i < icons.Length && icons[i] != null)
                {
                    RectTransform iconRect = icons[i].GetComponent<RectTransform>();
                    SetRect(iconRect, new Vector2(0f, 0.5f), new Vector2(15f, 0f), new Vector2(36f, 36f), new Vector2(0f, 0.5f));
                    Transform backdrop = slot.Find("Icon Backdrop");
                    if (backdrop != null)
                    {
                        SetRect((RectTransform)backdrop, new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(42f, 42f), new Vector2(0f, 0.5f));
                    }
                }
            }
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

        private static RectTransform GetOrCreatePanel(RectTransform frame, string name, Vector2 position, Vector2 size)
        {
            RectTransform panel = FindDirectRect(frame, name);
            if (panel == null)
            {
                GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
                panelObject.transform.SetParent(frame, false);
                panel = panelObject.GetComponent<RectTransform>();
                panelObject.GetComponent<Image>().color = new Color(0.07f, 0.085f, 0.12f, 0.94f);
            }

            SetRect(panel, new Vector2(0f, 1f), position, size, new Vector2(0f, 1f));
            return panel;
        }

        private static void EnsurePanelTitle(RectTransform parent, string name, string value, Vector2 position)
        {
            RectTransform rect = FindDirectRect(parent, name);
            Text text;
            if (rect == null)
            {
                GameObject titleObject = new GameObject(name, typeof(RectTransform), typeof(Text));
                titleObject.transform.SetParent(parent, false);
                rect = titleObject.GetComponent<RectTransform>();
                text = titleObject.GetComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.color = Color.white;
                text.alignment = TextAnchor.UpperLeft;
            }
            else
            {
                text = rect.GetComponent<Text>();
            }

            SetRect(rect, new Vector2(0f, 1f), position, new Vector2(330f, 34f), new Vector2(0f, 1f));
            if (text != null)
            {
                text.text = value;
                text.fontSize = 22;
                text.verticalOverflow = VerticalWrapMode.Truncate;
            }
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
