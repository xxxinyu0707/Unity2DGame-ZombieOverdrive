using System.Collections.Generic;
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
        Radio
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
        private PlayerStats stats;
        private PlayerHealth health;

        public void Initialize(PlayerStats playerStats, IEnumerable<WeaponBase> weaponComponents, PlayerHealth playerHealth)
        {
            stats = playerStats;
            health = playerHealth;

            weapons.Clear();
            foreach (WeaponBase weapon in weaponComponents)
            {
                if (weapon == null)
                {
                    continue;
                }

                weapons[weapon.Id] = weapon;
            }

            if (weapons.TryGetValue(WeaponId.Pistol, out WeaponBase pistol) && !pistol.IsUnlocked)
            {
                pistol.UnlockOrLevel();
            }
        }

        public List<UpgradeOption> RollOptions()
        {
            List<UpgradeOption> options = new List<UpgradeOption>();
            List<WeaponBase> weaponCandidates = GetWeaponCandidates();
            List<UpgradeType> passiveCandidates = GetPassiveCandidates();

            for (int i = 0; i < optionsPerLevel; i++)
            {
                bool preferWeapon = weaponCandidates.Count > 0 && (i == 0 || Random.value < 0.48f);
                if (preferWeapon)
                {
                    int index = Random.Range(0, weaponCandidates.Count);
                    WeaponBase weapon = weaponCandidates[index];
                    weaponCandidates.RemoveAt(index);
                    options.Add(CreateWeaponOption(weapon));
                    continue;
                }

                if (passiveCandidates.Count > 0)
                {
                    int index = Random.Range(0, passiveCandidates.Count);
                    UpgradeType type = passiveCandidates[index];
                    passiveCandidates.RemoveAt(index);
                    options.Add(CreatePassiveOption(type));
                    continue;
                }

                if (weaponCandidates.Count > 0)
                {
                    int index = Random.Range(0, weaponCandidates.Count);
                    WeaponBase weapon = weaponCandidates[index];
                    weaponCandidates.RemoveAt(index);
                    options.Add(CreateWeaponOption(weapon));
                }
            }

            return options;
        }

        public void Apply(UpgradeOption option)
        {
            switch (option.Type)
            {
                case UpgradeType.Weapon:
                    if (weapons.TryGetValue(option.WeaponId, out WeaponBase weapon))
                    {
                        weapon.UnlockOrLevel();
                    }
                    break;
                case UpgradeType.AmmoBox:
                    AddPassiveLevel(option.Type);
                    stats.AddDamage(0.1f);
                    stats.AddArea(0.05f);
                    break;
                case UpgradeType.Overclock:
                    AddPassiveLevel(option.Type);
                    stats.AddFireRate(0.1f);
                    break;
                case UpgradeType.Adrenaline:
                    AddPassiveLevel(option.Type);
                    stats.AddMoveSpeed(0.08f);
                    break;
                case UpgradeType.NanoArmor:
                    AddPassiveLevel(option.Type);
                    stats.AddDamageReduction(0.08f);
                    break;
                case UpgradeType.Propellent:
                    AddPassiveLevel(option.Type);
                    stats.AddProjectileSpeed(0.15f);
                    stats.bulletPierceBonus++;
                    break;
                case UpgradeType.GravityCore:
                    AddPassiveLevel(option.Type);
                    stats.AddDuration(0.15f);
                    break;
                case UpgradeType.Magnet:
                    AddPassiveLevel(option.Type);
                    stats.AddMagnetRange(1.5f);
                    break;
                case UpgradeType.HazmatSuit:
                    AddPassiveLevel(option.Type);
                    stats.AddMaxHealth(24f);
                    stats.AddHealthRegen(0.5f);
                    if (health != null) health.Heal(30f);
                    break;
                case UpgradeType.GreedChip:
                    AddPassiveLevel(option.Type);
                    stats.AddXpGain(0.1f);
                    break;
                case UpgradeType.Radar:
                    AddPassiveLevel(option.Type);
                    stats.AddCritical(0.05f, 0.12f);
                    break;
                case UpgradeType.Defibrillator:
                    AddPassiveLevel(option.Type);
                    if (GetPassiveLevel(option.Type) == 1)
                    {
                        stats.reviveCharges++;
                    }
                    stats.levelUpHealPercent += 0.02f;
                    break;
                case UpgradeType.Radio:
                    AddPassiveLevel(option.Type);
                    stats.AddPickupLuck(0.12f);
                    break;
            }

            if (option.Type == UpgradeType.HazmatSuit && health != null)
            {
                health.Heal(health.MaxHealth * 0.1f);
            }
        }

        private List<WeaponBase> GetWeaponCandidates()
        {
            List<WeaponBase> candidates = new List<WeaponBase>();
            foreach (WeaponBase weapon in weapons.Values)
            {
                if (weapon != null && weapon.CanLevel)
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
                if (GetPassiveLevel(type) < 5)
                {
                    candidates.Add(type);
                }
            }

            return candidates;
        }

        private void AddPassiveLevel(UpgradeType type)
        {
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
                Description = GetWeaponDescription(weapon.Id, nextLevel)
            };
        }

        private UpgradeOption CreatePassiveOption(UpgradeType type)
        {
            int nextLevel = GetPassiveLevel(type) + 1;
            switch (type)
            {
                case UpgradeType.AmmoBox:
                    return Passive(type, "弹药集束箱 Lv " + nextLevel, "所有武器伤害 +10%，范围 +5%。");
                case UpgradeType.Overclock:
                    return Passive(type, "超频处理器 Lv " + nextLevel, "所有武器攻击频率 +10%。");
                case UpgradeType.Adrenaline:
                    return Passive(type, "肾上腺素 Lv " + nextLevel, "移动速度 +8%。");
                case UpgradeType.NanoArmor:
                    return Passive(type, "重型纳米甲 Lv " + nextLevel, "受到的伤害降低 8%。");
                case UpgradeType.Propellent:
                    return Passive(type, "高能燃料罐 Lv " + nextLevel, "子弹速度 +15%，远程穿透 +1。");
                case UpgradeType.GravityCore:
                    return Passive(type, "重力稳定器 Lv " + nextLevel, "黑洞等持续类效果时间 +15%。");
                case UpgradeType.Magnet:
                    return Passive(type, "超导电磁链 Lv " + nextLevel, "经验水晶吸取范围 +1.5。");
                case UpgradeType.HazmatSuit:
                    return Passive(type, "生物防护服 Lv " + nextLevel, "最大生命 +24，生命恢复 +0.5/秒。");
                case UpgradeType.GreedChip:
                    return Passive(type, "贪婪芯片 Lv " + nextLevel, "经验获取 +10%，更快进入成型节奏。");
                case UpgradeType.Radar:
                    return Passive(type, "高频雷达 Lv " + nextLevel, "暴击率 +5%，暴击伤害提高。");
                case UpgradeType.Defibrillator:
                    return Passive(type, "紧急除颤器 Lv " + nextLevel, "首次获得 1 次复活，后续提高升级回血。");
                case UpgradeType.Radio:
                    return Passive(type, "战术无线电 Lv " + nextLevel, "补给掉落运气 +12%。");
                default:
                    return Passive(type, "补给", "获得一项强化。");
            }
        }

        private static UpgradeOption Passive(UpgradeType type, string title, string description)
        {
            return new UpgradeOption { Type = type, Title = title, Description = description };
        }

        private static string GetWeaponName(WeaponId id)
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
                    return "光刃·影切";
                case WeaponId.Laser:
                    return "裂变激光枪";
                default:
                    return "哨兵手枪";
            }
        }

        private static string GetWeaponDescription(WeaponId id, int level)
        {
            switch (id)
            {
                case WeaponId.Shotgun:
                    return "Lv " + level + "：扇形发射多颗弹丸，近距离爆发和击退强。";
                case WeaponId.Tesla:
                    return "Lv " + level + "：自动连锁附近敌人，并附带减速。";
                case WeaponId.Singularity:
                    return "Lv " + level + "：发射缓慢黑洞，牵引并持续伤害尸群。";
                case WeaponId.Lightblade:
                    return "Lv " + level + "：朝鼠标方向进行扇形近战斩击。";
                case WeaponId.Laser:
                    return "Lv " + level + "：持续贯穿激光，适合处理排成线的尸潮。";
                default:
                    return "Lv " + level + "：提升手枪射速、伤害和弹幕密度。";
            }
        }
    }
}
