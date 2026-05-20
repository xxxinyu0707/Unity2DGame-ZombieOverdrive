using UnityEngine;

namespace ZombieOverdrive.Core
{
    public enum MetaTalent
    {
        Vitality,
        Mobility,
        Power,
        Magnet,
        Greed,
        SecondChance
    }

    public static class MetaProgression
    {
        public const int MaxTalentLevel = 5;
        private const string GoldKey = "ZombieOverdrive.MetaGold";
        private const string TalentPrefix = "ZombieOverdrive.Talent.";

        public static int Gold => PlayerPrefs.GetInt(GoldKey, 0);

        public static void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            PlayerPrefs.SetInt(GoldKey, Gold + amount);
            PlayerPrefs.Save();
        }

        public static int GetTalentLevel(MetaTalent talent)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(TalentPrefix + talent, 0), 0, MaxTalentLevel);
        }

        public static int GetUpgradeCost(MetaTalent talent)
        {
            int level = GetTalentLevel(talent);
            if (level >= MaxTalentLevel)
            {
                return 0;
            }

            return 120 + level * 90 + (level * level) * 35;
        }

        public static bool TryUpgrade(MetaTalent talent)
        {
            int level = GetTalentLevel(talent);
            if (level >= MaxTalentLevel)
            {
                return false;
            }

            int cost = GetUpgradeCost(talent);
            if (Gold < cost)
            {
                return false;
            }

            PlayerPrefs.SetInt(GoldKey, Gold - cost);
            PlayerPrefs.SetInt(TalentPrefix + talent, level + 1);
            PlayerPrefs.Save();
            return true;
        }

        public static void ApplyTo(PlayerStats stats)
        {
            if (stats == null)
            {
                return;
            }

            stats.AddMaxHealth(GetTalentLevel(MetaTalent.Vitality) * 12f);
            stats.AddMoveSpeed(GetTalentLevel(MetaTalent.Mobility) * 0.055f);
            stats.AddDamage(GetTalentLevel(MetaTalent.Power) * 0.06f);
            stats.AddMagnetRange(GetTalentLevel(MetaTalent.Magnet) * 0.32f);
            stats.AddGoldGain(GetTalentLevel(MetaTalent.Greed) * 0.1f);

            int secondChance = GetTalentLevel(MetaTalent.SecondChance);
            if (secondChance > 0)
            {
                stats.reviveCharges += 1;
                stats.reviveHealthPercent = 0.3f + (secondChance - 1) * 0.05f;
                stats.AddMoveSpeed((secondChance - 1) * 0.02f);
            }
        }

        public static string GetTalentName(MetaTalent talent)
        {
            switch (talent)
            {
                case MetaTalent.Vitality:
                    return "强化体质";
                case MetaTalent.Mobility:
                    return "机动训练";
                case MetaTalent.Power:
                    return "武器校准";
                case MetaTalent.Magnet:
                    return "磁场背包";
                case MetaTalent.Greed:
                    return "收益芯片";
                case MetaTalent.SecondChance:
                    return "备用生命";
                default:
                    return "未知天赋";
            }
        }

        public static string GetTalentDescription(MetaTalent talent)
        {
            switch (talent)
            {
                case MetaTalent.Vitality:
                    return "生命上限每级 +12。";
                case MetaTalent.Mobility:
                    return "移动速度每级 +5.5%。";
                case MetaTalent.Power:
                    return "全武器伤害每级 +6%。";
                case MetaTalent.Magnet:
                    return "初始拾取范围每级小幅提升。";
                case MetaTalent.Greed:
                    return "局内金币收益每级 +10%。";
                case MetaTalent.SecondChance:
                    return "解锁一次复活，并提升复活后的生命。";
                default:
                    return "";
            }
        }
    }
}
