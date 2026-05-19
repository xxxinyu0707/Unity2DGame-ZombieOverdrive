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
        [SerializeField] private UpgradeIconLibrary iconLibrary;

        private Action<UpgradeOption> onSelected;
        private List<UpgradeOption> currentOptions;

        public void Show(List<UpgradeOption> options, Action<UpgradeOption> selectedCallback)
        {
            currentOptions = options;
            onSelected = selectedCallback;
            gameObject.SetActive(true);

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
    }
}
