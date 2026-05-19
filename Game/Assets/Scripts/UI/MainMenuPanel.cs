using System;
using UnityEngine;
using UnityEngine.UI;
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

        private Action onStart;
        private Action onQuit;

        public void Initialize(Action start, Action quit)
        {
            onStart = start;
            onQuit = quit;

            startButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => onStart?.Invoke());
            quitButton.onClick.AddListener(() => onQuit?.Invoke());

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
            }

            Refresh();
        }

        public void Show()
        {
            Refresh();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            if (goldText != null)
            {
                goldText.text = "局外金币 " + MetaProgression.Gold;
            }

            MetaTalent[] talents = GetTalents();
            for (int i = 0; i < talentTexts.Length && i < talents.Length; i++)
            {
                int level = MetaProgression.GetTalentLevel(talents[i]);
                int cost = MetaProgression.GetUpgradeCost(talents[i]);
                string costText = level >= MetaProgression.MaxTalentLevel ? "已满级" : "花费 " + cost;
                talentTexts[i].text = MetaProgression.GetTalentName(talents[i]) + "  " + level + "/" + MetaProgression.MaxTalentLevel
                    + "\n" + MetaProgression.GetTalentDescription(talents[i])
                    + "\n" + costText;
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
    }
}
