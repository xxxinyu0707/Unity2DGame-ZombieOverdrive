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
        [SerializeField] private Text[] titleTexts;
        [SerializeField] private Text[] descriptionTexts;

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
