using System;
using UnityEngine;
using UnityEngine.UI;
using ZombieOverdrive.Audio;

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
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Text musicVolumeValueText;

        private Action onResume;
        private Action onRestart;
        private Action onQuit;
        private bool layoutFixed;
        private bool suppressVolumeEvent;

        public void Initialize(Action resume, Action restart, Action quit)
        {
            UIFontProvider.ApplyToChildren(gameObject);
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
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveAllListeners();
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            // Add dynamic outline scaling and hover effects to the pause menu buttons
            UIInteractiveEffect.AddTo(resumeButton, new Color(0f, 0.89f, 1f, 1f), new Color(1f, 1f, 1f, 0.08f));
            UIInteractiveEffect.AddTo(restartButton, new Color(1f, 0.78f, 0.22f, 1f), new Color(1f, 1f, 1f, 0.08f));
            UIInteractiveEffect.AddTo(quitButton, new Color(1f, 0.09f, 0.27f, 1f), new Color(1f, 1f, 1f, 0.08f));

            Hide();
        }

        public void Show(string status)
        {
            EnsureLayout();
            RefreshMusicVolume();
            if (statusText != null)
            {
                statusText.text = FormatStatusText(status);
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
                    if (hasText)
                    {
                        // Color levels in blue and super evolutions in gold
                        string formatted = texts[i];
                        formatted = formatted.Replace(" 超进化", " <color=#ffd66b><b>★超进化★</b></color>");
                        formatted = System.Text.RegularExpressions.Regex.Replace(formatted, @"等级 \d+", "<color=#29b6f6><b>$&</b></color>");
                        labels[i].text = formatted;
                    }
                    else
                    {
                        labels[i].text = "<color=#78909c>空槽</color>";
                    }
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

        private string FormatStatusText(string status)
        {
            if (string.IsNullOrEmpty(status)) return status;

            string[] lines = status.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("时间 "))
                {
                    line = System.Text.RegularExpressions.Regex.Replace(line, @"时间 (\d+:\d+)", "时间 <color=#00e5ff><b>$1</b></color>");
                    line = System.Text.RegularExpressions.Regex.Replace(line, @"等级 (\d+)", "等级 <color=#29b6f6><b>$1</b></color>");
                    line = System.Text.RegularExpressions.Regex.Replace(line, @"击杀 (\d+)", "击杀 <color=#ff7043><b>$1</b></color>");
                    lines[i] = line;
                }
                else if (line.Contains("超进化搭配"))
                {
                    lines[i] = "<b><color=#ffd66b>⚔️ 超进化搭配 ⚔️</color></b>";
                }
                else if (line.Trim().StartsWith("- "))
                {
                    line = line.Replace("已完成", "<color=#81c784>已完成</color>");
                    line = line.Replace("可进化", "<color=#ffd66b><b>可进化</b></color>");
                    line = line.Replace("准备中", "<color=#b0bec5>准备中</color>");
                    line = line.Replace("未获得", "<color=#78909c>未获得</color>");
                    line = line.Replace(" + ", " <color=#b0bec5>+</color> ");
                    lines[i] = line;
                }
                else if (line.Contains("ESC") || line.Contains("重开"))
                {
                    lines[i] = "<color=#90a4ae><i>" + line + "</i></color>";
                }
            }

            return string.Join("\n", lines);
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
                SetRect(statusRect, new Vector2(0f, 1f), new Vector2(24f, -62f), new Vector2(382f, 416f), new Vector2(0f, 1f));
                statusText.fontSize = 17;
                statusText.verticalOverflow = VerticalWrapMode.Truncate;
            }

            LayoutSlots(activeSlotIcons, activeSlotTexts, slotPanel, -62f);
            LayoutSlots(passiveSlotIcons, passiveSlotTexts, slotPanel, -310f);
            EnsureMusicVolumeControl(infoPanel);
            LayoutButton(resumeButton, frame, new Vector2(-240f, -304f));
            LayoutButton(restartButton, frame, new Vector2(0f, -304f));
            LayoutButton(quitButton, frame, new Vector2(240f, -304f));
            layoutFixed = true;
        }

        private void SetMusicVolume(float value)
        {
            if (suppressVolumeEvent)
            {
                return;
            }

            MusicManager.Instance?.SetNormalizedVolume(value);
            UpdateMusicVolumeText(value);
        }

        private void RefreshMusicVolume()
        {
            float value = MusicManager.Instance != null ? MusicManager.Instance.NormalizedVolume : 0.75f;
            suppressVolumeEvent = true;
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = value;
            }

            suppressVolumeEvent = false;
            UpdateMusicVolumeText(value);
        }

        private void UpdateMusicVolumeText(float value)
        {
            if (musicVolumeValueText != null)
            {
                musicVolumeValueText.text = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
            }
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
                text.font = UIFontProvider.Font;
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

        private void EnsureMusicVolumeControl(RectTransform infoPanel)
        {
            RectTransform container = FindDirectRect(infoPanel, "Music Volume Control");
            if (container == null)
            {
                GameObject containerObject = new GameObject("Music Volume Control", typeof(RectTransform), typeof(Image));
                containerObject.transform.SetParent(infoPanel, false);
                container = containerObject.GetComponent<RectTransform>();
                containerObject.GetComponent<Image>().color = new Color(0.035f, 0.05f, 0.075f, 0.82f);
            }

            SetRect(container, new Vector2(1f, 1f), new Vector2(-24f, -62f), new Vector2(240f, 104f), new Vector2(1f, 1f));
            Text label = GetOrCreateText(container, "Music Volume Label", "音乐音量", 18, TextAnchor.MiddleLeft);
            SetRect(label.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(128f, 28f), new Vector2(0f, 1f));

            musicVolumeValueText = GetOrCreateText(container, "Music Volume Value", "75%", 17, TextAnchor.MiddleRight);
            musicVolumeValueText.color = new Color(1f, 0.82f, 0.35f, 1f);
            SetRect(musicVolumeValueText.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-18f, -18f), new Vector2(76f, 28f), new Vector2(1f, 1f));

            musicVolumeSlider = GetOrCreateSlider(container, "Music Volume Slider");
            SetRect(musicVolumeSlider.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(18f, 18f), new Vector2(218f, 20f), new Vector2(0f, 0f));
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.wholeNumbers = false;
        }

        private static Text GetOrCreateText(RectTransform parent, string name, string value, int fontSize, TextAnchor alignment)
        {
            RectTransform rect = FindDirectRect(parent, name);
            Text text;
            if (rect == null)
            {
                GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
                textObject.transform.SetParent(parent, false);
                text = textObject.GetComponent<Text>();
                text.font = UIFontProvider.Font;
                text.color = Color.white;
            }
            else
            {
                text = rect.GetComponent<Text>();
            }

            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Slider GetOrCreateSlider(RectTransform parent, string name)
        {
            RectTransform rect = FindDirectRect(parent, name);
            if (rect != null)
            {
                return rect.GetComponent<Slider>();
            }

            GameObject sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            Slider slider = sliderObject.GetComponent<Slider>();
            slider.transition = Selectable.Transition.None;

            Image background = CreateSliderPart(sliderObject.transform, "Background", new Color(0.03f, 0.035f, 0.05f, 1f));
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);

            Image fill = CreateSliderPart(fillArea.transform, "Fill", new Color(0f, 0.82f, 1f, 1f));
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image handle = CreateSliderPart(sliderObject.transform, "Handle", new Color(1f, 0.82f, 0.35f, 1f));
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(14f, 30f);

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.value = 0.75f;
            return slider;
        }

        private static Image CreateSliderPart(Transform parent, string name, Color color)
        {
            GameObject part = new GameObject(name, typeof(RectTransform), typeof(Image));
            part.transform.SetParent(parent, false);
            Image image = part.GetComponent<Image>();
            image.color = color;
            return image;
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
