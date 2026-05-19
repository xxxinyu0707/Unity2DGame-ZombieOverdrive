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
                    Title = "Emergency Repair",
                    Description = "All chosen slots are maxed. Recover 35% health."
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
            builder.AppendLine("RUN STATUS");
            builder.AppendLine("Time " + FormatTime(elapsedSeconds) + "   Level " + level + "   Kills " + kills);
            builder.AppendLine();
            builder.AppendLine("Active Weapons " + activeSlots.Count + "/" + maxActiveWeapons);
            for (int i = 0; i < maxActiveWeapons; i++)
            {
                if (i < activeSlots.Count && weapons.TryGetValue(activeSlots[i], out WeaponBase weapon))
                {
                    builder.AppendLine("- " + GetWeaponName(activeSlots[i]) + " Lv " + weapon.Level);
                }
                else
                {
                    builder.AppendLine("- [Empty]");
                }
            }

            builder.AppendLine();
            builder.AppendLine("Passive Skills " + passiveSlots.Count + "/" + maxPassiveSkills);
            for (int i = 0; i < maxPassiveSkills; i++)
            {
                if (i < passiveSlots.Count)
                {
                    UpgradeType passive = passiveSlots[i];
                    builder.AppendLine("- " + GetPassiveName(passive) + " Lv " + GetPassiveLevel(passive));
                }
                else
                {
                    builder.AppendLine("- [Empty]");
                }
            }

            builder.AppendLine();
            builder.AppendLine("ESC/Resume to continue. Restart starts a new run.");
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
            string verb = weapon.IsUnlocked ? "Upgrade" : "Unlock";
            return new UpgradeOption
            {
                Type = UpgradeType.Weapon,
                WeaponId = weapon.Id,
                Title = verb + ": " + GetWeaponName(weapon.Id),
                Description = "Lv " + nextLevel + ". " + GetWeaponDescription(weapon.Id)
            };
        }

        private UpgradeOption CreatePassiveOption(UpgradeType type)
        {
            int nextLevel = GetPassiveLevel(type) + 1;
            return new UpgradeOption
            {
                Type = type,
                Title = GetPassiveName(type) + " Lv " + nextLevel,
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
                    return "Shotgun";
                case WeaponId.Tesla:
                    return "Tesla Glove";
                case WeaponId.Singularity:
                    return "Singularity Gun";
                case WeaponId.Lightblade:
                    return "Lightblade";
                case WeaponId.Laser:
                    return "Fission Laser";
                default:
                    return "Pistol";
            }
        }

        public static string GetPassiveName(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.AmmoBox:
                    return "Ammo Box";
                case UpgradeType.Overclock:
                    return "Overclock";
                case UpgradeType.Adrenaline:
                    return "Adrenaline";
                case UpgradeType.NanoArmor:
                    return "Nano Armor";
                case UpgradeType.Propellent:
                    return "Propellent";
                case UpgradeType.GravityCore:
                    return "Gravity Core";
                case UpgradeType.Magnet:
                    return "Magnet";
                case UpgradeType.HazmatSuit:
                    return "Hazmat Suit";
                case UpgradeType.GreedChip:
                    return "Greed Chip";
                case UpgradeType.Radar:
                    return "Radar";
                case UpgradeType.Defibrillator:
                    return "Defibrillator";
                case UpgradeType.Radio:
                    return "Radio";
                default:
                    return "Repair";
            }
        }

        private static string GetWeaponDescription(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Shotgun:
                    return "Short-range cone burst with knockback.";
                case WeaponId.Tesla:
                    return "Chains lightning between nearby enemies in your aim direction.";
                case WeaponId.Singularity:
                    return "Fires a slow gravity orb that pulls and damages crowds.";
                case WeaponId.Lightblade:
                    return "Front arc melee slash aimed by the mouse.";
                case WeaponId.Laser:
                    return "Continuous piercing beam in the aim direction.";
                default:
                    return "Reliable aimed bullet weapon.";
            }
        }

        private static string GetPassiveDescription(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.AmmoBox:
                    return "Weapon damage +8%, area +5%.";
                case UpgradeType.Overclock:
                    return "Attack speed +9%.";
                case UpgradeType.Adrenaline:
                    return "Move speed +7%.";
                case UpgradeType.NanoArmor:
                    return "Incoming damage -6%.";
                case UpgradeType.Propellent:
                    return "Projectile speed +12%, bullet pierce +1.";
                case UpgradeType.GravityCore:
                    return "Duration effects last +12%.";
                case UpgradeType.Magnet:
                    return "Pickup range +1.25.";
                case UpgradeType.HazmatSuit:
                    return "Max health +18, regen +0.35/sec.";
                case UpgradeType.GreedChip:
                    return "XP gain +8%.";
                case UpgradeType.Radar:
                    return "Crit chance +4%, crit damage +10%.";
                case UpgradeType.Defibrillator:
                    return "First pick grants 1 revive; later picks improve level-up healing.";
                case UpgradeType.Radio:
                    return "Supply drop luck +10%.";
                default:
                    return "Recover 35% health.";
            }
        }
    }
}
