using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ZombieOverdrive.Combat;

namespace ZombieOverdrive.Core
{
    public enum UpgradeType
    {
        Weapon,
        Evolution,
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
        Repair,
        GoldBag
    }

    public struct UpgradeOption
    {
        public UpgradeType Type;
        public WeaponId WeaponId;
        public string Title;
        public string Description;
        public bool Highlight;
        public string Hint;
        public string IconId;
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
        private readonly HashSet<UpgradeType> fusedPassives = new HashSet<UpgradeType>();
        private readonly HashSet<WeaponId> evolvedWeapons = new HashSet<WeaponId>();

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
            fusedPassives.Clear();
            evolvedWeapons.Clear();

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
            List<WeaponBase> evolutionCandidates = GetEvolutionCandidates();
            List<WeaponBase> weaponCandidates = GetWeaponCandidates();
            List<UpgradeType> passiveCandidates = GetPassiveCandidates();

            while (evolutionCandidates.Count > 0 && options.Count < optionsPerLevel)
            {
                options.Add(TakeEvolutionOption(evolutionCandidates));
            }

            for (int i = 0; i < optionsPerLevel; i++)
            {
                if (options.Count >= optionsPerLevel)
                {
                    break;
                }

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

            AddConsolationOptions(options);

            return options;
        }

        public void Apply(UpgradeOption option)
        {
            switch (option.Type)
            {
                case UpgradeType.Weapon:
                    ApplyWeapon(option.WeaponId);
                    break;
                case UpgradeType.Evolution:
                    EvolveWeapon(option.WeaponId);
                    break;
                case UpgradeType.AmmoBox:
                    AddPassiveLevel(option.Type);
                    stats.AddDamage(0.07f);
                    stats.AddArea(0.04f);
                    break;
                case UpgradeType.Overclock:
                    AddPassiveLevel(option.Type);
                    stats.AddFireRate(0.08f);
                    break;
                case UpgradeType.Adrenaline:
                    AddPassiveLevel(option.Type);
                    stats.AddMoveSpeed(0.06f);
                    break;
                case UpgradeType.NanoArmor:
                    AddPassiveLevel(option.Type);
                    stats.AddDamageReduction(0.05f);
                    break;
                case UpgradeType.Propellent:
                    AddPassiveLevel(option.Type);
                    stats.AddProjectileSpeed(0.1f);
                    int propellentLevel = GetPassiveLevel(option.Type);
                    if (propellentLevel == 1 || propellentLevel == 3 || propellentLevel == 5)
                    {
                        stats.bulletPierceBonus++;
                    }
                    break;
                case UpgradeType.GravityCore:
                    AddPassiveLevel(option.Type);
                    stats.AddDuration(0.1f);
                    break;
                case UpgradeType.Magnet:
                    AddPassiveLevel(option.Type);
                    stats.AddMagnetRange(0.95f);
                    break;
                case UpgradeType.HazmatSuit:
                    AddPassiveLevel(option.Type);
                    stats.AddMaxHealth(16f);
                    stats.AddHealthRegen(0.28f);
                    if (health != null) health.Heal(health.MaxHealth * 0.15f);
                    break;
                case UpgradeType.GreedChip:
                    AddPassiveLevel(option.Type);
                    stats.AddXpGain(0.06f);
                    stats.AddGoldGain(0.12f);
                    break;
                case UpgradeType.Radar:
                    AddPassiveLevel(option.Type);
                    stats.AddCritical(0.035f, 0.08f);
                    break;
                case UpgradeType.Defibrillator:
                    AddPassiveLevel(option.Type);
                    if (GetPassiveLevel(option.Type) == 1)
                    {
                        stats.reviveCharges++;
                    }

                    stats.levelUpHealPercent += 0.012f;
                    break;
                case UpgradeType.Radio:
                    AddPassiveLevel(option.Type);
                    stats.AddPickupLuck(0.08f);
                    break;
                case UpgradeType.Repair:
                    if (health != null) health.Heal(health.MaxHealth * 0.3f);
                    break;
                case UpgradeType.GoldBag:
                    if (GameManager.Instance != null) GameManager.Instance.AddRunGold(85);
                    break;
            }
        }

        public string BuildStatusText(int level, int kills, float elapsedSeconds)
        {
            StringBuilder builder = new StringBuilder(512);
            builder.AppendLine("时间 " + FormatTime(elapsedSeconds) + "   等级 " + level + "   击杀 " + kills);
            builder.AppendLine();
            builder.AppendLine("超进化搭配");
            foreach (WeaponId id in GetAllWeaponIds())
            {
                builder.AppendLine(FormatEvolutionLine(id));
            }

            builder.AppendLine();
            builder.AppendLine("ESC 或继续：返回游戏");
            builder.AppendLine("重开：结算当前金币并开启新一局");
            return builder.ToString();
        }

        private string FormatEvolutionLine(WeaponId id)
        {
            string state;
            if (evolvedWeapons.Contains(id))
            {
                state = "已完成";
            }
            else if (IsWeaponUnlocked(id) && GetWeaponLevel(id) >= 5 && GetPassiveLevel(GetEvolutionPassive(id)) > 0)
            {
                state = "可进化";
            }
            else if (IsWeaponUnlocked(id) || GetPassiveLevel(GetEvolutionPassive(id)) > 0)
            {
                state = "准备中";
            }
            else
            {
                state = "未获得";
            }

            return "- " + GetWeaponName(id) + " + " + GetPassiveName(GetEvolutionPassive(id)) + "：" + state;
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

        private void EvolveWeapon(WeaponId id)
        {
            if (!weapons.TryGetValue(id, out WeaponBase weapon) || !CanEvolve(id, weapon))
            {
                return;
            }

            evolvedWeapons.Add(id);
            UpgradeType passive = GetEvolutionPassive(id);
            if (passiveSlots.Remove(passive))
            {
                fusedPassives.Add(passive);
            }

            weapon.Evolve();
        }

        private void AddConsolationOptions(List<UpgradeOption> options)
        {
            if (options.Count >= optionsPerLevel)
            {
                return;
            }

            if (!ContainsOption(options, UpgradeType.Repair))
            {
                options.Add(new UpgradeOption
                {
                    Type = UpgradeType.Repair,
                    Title = "战地烧鸡",
                    Description = "恢复 35% 最大生命值。适合在构筑接近成型时稳住局面。",
                    Hint = "保底补给",
                    IconId = "passive_repair"
                });
            }

            if (options.Count >= optionsPerLevel)
            {
                return;
            }

            if (!ContainsOption(options, UpgradeType.GoldBag))
            {
                options.Add(new UpgradeOption
                {
                    Type = UpgradeType.GoldBag,
                    Title = "补给钱包",
                    Description = "立即获得 85 局内金币，结算时会转化为局外金币。",
                    Hint = "保底收益",
                    IconId = "passive_goldbag"
                });
            }
        }

        private static bool ContainsOption(List<UpgradeOption> options, UpgradeType type)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Type == type)
                {
                    return true;
                }
            }

            return false;
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

        private UpgradeOption TakeEvolutionOption(List<WeaponBase> candidates)
        {
            int index = Random.Range(0, candidates.Count);
            WeaponBase weapon = candidates[index];
            candidates.RemoveAt(index);
            return CreateEvolutionOption(weapon);
        }

        private List<WeaponBase> GetEvolutionCandidates()
        {
            List<WeaponBase> candidates = new List<WeaponBase>();
            foreach (WeaponBase weapon in weapons.Values)
            {
                if (weapon == null || !CanEvolve(weapon.Id, weapon))
                {
                    continue;
                }

                candidates.Add(weapon);
            }

            return candidates;
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
                if (!fusedPassives.Contains(type) && (alreadySlotted || canSlotNew) && GetPassiveLevel(type) < 5)
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

        private bool CanEvolve(WeaponId id, WeaponBase weapon)
        {
            return weapon != null
                && weapon.Level >= 5
                && GetPassiveLevel(GetEvolutionPassive(id)) > 0
                && !evolvedWeapons.Contains(id);
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
                Description = GetWeaponDescription(weapon.Id, nextLevel),
                Highlight = HasMatchingEvolutionPassive(weapon.Id),
                Hint = GetEvolutionHint(weapon.Id),
                IconId = GetWeaponIconId(weapon.Id)
            };
        }

        private UpgradeOption CreateEvolutionOption(WeaponBase weapon)
        {
            return new UpgradeOption
            {
                Type = UpgradeType.Evolution,
                WeaponId = weapon.Id,
                Title = "超进化：" + GetEvolutionName(weapon.Id),
                Description = GetWeaponName(weapon.Id) + " 已满级，并拥有 " + GetPassiveName(GetEvolutionPassive(weapon.Id)) + "。选择后获得强化形态。",
                Highlight = true,
                Hint = "觉醒组合已完成",
                IconId = GetEvolutionIconId(weapon.Id)
            };
        }

        private UpgradeOption CreatePassiveOption(UpgradeType type)
        {
            int nextLevel = GetPassiveLevel(type) + 1;
            return new UpgradeOption
            {
                Type = type,
                Title = GetPassiveName(type) + " 等级 " + nextLevel,
                Description = GetPassiveDescription(type),
                Highlight = SupportsOwnedWeapon(type),
                Hint = GetPassivePairHint(type),
                IconId = GetPassiveIconId(type)
            };
        }

        public bool IsWeaponEvolved(WeaponId id)
        {
            return evolvedWeapons.Contains(id);
        }

        public bool IsWeaponUnlocked(WeaponId id)
        {
            return activeSlots.Contains(id);
        }

        public int GetWeaponLevel(WeaponId id)
        {
            return weapons.TryGetValue(id, out WeaponBase weapon) ? weapon.Level : 0;
        }

        public IReadOnlyList<WeaponId> ActiveSlots => activeSlots;

        public IReadOnlyList<UpgradeType> PassiveSlots => passiveSlots;

        public int MaxActiveWeapons => maxActiveWeapons;

        public int MaxPassiveSkills => maxPassiveSkills;

        public int GetPassiveSkillLevel(UpgradeType type)
        {
            return GetPassiveLevel(type);
        }

        public string GetEvolutionReadyText(WeaponId id)
        {
            bool weaponReady = GetWeaponLevel(id) >= 5;
            bool passiveReady = GetPassiveLevel(GetEvolutionPassive(id)) > 0;
            if (evolvedWeapons.Contains(id))
            {
                return "已完成，搭配被动已融合";
            }

            if (weaponReady && passiveReady)
            {
                return "可超进化";
            }

            return "需要武器 5 级 + 对应被动";
        }

        private bool HasMatchingEvolutionPassive(WeaponId id)
        {
            return GetPassiveLevel(GetEvolutionPassive(id)) > 0 && !evolvedWeapons.Contains(id);
        }

        private bool SupportsOwnedWeapon(UpgradeType type)
        {
            foreach (WeaponId weaponId in activeSlots)
            {
                if (GetEvolutionPassive(weaponId) == type && !evolvedWeapons.Contains(weaponId))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetEvolutionHint(WeaponId id)
        {
            UpgradeType passive = GetEvolutionPassive(id);
            string hint = "超进化搭配：" + GetPassiveName(passive);
            if (GetPassiveLevel(passive) > 0 && GetWeaponLevel(id) < 5)
            {
                hint += " 已拥有，武器升到 5 级即可觉醒";
            }
            else if (GetPassiveLevel(passive) == 0)
            {
                hint += " 未拥有";
            }

            return hint;
        }

        private string GetPassivePairHint(UpgradeType type)
        {
            WeaponId weaponId = GetEvolutionWeapon(type);
            if (weaponId == WeaponId.Pistol && type != UpgradeType.Propellent)
            {
                return "通用被动";
            }

            return "可搭配：" + GetWeaponName(weaponId);
        }

        public static UpgradeType GetEvolutionPassive(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Shotgun:
                    return UpgradeType.AmmoBox;
                case WeaponId.Tesla:
                    return UpgradeType.Overclock;
                case WeaponId.Singularity:
                    return UpgradeType.GravityCore;
                case WeaponId.Lightblade:
                    return UpgradeType.Adrenaline;
                case WeaponId.Laser:
                    return UpgradeType.NanoArmor;
                default:
                    return UpgradeType.Propellent;
            }
        }

        public static WeaponId GetEvolutionWeapon(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.AmmoBox:
                    return WeaponId.Shotgun;
                case UpgradeType.Overclock:
                    return WeaponId.Tesla;
                case UpgradeType.GravityCore:
                    return WeaponId.Singularity;
                case UpgradeType.Adrenaline:
                    return WeaponId.Lightblade;
                case UpgradeType.NanoArmor:
                    return WeaponId.Laser;
                default:
                    return WeaponId.Pistol;
            }
        }

        public static string GetWeaponIconId(WeaponId id)
        {
            return "weapon_" + id.ToString().ToLowerInvariant();
        }

        public static string GetEvolutionIconId(WeaponId id)
        {
            return "evolution_" + id.ToString().ToLowerInvariant();
        }

        public static string GetPassiveIconId(UpgradeType type)
        {
            return "passive_" + type.ToString().ToLowerInvariant();
        }

        public static IEnumerable<WeaponId> GetAllWeaponIds()
        {
            yield return WeaponId.Pistol;
            yield return WeaponId.Shotgun;
            yield return WeaponId.Tesla;
            yield return WeaponId.Singularity;
            yield return WeaponId.Lightblade;
            yield return WeaponId.Laser;
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

        public static string GetEvolutionName(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Shotgun:
                    return "钢雨撕裂者";
                case WeaponId.Tesla:
                    return "雷暴线圈";
                case WeaponId.Singularity:
                    return "暗物质坍缩";
                case WeaponId.Lightblade:
                    return "修罗剑阵";
                case WeaponId.Laser:
                    return "日冕切割束";
                default:
                    return "双持穿甲哨兵";
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
                case UpgradeType.GoldBag:
                    return "补给钱包";
            }
        }

        private static string GetWeaponDescription(WeaponId id, int level)
        {
            switch (id)
            {
                case WeaponId.Shotgun:
                    switch (level)
                    {
                        case 1: return "等级 1：5 发扇形弹丸，近距离击退。";
                        case 2: return "等级 2：弹丸增至 7 发，散布更集中。";
                        case 3: return "等级 3：弹丸命中后留下短暂燃烧区域。";
                        case 4: return "等级 4：装填更快，击退更强。";
                        default: return "等级 5：弹丸获得穿透，能打穿前排。";
                    }
                case WeaponId.Tesla:
                    switch (level)
                    {
                        case 1: return "等级 1：链式电弧最多命中 3 个目标。";
                        case 2: return "等级 2：链上限提升，首个目标承受更高伤害。";
                        case 3: return "等级 3：电弧有概率短暂眩晕敌人。";
                        case 4: return "等级 4：传导距离提升，并可能分叉。";
                        default: return "等级 5：击杀目标会产生电磁爆链。";
                    }
                case WeaponId.Singularity:
                    switch (level)
                    {
                        case 1: return "等级 1：发射牵引黑洞球。";
                        case 2: return "等级 2：牵引半径扩大。";
                        case 3: return "等级 3：黑洞附加最大生命百分比撕裂。";
                        case 4: return "等级 4：黑洞会吞噬敌方酸弹。";
                        default: return "等级 5：同时发射双重黑洞。";
                    }
                case WeaponId.Lightblade:
                    switch (level)
                    {
                        case 1: return "等级 1：前方 120 度光刃斩击。";
                        case 2: return "等级 2：斩击半径提升，并射出剑气。";
                        case 3: return "等级 3：挥刀更快，挥刀瞬间有短暂无敌。";
                        case 4: return "等级 4：对残血敌人造成斩杀伤害，斩 Boss 可回血。";
                        default: return "等级 5：斩击角度扩大到大半圈。";
                    }
                case WeaponId.Laser:
                    switch (level)
                    {
                        case 1: return "等级 1：持续穿透激光。";
                        case 2: return "等级 2：激光更粗，伤害提升。";
                        case 3: return "等级 3：命中敌人后会折射到附近目标。";
                        case 4: return "等级 4：持续照射同一目标会热能过载。";
                        default: return "等级 5：折射次数提升，形成乱射光网。";
                    }
                default:
                    switch (level)
                    {
                        case 1: return "等级 1：稳定单发瞄准射击。";
                        case 2: return "等级 2：射速和伤害提升。";
                        case 3: return "等级 3：命中首个目标后分裂出两发小弹。";
                        case 4: return "等级 4：变为双枪齐射。";
                        default: return "等级 5：穿透提升，并对异常状态敌人造成双倍伤害。";
                    }
            }
        }

        private static string GetPassiveDescription(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.AmmoBox:
                    return "武器伤害 +7%，范围 +4%。";
                case UpgradeType.Overclock:
                    return "攻击速度 +8%。";
                case UpgradeType.Adrenaline:
                    return "移动速度 +6%。";
                case UpgradeType.NanoArmor:
                    return "受到伤害 -5%。";
                case UpgradeType.Propellent:
                    return "弹体速度 +10%，等级 1/3/5 增加子弹穿透。";
                case UpgradeType.GravityCore:
                    return "持续性效果时长 +10%。";
                case UpgradeType.Magnet:
                    return "拾取范围 +0.95。";
                case UpgradeType.HazmatSuit:
                    return "生命上限 +16，每秒回复 +0.28。";
                case UpgradeType.GreedChip:
                    return "经验获取 +6%，金币收益 +12%。";
                case UpgradeType.Radar:
                    return "暴击率 +3.5%，暴击伤害 +8%。";
                case UpgradeType.Defibrillator:
                    return "首次选择获得 1 次复活；之后提升升级回血。";
                case UpgradeType.Radio:
                    return "补给掉落幸运 +8%。";
                case UpgradeType.GoldBag:
                    return "立即获得 85 金币。";
                default:
                    return "恢复 30% 生命。";
            }
        }
    }
}
