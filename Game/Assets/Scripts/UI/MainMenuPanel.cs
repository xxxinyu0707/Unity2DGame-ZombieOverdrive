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
            onStart = start;
            onQuit = quit;

            startButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => onStart?.Invoke());
            quitButton.onClick.AddListener(() => onQuit?.Invoke());

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
                goldText.text = "局外金币: <color=#ffd66b><b>" + MetaProgression.Gold + "</b></color>";
            }

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
    }
}
