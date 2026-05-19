using System.Collections.Generic;
using UnityEngine;
using ZombieOverdrive.Combat;

namespace ZombieOverdrive.Core
{
    public enum UpgradeType
    {
        PistolDamage,
        PistolFireRate,
        BulletPierce,
        MoveSpeed,
        MaxHealth,
        MagnetRange
    }

    public struct UpgradeOption
    {
        public UpgradeType Type;
        public string Title;
        public string Description;
    }

    public class UpgradeSystem : MonoBehaviour
    {
        [SerializeField] private int optionsPerLevel = 3;

        private readonly List<UpgradeType> pool = new List<UpgradeType>
        {
            UpgradeType.PistolDamage,
            UpgradeType.PistolFireRate,
            UpgradeType.BulletPierce,
            UpgradeType.MoveSpeed,
            UpgradeType.MaxHealth,
            UpgradeType.MagnetRange
        };

        private PlayerStats stats;
        private PistolWeapon pistol;
        private PlayerHealth health;

        public void Initialize(PlayerStats playerStats, PistolWeapon pistolWeapon, PlayerHealth playerHealth)
        {
            stats = playerStats;
            pistol = pistolWeapon;
            health = playerHealth;
        }

        public List<UpgradeOption> RollOptions()
        {
            List<UpgradeType> candidates = new List<UpgradeType>(pool);
            List<UpgradeOption> options = new List<UpgradeOption>();

            for (int i = 0; i < optionsPerLevel && candidates.Count > 0; i++)
            {
                int index = Random.Range(0, candidates.Count);
                UpgradeType type = candidates[index];
                candidates.RemoveAt(index);
                options.Add(CreateOption(type));
            }

            return options;
        }

        public void Apply(UpgradeOption option)
        {
            switch (option.Type)
            {
                case UpgradeType.PistolDamage:
                    stats.AddDamage(0.2f);
                    if (pistol != null) pistol.AddLevel();
                    break;
                case UpgradeType.PistolFireRate:
                    stats.AddFireRate(0.18f);
                    if (pistol != null) pistol.AddLevel();
                    break;
                case UpgradeType.BulletPierce:
                    stats.bulletPierceBonus++;
                    break;
                case UpgradeType.MoveSpeed:
                    stats.AddMoveSpeed(0.1f);
                    break;
                case UpgradeType.MaxHealth:
                    stats.AddMaxHealth(20f);
                    if (health != null) health.Heal(20f);
                    break;
                case UpgradeType.MagnetRange:
                    stats.AddMagnetRange(1.25f);
                    break;
            }
        }

        private UpgradeOption CreateOption(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.PistolDamage:
                    return new UpgradeOption { Type = type, Title = "哨兵手枪：增伤", Description = "武器伤害 +20%，手枪等级 +1" };
                case UpgradeType.PistolFireRate:
                    return new UpgradeOption { Type = type, Title = "哨兵手枪：速射", Description = "攻击频率 +18%，手枪等级 +1" };
                case UpgradeType.BulletPierce:
                    return new UpgradeOption { Type = type, Title = "高能燃料罐", Description = "子弹穿透次数 +1" };
                case UpgradeType.MoveSpeed:
                    return new UpgradeOption { Type = type, Title = "肾上腺素", Description = "移动速度 +10%" };
                case UpgradeType.MaxHealth:
                    return new UpgradeOption { Type = type, Title = "生物防护服", Description = "最大生命 +20，并立即治疗" };
                case UpgradeType.MagnetRange:
                    return new UpgradeOption { Type = type, Title = "超导电磁链", Description = "经验水晶拾取范围 +1.25" };
                default:
                    return new UpgradeOption { Type = type, Title = "补给", Description = "获得一项强化" };
            }
        }
    }
}
