using System;
using UnityEngine;
using UnityEngine.UI;
using ZombieOverdrive.Audio;
using ZombieOverdrive.Core;

namespace ZombieOverdrive.UI
{
    public class MainMenuPanel : MonoBehaviour
    {
        [SerializeField] private Text goldText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button[] talentButtons;
        [SerializeField] private Text[] talentTexts;
        [SerializeField] private Button volumeButton;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeText;

        private Action onStart;
        private Action onQuit;
        private bool layoutFixed;
        private bool suppressVolumeEvent;

        private static readonly Color[] TalentColors = {
            new Color(0.83f, 0.18f, 0.23f, 1f), // Vitality - Red
            new Color(0.12f, 0.65f, 0.8f, 1f),  // Mobility - Blue/Cyan
            new Color(0.96f, 0.64f, 0.16f, 1f), // Power - Orange/Yellow
            new Color(0.55f, 0.42f, 0.96f, 1f), // Magnet - Purple
            new Color(0.23f, 0.74f, 0.42f, 1f), // Greed - Green
            new Color(0.88f, 0.42f, 0.82f, 1f)  // SecondChance - Pink/Magenta
        };

        public void Initialize(Action start, Action quit)
        {
            UIFontProvider.ApplyToChildren(gameObject);
            EnsureLayout();
            onStart = start;
            onQuit = quit;

            startButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => onStart?.Invoke());
            quitButton.onClick.AddListener(() => onQuit?.Invoke());
            EnsureVolumeControl();
            if (volumeButton != null)
            {
                volumeButton.onClick.RemoveAllListeners();
                volumeButton.onClick.AddListener(CycleVolume);
            }

            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.RemoveAllListeners();
                volumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            // Add polished interactive animations to buttons
            UIInteractiveEffect.AddTo(startButton, new Color(0f, 0.89f, 1f, 1f), new Color(0.13f, 0.71f, 0.84f, 0.5f));
            UIInteractiveEffect.AddTo(quitButton, new Color(1f, 0.09f, 0.27f, 1f), new Color(0.45f, 0.45f, 0.45f, 0.3f));

            MetaTalent[] talents = GetTalents();
            for (int i = 0; i < talentButtons.Length && i < talents.Length; i++)
            {
                int index = i;
                talentButtons[i].onClick.RemoveAllListeners();
                talentButtons[i].onClick.AddListener(() =>
                {
                    MetaProgression.TryUpgrade(talents[index]);
                    Refresh();
                });

                // Set up interactive outline hover based on talent color theme
                Color talentColor = index < TalentColors.Length ? TalentColors[index] : Color.cyan;
                UIInteractiveEffect.AddTo(talentButtons[i], talentColor, talentColor * 0.35f);
            }

            Refresh();
        }

        public void Show()
        {
            EnsureLayout();
            EnsureVolumeControl();
            Refresh();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            if (goldText != null)
            {
                goldText.text = "金币: <color=#ffd66b><b>" + MetaProgression.Gold + "</b></color>";
            }

            RefreshMusicVolume();

            MetaTalent[] talents = GetTalents();
            for (int i = 0; i < talentTexts.Length && i < talents.Length; i++)
            {
                int level = MetaProgression.GetTalentLevel(talents[i]);
                int cost = MetaProgression.GetUpgradeCost(talents[i]);

                // Build a beautiful progress bar using block icons
                string progressBar = "  ";
                for (int j = 1; j <= MetaProgression.MaxTalentLevel; j++)
                {
                    if (j <= level)
                    {
                        progressBar += "<color=#23b7d7>■</color> ";
                    }
                    else
                    {
                        progressBar += "<color=#37474f>□</color> ";
                    }
                }

                string name = MetaProgression.GetTalentName(talents[i]);
                string desc = MetaProgression.GetTalentDescription(talents[i]);
                string costText;

                if (level >= MetaProgression.MaxTalentLevel)
                {
                    costText = "<color=#90a4ae><b>[ 已满级 ]</b></color>";
                }
                else
                {
                    bool canAfford = MetaProgression.Gold >= cost;
                    costText = canAfford
                        ? $"升级花费: <color=#ffd66b><b>{cost}</b></color> 金币"
                        : $"升级花费: <color=#ef5350><b>{cost}</b></color> 金币 (余额不足)";
                }

                talentTexts[i].text = $"<b>{name}</b>    {level}/{MetaProgression.MaxTalentLevel}{progressBar}\n" +
                                      $"<color=#b0bec5>{desc}</color>\n" +
                                      $"{costText}";

                if (talentButtons != null && i < talentButtons.Length && talentButtons[i] != null)
                {
                    talentButtons[i].interactable = level < MetaProgression.MaxTalentLevel && MetaProgression.Gold >= cost;
                }
            }
        }

        private static MetaTalent[] GetTalents()
        {
            return new[]
            {
                MetaTalent.Vitality,
                MetaTalent.Mobility,
                MetaTalent.Power,
                MetaTalent.Magnet,
                MetaTalent.Greed,
                MetaTalent.SecondChance
            };
        }

        private void EnsureLayout()
        {
            if (layoutFixed)
            {
                return;
            }

            RectTransform title = FindDirectRect(transform, "Title");
            if (title != null)
            {
                SetRect(title, new Vector2(0f, 1f), new Vector2(56f, -24f), new Vector2(560f, 64f), new Vector2(0f, 1f));
                Text titleText = title.GetComponent<Text>();
                if (titleText != null)
                {
                    titleText.text = "基因重组舱";
                    ConfigureText(titleText, 44, 34, 44, TextAnchor.UpperLeft);
                    titleText.color = Color.white;
                    titleText.gameObject.SetActive(true);
                }
            }

            RectTransform subtitle = FindDirectRect(transform, "Subtitle");
            if (subtitle != null)
            {
                SetRect(subtitle, new Vector2(0f, 1f), new Vector2(58f, -92f), new Vector2(760f, 34f), new Vector2(0f, 1f));
                Text subtitleText = subtitle.GetComponent<Text>();
                if (subtitleText != null)
                {
                    ConfigureText(subtitleText, 18, 14, 18, TextAnchor.UpperLeft);
                    subtitleText.color = new Color(0.56f, 0.63f, 0.71f, 1f);
                }
            }

            if (goldText != null)
            {
                RectTransform goldRect = goldText.GetComponent<RectTransform>();
                SetRect(goldRect, new Vector2(1f, 1f), new Vector2(-76f, -70f), new Vector2(300f, 52f), new Vector2(1f, 1f));
                ConfigureText(goldText, 26, 20, 26, TextAnchor.MiddleRight);
                goldText.color = new Color(1f, 0.84f, 0.42f, 1f);
            }

            layoutFixed = true;
        }

        private void EnsureVolumeControl()
        {
            RectTransform container = FindDirectRect(transform, "Music Volume Control");
            if (container == null)
            {
                GameObject containerObject = new GameObject("Music Volume Control", typeof(RectTransform), typeof(Image));
                containerObject.transform.SetParent(transform, false);
                container = containerObject.GetComponent<RectTransform>();
                containerObject.GetComponent<Image>().color = new Color(0.045f, 0.06f, 0.085f, 0.92f);
            }

            SetRect(container, new Vector2(1f, 1f), new Vector2(-76f, -118f), new Vector2(300f, 44f), new Vector2(1f, 1f));
            volumeButton = GetOrCreateVolumeButton(container);
            SetRect(volumeButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(86f, 44f), new Vector2(0f, 0.5f));

            volumeSlider = GetOrCreateSlider(container, "Music Volume Slider");
            SetRect(volumeSlider.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(142f, 18f), new Vector2(0f, 0.5f));

            volumeText = GetOrCreateText(container, "Music Volume Text", "75%", 15, TextAnchor.MiddleRight);
            SetRect(volumeText.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(52f, 30f), new Vector2(1f, 0.5f));
            volumeText.color = new Color(1f, 0.82f, 0.35f, 1f);
        }

        private Button GetOrCreateVolumeButton(RectTransform parent)
        {
            RectTransform rect = FindDirectRect(parent, "Music Volume Button");
            if (rect != null)
            {
                return rect.GetComponent<Button>();
            }

            GameObject buttonObject = new GameObject("Music Volume Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.09f, 0.11f, 0.16f, 1f);
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.14f, 0.18f, 0.24f, 1f);
            colors.pressedColor = new Color(0.05f, 0.07f, 0.1f, 1f);
            button.colors = colors;
            Text label = GetOrCreateText(buttonObject.GetComponent<RectTransform>(), "Label", "音乐", 16, TextAnchor.MiddleCenter);
            SetRect(label.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(82f, 34f), new Vector2(0.5f, 0.5f));
            return button;
        }

        private void CycleVolume()
        {
            float current = MusicManager.Instance != null ? MusicManager.Instance.NormalizedVolume : 0.75f;
            float next = current > 0.66f ? 0.35f : current > 0.05f ? 0f : 0.75f;
            SetMusicVolume(next);
            RefreshMusicVolume();
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
            if (volumeSlider != null)
            {
                volumeSlider.value = value;
            }

            suppressVolumeEvent = false;
            UpdateMusicVolumeText(value);
        }

        private void UpdateMusicVolumeText(float value)
        {
            if (volumeText != null)
            {
                volumeText.text = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
            }
        }

        private static RectTransform FindDirectRect(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            Transform child = parent.Find(name);
            return child != null ? child.GetComponent<RectTransform>() : null;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void ConfigureText(Text text, int fontSize, int minSize, int maxSize, TextAnchor alignment)
        {
            if (text == null)
            {
                return;
            }

            UIFontProvider.ApplyTo(text);
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minSize;
            text.resizeTextMaxSize = maxSize;
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
            text.supportRichText = true;
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
            slider.minValue = 0f;
            slider.maxValue = 1f;

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
            handleRect.sizeDelta = new Vector2(12f, 26f);

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
    }
}
