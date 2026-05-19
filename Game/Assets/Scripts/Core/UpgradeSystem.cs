using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ZombieOverdrive.Combat;

namespace ZombieOverdrive.Core
{
    public enum UpgradeType
    {
        Weapon,
        AmmoBox,
        Overclock,
        Adrenaline,
        NanoArmor,
        Propellent,
        GravityCore,
        Magnet,
        HazmatSuit,
        GreedChip,
        Radar,
        Defibrillator,
        Radio,
        Repair
    }

    public struct UpgradeOption
    {
        public UpgradeType Type;
        public WeaponId WeaponId;
        public string Title;
        public string Description;
    }

    public class UpgradeSystem : MonoBehaviour
    {
        [SerializeField] private int optionsPerLevel = 3;
        [SerializeField] private int maxActiveWeapons = 3;
        [SerializeField] private int maxPassiveSkills = 3;

        private readonly List<UpgradeType> passivePool = new List<UpgradeType>
        {
            UpgradeType.AmmoBox,
            UpgradeType.Overclock,
            UpgradeType.Adrenaline,
            UpgradeType.NanoArmor,
            UpgradeType.Propellent,
            UpgradeType.GravityCore,
            UpgradeType.Magnet,
            UpgradeType.HazmatSuit,
            UpgradeType.GreedChip,
            UpgradeType.Radar,
            UpgradeType.Defibrillator,
            UpgradeType.Radio
        };

        private readonly Dictionary<UpgradeType, int> passiveLevels = new Dictionary<UpgradeType, int>();
        private readonly Dictionary<WeaponId, WeaponBase> weapons = new Dictionary<WeaponId, WeaponBase>();
        private readonly List<WeaponId> activeSlots = new List<WeaponId>();
        private readonly List<UpgradeType> passiveSlots = new List<UpgradeType>();

        private PlayerStats stats;
        private PlayerHealth health;

        public void Initialize(PlayerStats playerStats, IEnumerable<WeaponBase> weaponComponents, PlayerHealth playerHealth)
        {
            stats = playerStats;
            health = playerHealth;
            weapons.Clear();
            activeSlots.Clear();
            passiveSlots.Clear();
            passiveLevels.Clear();

            foreach (WeaponBase weapon in weaponComponents)
            {
                if (weapon == null)
                {
                    continue;
                }

                weapons[weapon.Id] = weapon;
            }

            UnlockInitialWeapon(WeaponId.Pistol);
        }

        public List<UpgradeOption> RollOptions()
        {
            List<UpgradeOption> options = new List<UpgradeOption>();
            List<WeaponBase> weaponCandidates = GetWeaponCandidates();
            List<UpgradeType> passiveCandidates = GetPassiveCandidates();

            for (int i = 0; i < optionsPerLevel; i++)
            {
                bool chooseWeapon = weaponCandidates.Count > 0 && (i == 0 || Random.value < 0.45f);
                if (chooseWeapon)
                {
                    options.Add(TakeWeaponOption(weaponCandidates));
                    continue;
                }

                if (passiveCandidates.Count > 0)
                {
                    options.Add(TakePassiveOption(passiveCandidates));
                    continue;
                }

                if (weaponCandidates.Count > 0)
                {
                    options.Add(TakeWeaponOption(weaponCandidates));
                }
            }

            if (options.Count == 0)
            {
                options.Add(new UpgradeOption
                {
                    Type = UpgradeType.Repair,
                    Title = "紧急修复",
                    Description = "已选槽位都达到满级。恢复 35% 生命。"
                });
            }

            return options;
        }

        public void Apply(UpgradeOption option)
        {
            switch (option.Type)
            {
                case UpgradeType.Weapon:
                    ApplyWeapon(option.WeaponId);
                    break;
                case UpgradeType.AmmoBox:
                    AddPassiveLevel(option.Type);
                    stats.AddDamage(0.08f);
                    stats.AddArea(0.05f);
                    break;
                case UpgradeType.Overclock:
                    AddPassiveLevel(option.Type);
                    stats.AddFireRate(0.09f);
                    break;
                case UpgradeType.Adrenaline:
                    AddPassiveLevel(option.Type);
                    stats.AddMoveSpeed(0.07f);
                    break;
                case UpgradeType.NanoArmor:
                    AddPassiveLevel(option.Type);
                    stats.AddDamageReduction(0.06f);
                    break;
                case UpgradeType.Propellent:
                    AddPassiveLevel(option.Type);
                    stats.AddProjectileSpeed(0.12f);
                    stats.bulletPierceBonus++;
                    break;
                case UpgradeType.GravityCore:
                    AddPassiveLevel(option.Type);
                    stats.AddDuration(0.12f);
                    break;
                case UpgradeType.Magnet:
                    AddPassiveLevel(option.Type);
                    stats.AddMagnetRange(1.25f);
                    break;
                case UpgradeType.HazmatSuit:
                    AddPassiveLevel(option.Type);
                    stats.AddMaxHealth(18f);
                    stats.AddHealthRegen(0.35f);
                    if (health != null) health.Heal(health.MaxHealth * 0.18f);
                    break;
                case UpgradeType.GreedChip:
                    AddPassiveLevel(option.Type);
                    stats.AddXpGain(0.08f);
                    break;
                case UpgradeType.Radar:
                    AddPassiveLevel(option.Type);
                    stats.AddCritical(0.04f, 0.1f);
                    break;
                case UpgradeType.Defibrillator:
                    AddPassiveLevel(option.Type);
                    if (GetPassiveLevel(option.Type) == 1)
                    {
                        stats.reviveCharges++;
                    }

                    stats.levelUpHealPercent += 0.015f;
                    break;
                case UpgradeType.Radio:
                    AddPassiveLevel(option.Type);
                    stats.AddPickupLuck(0.1f);
                    break;
                case UpgradeType.Repair:
                    if (health != null) health.Heal(health.MaxHealth * 0.35f);
                    break;
            }
        }

        public string BuildStatusText(int level, int kills, float elapsedSeconds)
        {
            StringBuilder builder = new StringBuilder(512);
            builder.AppendLine("作战状态");
            builder.AppendLine("时间 " + FormatTime(elapsedSeconds) + "   等级 " + level + "   击杀 " + kills);
            builder.AppendLine();
            builder.AppendLine("主动武器 " + activeSlots.Count + "/" + maxActiveWeapons);
            for (int i = 0; i < maxActiveWeapons; i++)
            {
                if (i < activeSlots.Count && weapons.TryGetValue(activeSlots[i], out WeaponBase weapon))
                {
                    builder.AppendLine("- " + GetWeaponName(activeSlots[i]) + " 等级 " + weapon.Level);
                }
                else
                {
                    builder.AppendLine("- [空槽]");
                }
            }

            builder.AppendLine();
            builder.AppendLine("被动技能 " + passiveSlots.Count + "/" + maxPassiveSkills);
            for (int i = 0; i < maxPassiveSkills; i++)
            {
                if (i < passiveSlots.Count)
                {
                    UpgradeType passive = passiveSlots[i];
                    builder.AppendLine("- " + GetPassiveName(passive) + " 等级 " + GetPassiveLevel(passive));
                }
                else
                {
                    builder.AppendLine("- [空槽]");
                }
            }

            builder.AppendLine();
            builder.AppendLine("按 ESC 或继续按钮返回游戏。重新开始会开启新一局。");
            return builder.ToString();
        }

        private void UnlockInitialWeapon(WeaponId id)
        {
            if (!weapons.TryGetValue(id, out WeaponBase weapon))
            {
                return;
            }

            if (!activeSlots.Contains(id))
            {
                activeSlots.Add(id);
            }

            if (!weapon.IsUnlocked)
            {
                weapon.UnlockOrLevel();
            }
        }

        private void ApplyWeapon(WeaponId id)
        {
            if (!weapons.TryGetValue(id, out WeaponBase weapon))
            {
                return;
            }

            if (!weapon.IsUnlocked)
            {
                if (activeSlots.Count >= maxActiveWeapons)
                {
                    return;
                }

                activeSlots.Add(id);
            }

            weapon.UnlockOrLevel();
        }

        private UpgradeOption TakeWeaponOption(List<WeaponBase> candidates)
        {
            int index = Random.Range(0, candidates.Count);
            WeaponBase weapon = candidates[index];
            candidates.RemoveAt(index);
            return CreateWeaponOption(weapon);
        }

        private UpgradeOption TakePassiveOption(List<UpgradeType> candidates)
        {
            int index = Random.Range(0, candidates.Count);
            UpgradeType type = candidates[index];
            candidates.RemoveAt(index);
            return CreatePassiveOption(type);
        }

        private List<WeaponBase> GetWeaponCandidates()
        {
            List<WeaponBase> candidates = new List<WeaponBase>();
            foreach (WeaponBase weapon in weapons.Values)
            {
                if (weapon == null || !weapon.CanLevel)
                {
                    continue;
                }

                bool canUnlock = weapon.IsUnlocked || activeSlots.Count < maxActiveWeapons;
                if (canUnlock)
                {
                    candidates.Add(weapon);
                }
            }

            return candidates;
        }

        private List<UpgradeType> GetPassiveCandidates()
        {
            List<UpgradeType> candidates = new List<UpgradeType>();
            foreach (UpgradeType type in passivePool)
            {
                bool alreadySlotted = passiveSlots.Contains(type);
                bool canSlotNew = passiveSlots.Count < maxPassiveSkills;
                if ((alreadySlotted || canSlotNew) && GetPassiveLevel(type) < 5)
                {
                    candidates.Add(type);
                }
            }

            return candidates;
        }

        private void AddPassiveLevel(UpgradeType type)
        {
            if (!passiveSlots.Contains(type))
            {
                if (passiveSlots.Count >= maxPassiveSkills)
                {
                    return;
                }

                passiveSlots.Add(type);
            }

            passiveLevels[type] = GetPassiveLevel(type) + 1;
        }

        private int GetPassiveLevel(UpgradeType type)
        {
            return passiveLevels.TryGetValue(type, out int level) ? level : 0;
        }

        private UpgradeOption CreateWeaponOption(WeaponBase weapon)
        {
            int nextLevel = weapon.Level + 1;
            string verb = weapon.IsUnlocked ? "升级" : "解锁";
            return new UpgradeOption
            {
                Type = UpgradeType.Weapon,
                WeaponId = weapon.Id,
                Title = verb + "：" + GetWeaponName(weapon.Id),
                Description = "等级 " + nextLevel + "。" + GetWeaponDescription(weapon.Id)
            };
        }

        private UpgradeOption CreatePassiveOption(UpgradeType type)
        {
            int nextLevel = GetPassiveLevel(type) + 1;
            return new UpgradeOption
            {
                Type = type,
                Title = GetPassiveName(type) + " 等级 " + nextLevel,
                Description = GetPassiveDescription(type)
            };
        }

        private static string FormatTime(float elapsedSeconds)
        {
            int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
            int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);
            return minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        public static string GetWeaponName(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Shotgun:
                    return "爆裂霰弹枪";
                case WeaponId.Tesla:
                    return "电磁手套";
                case WeaponId.Singularity:
                    return "重力黑洞炮";
                case WeaponId.Lightblade:
                    return "光刃影切";
                case WeaponId.Laser:
                    return "裂变激光枪";
                default:
                    return "哨兵手枪";
            }
        }

        public static string GetPassiveName(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.AmmoBox:
                    return "扩容弹药箱";
                case UpgradeType.Overclock:
                    return "超频处理器";
                case UpgradeType.Adrenaline:
                    return "肾上腺素";
                case UpgradeType.NanoArmor:
                    return "钛合金护甲";
                case UpgradeType.Propellent:
                    return "高能推进剂";
                case UpgradeType.GravityCore:
                    return "重力核心";
                case UpgradeType.Magnet:
                    return "超导磁铁";
                case UpgradeType.HazmatSuit:
                    return "防化服";
                case UpgradeType.GreedChip:
                    return "贪婪芯片";
                case UpgradeType.Radar:
                    return "战术雷达";
                case UpgradeType.Defibrillator:
                    return "紧急除颤器";
                case UpgradeType.Radio:
                    return "战术无线电";
                default:
                    return "修复";
            }
        }

        private static string GetWeaponDescription(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Shotgun:
                    return "近距离扇形爆发，附带击退。";
                case WeaponId.Tesla:
                    return "沿瞄准方向锁定敌人，电弧会连锁传导。";
                case WeaponId.Singularity:
                    return "发射缓慢黑洞球，牵引并伤害尸群。";
                case WeaponId.Lightblade:
                    return "向鼠标方向挥出前方扇形近战斩击。";
                case WeaponId.Laser:
                    return "朝瞄准方向持续发射穿透激光。";
                default:
                    return "稳定可靠的瞄准射击武器。";
            }
        }

        private static string GetPassiveDescription(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.AmmoBox:
                    return "武器伤害 +8%，范围 +5%。";
                case UpgradeType.Overclock:
                    return "攻击速度 +9%。";
                case UpgradeType.Adrenaline:
                    return "移动速度 +7%。";
                case UpgradeType.NanoArmor:
                    return "受到伤害 -6%。";
                case UpgradeType.Propellent:
                    return "弹体速度 +12%，子弹穿透 +1。";
                case UpgradeType.GravityCore:
                    return "持续性效果时长 +12%。";
                case UpgradeType.Magnet:
                    return "拾取范围 +1.25。";
                case UpgradeType.HazmatSuit:
                    return "生命上限 +18，每秒回复 +0.35。";
                case UpgradeType.GreedChip:
                    return "经验获取 +8%。";
                case UpgradeType.Radar:
                    return "暴击率 +4%，暴击伤害 +10%。";
                case UpgradeType.Defibrillator:
                    return "首次选择获得 1 次复活；之后提升升级回血。";
                case UpgradeType.Radio:
                    return "补给掉落幸运 +10%。";
                default:
                    return "恢复 35% 生命。";
            }
        }
    }
}
