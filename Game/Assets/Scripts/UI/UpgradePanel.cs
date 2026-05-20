using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZombieOverdrive.Core;

namespace ZombieOverdrive.UI
{
    public class UpgradePanel : MonoBehaviour
    {
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private Image[] iconImages;
        [SerializeField] private Image[] accentImages;
        [SerializeField] private Text[] titleTexts;
        [SerializeField] private Text[] descriptionTexts;
        [SerializeField] private Text[] hintTexts;
        [SerializeField] private Button rerollButton;
        [SerializeField] private Text rerollText;
        [SerializeField] private UpgradeIconLibrary iconLibrary;

        private Action<UpgradeOption> onSelected;
        private Action onReroll;
        private List<UpgradeOption> currentOptions;
        private int currentRerollsRemaining;
        private bool layoutFixed;

        public void Show(List<UpgradeOption> options, Action<UpgradeOption> selectedCallback, Action rerollCallback, int rerollsRemaining)
        {
            currentOptions = options;
            onSelected = selectedCallback;
            onReroll = rerollCallback;
            currentRerollsRemaining = rerollsRemaining;
            gameObject.SetActive(true);
            EnsureLayout();
            RefreshRerollButton();

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                bool hasOption = i < currentOptions.Count;
                optionButtons[i].gameObject.SetActive(hasOption);
                optionButtons[i].onClick.RemoveAllListeners();

                if (!hasOption)
                {
                    continue;
                }

                titleTexts[i].text = currentOptions[i].Title;
                descriptionTexts[i].text = currentOptions[i].Description;
                if (hintTexts != null && i < hintTexts.Length && hintTexts[i] != null)
                {
                    hintTexts[i].text = currentOptions[i].Hint;
                    hintTexts[i].gameObject.SetActive(!string.IsNullOrWhiteSpace(currentOptions[i].Hint));
                }

                if (iconImages != null && i < iconImages.Length && iconImages[i] != null)
                {
                    iconImages[i].sprite = iconLibrary != null ? iconLibrary.Get(currentOptions[i].IconId) : null;
                    iconImages[i].enabled = iconImages[i].sprite != null;
                }

                if (accentImages != null && i < accentImages.Length && accentImages[i] != null)
                {
                    accentImages[i].color = currentOptions[i].Highlight
                        ? new Color(1f, 0.78f, 0.22f, 1f)
                        : new Color(0.28f, 0.56f, 0.78f, 1f);
                }

                optionButtons[i].onClick.AddListener(() => Select(index));
            }
        }

        public void Refresh(List<UpgradeOption> options, int rerollsRemaining)
        {
            Show(options, onSelected, onReroll, rerollsRemaining);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Select(int index)
        {
            if (currentOptions == null || index < 0 || index >= currentOptions.Count)
            {
                return;
            }

            onSelected?.Invoke(currentOptions[index]);
        }

        private void RefreshRerollButton()
        {
            if (rerollButton == null)
            {
                return;
            }

            rerollButton.onClick.RemoveAllListeners();
            bool canReroll = onReroll != null && currentRerollsRemaining > 0;
            rerollButton.interactable = canReroll;
            if (rerollText != null)
            {
                rerollText.text = currentRerollsRemaining > 0
                    ? "刷新词条 " + currentRerollsRemaining + "/3"
                    : "刷新已用完";
            }

            if (canReroll)
            {
                rerollButton.onClick.AddListener(() => onReroll?.Invoke());
            }
        }

        private void EnsureLayout()
        {
            if (layoutFixed)
            {
                return;
            }

            RectTransform frame = FindDirectRect(transform, "Frame");
            if (frame != null)
            {
                SetRect(frame, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1280f, 640f), new Vector2(0.5f, 0.5f));
            }

            RectTransform topRule = FindDirectRect(transform, "Top Rule");
            if (topRule != null)
            {
                SetRect(topRule, new Vector2(0.5f, 0.5f), new Vector2(0f, 286f), new Vector2(1280f, 6f), new Vector2(0.5f, 0.5f));
            }

            RectTransform title = FindDirectRect(transform, "Title");
            if (title != null)
            {
                SetRect(title, new Vector2(0.5f, 0.5f), new Vector2(0f, 236f), new Vector2(560f, 58f), new Vector2(0.5f, 0.5f));
                Text titleText = title.GetComponent<Text>();
                if (titleText != null)
                {
                    ConfigureText(titleText, 38, 30, 38, TextAnchor.MiddleCenter);
                }
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == null)
                {
                    continue;
                }

                RectTransform buttonRect = optionButtons[i].GetComponent<RectTransform>();
                SetRect(buttonRect, new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 390f, -48f), new Vector2(340f, 390f), new Vector2(0.5f, 0.5f));

                Transform accent = buttonRect.Find("Accent");
                if (accent != null)
                {
                    SetRect((RectTransform)accent, new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(330f, 8f), new Vector2(0.5f, 1f));
                }

                if (titleTexts != null && i < titleTexts.Length && titleTexts[i] != null)
                {
                    SetRect(titleTexts[i].GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(294f, 72f), new Vector2(0.5f, 1f));
                    ConfigureText(titleTexts[i], 24, 16, 24, TextAnchor.MiddleCenter);
                }

                RectTransform iconBackdrop = FindDirectRect(buttonRect, "Icon Backdrop");
                if (iconBackdrop != null)
                {
                    SetRect(iconBackdrop, new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(82f, 82f), new Vector2(0.5f, 1f));
                }

                if (iconImages != null && i < iconImages.Length && iconImages[i] != null)
                {
                    SetRect(iconImages[i].GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(70f, 70f), new Vector2(0.5f, 1f));
                    iconImages[i].preserveAspect = true;
                }

                if (descriptionTexts != null && i < descriptionTexts.Length && descriptionTexts[i] != null)
                {
                    SetRect(descriptionTexts[i].GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -214f), new Vector2(292f, 96f), new Vector2(0.5f, 1f));
                    ConfigureText(descriptionTexts[i], 19, 13, 19, TextAnchor.UpperCenter);
                    descriptionTexts[i].lineSpacing = 0.9f;
                }

                if (hintTexts != null && i < hintTexts.Length && hintTexts[i] != null)
                {
                    SetRect(hintTexts[i].GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(294f, 62f), new Vector2(0.5f, 0f));
                    ConfigureText(hintTexts[i], 18, 13, 18, TextAnchor.MiddleCenter);
                    hintTexts[i].lineSpacing = 0.9f;
                }
            }

            if (rerollButton != null)
            {
                RectTransform rerollRect = rerollButton.GetComponent<RectTransform>();
                SetRect(rerollRect, new Vector2(0.5f, 0.5f), new Vector2(0f, -284f), new Vector2(252f, 52f), new Vector2(0.5f, 0.5f));
            }

            if (rerollText != null)
            {
                SetRect(rerollText.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(228f, 42f), new Vector2(0.5f, 0.5f));
                ConfigureText(rerollText, 24, 18, 24, TextAnchor.MiddleCenter);
            }

            layoutFixed = true;
        }

        private static void ConfigureText(Text text, int fontSize, int minSize, int maxSize, TextAnchor alignment)
        {
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minSize;
            text.resizeTextMaxSize = maxSize;
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
    }
}
