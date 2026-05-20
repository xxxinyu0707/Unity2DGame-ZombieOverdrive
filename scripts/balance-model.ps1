param(
    [int]$Runs = 800,
    [int]$Minutes = 10,
    [int]$Seed = 5000,
    [ValidateSet("Current", "Baseline", "Both")]
    [string]$Curve = "Both"
)

$ErrorActionPreference = "Stop"

# Closed-loop Monte Carlo balance model.
# Each simulated second:
# 1. Estimate current build DPS from weapons/passives/evolutions.
# 2. Compare DPS with current wave enemy HP to estimate kill rate.
# 3. Convert kills into XP and level-ups.
# 4. Roll upgrade choices using the same 3-option, 3-active, 3-passive, 3-reroll rules.
# 5. Recompute DPS from the updated build on the next second.

$source = @"
using System;
using System.Collections.Generic;
using System.Linq;

public static class BalanceModel
{
    private static readonly string[] Weapons = { "Pistol", "Shotgun", "Tesla", "Singularity", "Lightblade", "Laser" };
    private static readonly string[] Passives = { "AmmoBox", "Overclock", "Adrenaline", "NanoArmor", "Propellent", "GravityCore", "Magnet", "HazmatSuit", "GreedChip", "Radar", "Defibrillator", "Radio" };
    private static readonly Dictionary<string, string> Pair = CreatePairs();

    private static readonly Dictionary<string, string> ReversePair = Pair.ToDictionary(kv => kv.Value, kv => kv.Key);

    private struct EnemyStats
    {
        public double Hp;
        public double Xp;
        public double Damage;
        public EnemyStats(double hp, double xp, double damage)
        {
            Hp = hp;
            Xp = xp;
            Damage = damage;
        }
    }

    private static readonly Dictionary<string, EnemyStats> Enemies = CreateEnemies();

    private static Dictionary<string, string> CreatePairs()
    {
        Dictionary<string, string> pairs = new Dictionary<string, string>();
        pairs["Pistol"] = "Propellent";
        pairs["Shotgun"] = "AmmoBox";
        pairs["Tesla"] = "Overclock";
        pairs["Singularity"] = "GravityCore";
        pairs["Lightblade"] = "Adrenaline";
        pairs["Laser"] = "NanoArmor";
        return pairs;
    }

    private static Dictionary<string, EnemyStats> CreateEnemies()
    {
        Dictionary<string, EnemyStats> enemies = new Dictionary<string, EnemyStats>();
        enemies["Walker"] = new EnemyStats(46, 1, 9);
        enemies["Runner"] = new EnemyStats(40, 4, 10);
        enemies["Spitter"] = new EnemyStats(82, 5, 12);
        enemies["Tanker"] = new EnemyStats(500, 18, 24);
        return enemies;
    }

    private sealed class BuildState
    {
        public readonly Dictionary<string, int> Weapons = new Dictionary<string, int>();
        public readonly List<string> ActiveSlots = new List<string> { "Pistol" };
        public readonly Dictionary<string, int> Passives = new Dictionary<string, int>();
        public readonly List<string> PassiveSlots = new List<string>();
        public readonly HashSet<string> FusedPassives = new HashSet<string>();
        public readonly HashSet<string> Evolved = new HashSet<string>();
        public int RerollsRemaining = 3;

        public BuildState()
        {
            Weapons["Pistol"] = 1;
        }

        public int WeaponLevel(string id)
        {
            int level;
            return Weapons.TryGetValue(id, out level) ? level : 0;
        }

        public int PassiveLevel(string id)
        {
            int level;
            return Passives.TryGetValue(id, out level) ? level : 0;
        }

        public bool CanEvolve(string weapon)
        {
            return WeaponLevel(weapon) >= 5 && PassiveLevel(Pair[weapon]) >= 5 && !Evolved.Contains(weapon);
        }

        public List<Tuple<string, string>> RollOptions(Random random)
        {
            List<Tuple<string, string>> options = new List<Tuple<string, string>>();
            List<string> evolutions = Weapons.Keys.Where(CanEvolve).OrderBy(_ => random.Next()).ToList();
            foreach (string weapon in evolutions)
            {
                if (options.Count >= 3) break;
                options.Add(Tuple.Create("evolve", weapon));
            }

            List<string> weaponCandidates = BalanceModel.Weapons
                .Where(w => WeaponLevel(w) < 5 && (ActiveSlots.Contains(w) || ActiveSlots.Count < 3))
                .ToList();
            List<string> passiveCandidates = BalanceModel.Passives
                .Where(p => !FusedPassives.Contains(p) && (PassiveSlots.Contains(p) || PassiveSlots.Count < 3) && PassiveLevel(p) < 5)
                .ToList();

            for (int i = 0; i < 3 && options.Count < 3; i++)
            {
                bool chooseWeapon = weaponCandidates.Count > 0 && (i == 0 || random.NextDouble() < 0.45);
                if (chooseWeapon)
                {
                    int index = random.Next(weaponCandidates.Count);
                    options.Add(Tuple.Create("weapon", weaponCandidates[index]));
                    weaponCandidates.RemoveAt(index);
                }
                else if (passiveCandidates.Count > 0)
                {
                    int index = random.Next(passiveCandidates.Count);
                    options.Add(Tuple.Create("passive", passiveCandidates[index]));
                    passiveCandidates.RemoveAt(index);
                }
                else if (weaponCandidates.Count > 0)
                {
                    int index = random.Next(weaponCandidates.Count);
                    options.Add(Tuple.Create("weapon", weaponCandidates[index]));
                    weaponCandidates.RemoveAt(index);
                }
            }

            while (options.Count < 3)
            {
                options.Add(Tuple.Create(options.Any(o => o.Item2 == "Repair") ? "gold" : "repair", options.Any(o => o.Item2 == "Repair") ? "GoldBag" : "Repair"));
            }

            return options;
        }

        public Tuple<string, string> ChooseUpgrade(Random random, int level)
        {
            Tuple<string, string> best = null;
            for (int attempt = 0; attempt < 4; attempt++)
            {
                var scored = RollOptions(random)
                    .Select(option => Tuple.Create(Score(option, random, level), option))
                    .OrderByDescending(item => item.Item1)
                    .ToList();

                if (scored[0].Item1 >= 42 || RerollsRemaining <= 0 || attempt == 3)
                {
                    best = scored[0].Item2;
                    break;
                }

                RerollsRemaining--;
            }

            Apply(best);
            return best;
        }

        private double Score(Tuple<string, string> option, Random random, int level)
        {
            string kind = option.Item1;
            string id = option.Item2;
            if (kind == "evolve") return 125;
            if (kind == "weapon")
            {
                double score = ActiveSlots.Contains(id) ? 48 + WeaponLevel(id) * 8 : 36;
                if (!ActiveSlots.Contains(id) && ActiveSlots.Count < 2 && level > 5) score += 10;
                if (PassiveSlots.Contains(Pair[id])) score += 14 + PassiveLevel(Pair[id]) * 4;
                if (id == "Pistol") score += 6;
                if (id == "Tesla") score += 5;
                if (id == "Laser") score += 3;
                return score + Noise(random, 8);
            }

            if (kind == "passive")
            {
                double score = 34 + PassiveLevel(id) * 8;
                string weapon;
                string pairedWeapon = ReversePair.TryGetValue(id, out weapon) ? weapon : "";
                if (ActiveSlots.Contains(pairedWeapon)) score += 24 + WeaponLevel(pairedWeapon) * 4;
                else if (PassiveSlots.Count < 3) score += 4;
                if (id == "Overclock" || id == "AmmoBox" || id == "Propellent") score += 6;
                return score + Noise(random, 8);
            }

            return 8 + Noise(random, 4);
        }

        private static double Noise(Random random, double sigma)
        {
            double u1 = Math.Max(0.0001, random.NextDouble());
            double u2 = random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2) * sigma;
        }

        private void Apply(Tuple<string, string> option)
        {
            string kind = option.Item1;
            string id = option.Item2;
            if (kind == "weapon")
            {
                if (!ActiveSlots.Contains(id))
                {
                    if (ActiveSlots.Count >= 3) return;
                    ActiveSlots.Add(id);
                }

                Weapons[id] = Math.Min(5, WeaponLevel(id) + 1);
            }
            else if (kind == "passive")
            {
                if (!PassiveSlots.Contains(id))
                {
                    if (PassiveSlots.Count >= 3) return;
                    PassiveSlots.Add(id);
                }

                Passives[id] = Math.Min(5, PassiveLevel(id) + 1);
            }
            else if (kind == "evolve" && CanEvolve(id))
            {
                Evolved.Add(id);
                string passive = Pair[id];
                if (PassiveSlots.Remove(passive)) FusedPassives.Add(passive);
            }
        }
    }

    private sealed class RunResult
    {
        public int FinalLevel;
        public double Kills;
        public int FirstEvolutionSecond;
        public readonly double[] CombatPressure = new double[10];
        public readonly double[] SurvivalPressure = new double[10];
        public readonly double[] HitsToDie = new double[10];
        public readonly int[] Levels = new int[10];
    }

    public static void Run(int runs, int minutes, int seed, string curve)
    {
        if (curve == "Both")
        {
            RunCurve(runs, minutes, seed, "Baseline");
            Console.WriteLine();
            RunCurve(runs, minutes, seed, "Current");
            return;
        }

        RunCurve(runs, minutes, seed, curve);
    }

    private static void RunCurve(int runs, int minutes, int seed, string curve)
    {
        List<RunResult> results = new List<RunResult>();
        for (int i = 0; i < runs; i++)
        {
            results.Add(Simulate(minutes, seed + i, curve));
        }

        Console.WriteLine("Closed-loop Monte Carlo balance model");
        Console.WriteLine("Curve: " + curve + ", Runs: " + runs + ", Minutes: " + minutes);
        Console.WriteLine("Assumption: mildly rational player, 3 choices per level, 3 global rerolls.");
        Console.WriteLine();
        Console.WriteLine("final_level_p25,final_level_med,final_level_p75,kills_med,first_evo_p25,first_evo_med,first_evo_p75,no_evo_percent");
        Console.WriteLine(string.Join(",",
            Percentile(results.Select(r => (double)r.FinalLevel), 0.25).ToString("0"),
            Percentile(results.Select(r => (double)r.FinalLevel), 0.50).ToString("0"),
            Percentile(results.Select(r => (double)r.FinalLevel), 0.75).ToString("0"),
            Percentile(results.Select(r => r.Kills), 0.50).ToString("0"),
            FormatSeconds(Percentile(results.Select(r => r.FirstEvolutionSecond > 0 ? (double)r.FirstEvolutionSecond : 999), 0.25)),
            FormatSeconds(Percentile(results.Select(r => r.FirstEvolutionSecond > 0 ? (double)r.FirstEvolutionSecond : 999), 0.50)),
            FormatSeconds(Percentile(results.Select(r => r.FirstEvolutionSecond > 0 ? (double)r.FirstEvolutionSecond : 999), 0.75)),
            (results.Count(r => r.FirstEvolutionSecond <= 0) * 100.0 / runs).ToString("0.0")));
        Console.WriteLine();
        Console.WriteLine("minute,level_med,combat_pressure_p25,combat_pressure_med,combat_pressure_p75,survival_pressure_med,hits_to_die_med");
        for (int minute = 0; minute < Math.Min(minutes, 10); minute++)
        {
            Console.WriteLine(string.Join(",",
                (minute + 1).ToString(),
                Percentile(results.Select(r => (double)r.Levels[minute]), 0.50).ToString("0"),
                Percentile(results.Select(r => r.CombatPressure[minute]), 0.25).ToString("0.00"),
                Percentile(results.Select(r => r.CombatPressure[minute]), 0.50).ToString("0.00"),
                Percentile(results.Select(r => r.CombatPressure[minute]), 0.75).ToString("0.00"),
                Percentile(results.Select(r => r.SurvivalPressure[minute]), 0.50).ToString("0.00"),
                Percentile(results.Select(r => r.HitsToDie[minute]), 0.50).ToString("0.0")));
        }
    }

    private static RunResult Simulate(int minutes, int seed, string curve)
    {
        Random random = new Random(seed);
        BuildState state = new BuildState();
        RunResult result = new RunResult();
        int level = 1;
        double xp = 0;
        double kills = 0;
        const double pickupRate = 0.86;
        int seconds = minutes * 60;

        for (int second = 0; second < seconds; second++)
        {
            double avgHp;
            double avgXp;
            double avgDamage;
            GetAverageEnemy(second, curve, out avgHp, out avgXp, out avgDamage);
            int ignoredMaxEnemies;
            double spawnInterval;
            GetWave(second, curve, out ignoredMaxEnemies, out spawnInterval);
            double spawnRate = 1.0 / spawnInterval;
            double dps = GetEffectiveDps(state, avgHp);
            double killRate = Math.Min(spawnRate, dps / Math.Max(1, avgHp));
            xp += killRate * avgXp * pickupRate;
            kills += killRate;

            while (xp >= GetXpRequirement(level))
            {
                xp -= GetXpRequirement(level);
                level++;
                var choice = state.ChooseUpgrade(random, level);
                if (choice.Item1 == "evolve" && result.FirstEvolutionSecond <= 0)
                {
                    result.FirstEvolutionSecond = second + 1;
                }
            }

            if (second % 60 == 59 && second / 60 < 10)
            {
                int index = second / 60;
                result.Levels[index] = level;
                double combatPressure = spawnRate * avgHp / Math.Max(1, dps);
                result.CombatPressure[index] = combatPressure;
                result.SurvivalPressure[index] = GetSurvivalPressure(state, level, combatPressure, spawnRate, avgDamage, second);
                result.HitsToDie[index] = GetHitsToDie(state, avgDamage);
            }
        }

        result.FinalLevel = level;
        result.Kills = kills;
        return result;
    }

    private static int GetXpRequirement(int level)
    {
        if (level <= 1) return 5;
        if (level <= 5) return 5 + (level - 1) * 4;
        if (level <= 12) return (int)Math.Round(24.0 + (level - 5) * 6.0);
        return (int)Math.Round(70.0 + (level - 12) * 11.0 + Math.Pow(level - 12, 1.35) * 3.4);
    }

    private static void GetWave(double elapsed, string curve, out int maxEnemies, out double spawnInterval)
    {
        if (curve == "Baseline")
        {
            if (elapsed < 120) { maxEnemies = 24; spawnInterval = 0.5; return; }
            if (elapsed < 240) { maxEnemies = 56; spawnInterval = 0.34; return; }
            if (elapsed < 360) { maxEnemies = 110; spawnInterval = 0.24; return; }
            if (elapsed < 480) { maxEnemies = 170; spawnInterval = 0.17; return; }
            maxEnemies = 250; spawnInterval = 0.105; return;
        }

        if (elapsed < 120) { maxEnemies = 24; spawnInterval = 0.5; return; }
        if (elapsed < 240) { maxEnemies = 60; spawnInterval = 0.33; return; }
        if (elapsed < 360) { maxEnemies = 130; spawnInterval = 0.22; return; }
        if (elapsed < 480) { maxEnemies = 215; spawnInterval = 0.145; return; }
        maxEnemies = 285; spawnInterval = 0.10;
    }

    private static Dictionary<string, double> GetMix(double elapsed, string curve)
    {
        Dictionary<string, double> mix = new Dictionary<string, double>();
        if (curve == "Baseline")
        {
            if (elapsed >= 360)
            {
                mix["Walker"] = 0.55;
                mix["Runner"] = 0.17;
                mix["Spitter"] = 0.12;
                mix["Tanker"] = 0.16;
                return mix;
            }

            if (elapsed >= 240)
            {
                mix["Walker"] = 0.55;
                mix["Runner"] = 0.17;
                mix["Spitter"] = 0.28;
                return mix;
            }

            if (elapsed >= 120)
            {
                mix["Walker"] = 0.55;
                mix["Runner"] = 0.45;
                return mix;
            }

            mix["Walker"] = 1.0;
            return mix;
        }

        if (elapsed >= 420)
        {
            mix["Walker"] = 0.42;
            mix["Runner"] = 0.20;
            mix["Spitter"] = 0.17;
            mix["Tanker"] = 0.21;
            return mix;
        }

        if (elapsed >= 300)
        {
            mix["Walker"] = 0.48;
            mix["Runner"] = 0.22;
            mix["Spitter"] = 0.18;
            mix["Tanker"] = 0.12;
            return mix;
        }

        if (elapsed >= 240)
        {
            mix["Walker"] = 0.56;
            mix["Runner"] = 0.20;
            mix["Spitter"] = 0.24;
            return mix;
        }

        if (elapsed >= 120)
        {
            mix["Walker"] = 0.58;
            mix["Runner"] = 0.42;
            return mix;
        }

        mix["Walker"] = 1.0;
        return mix;
    }

    private static double GetHpScale(double elapsed, string curve)
    {
        double minutes = elapsed / 60.0;
        double scale = curve == "Baseline"
            ? Math.Pow(1.0 + 0.24 * minutes, 1.5)
            : Math.Pow(1.0 + 0.255 * minutes, 1.52);
        if (elapsed >= 300) scale *= 1 + (elapsed - 300) / 300.0 * (curve == "Baseline" ? 0.32 : 0.42);
        return scale;
    }

    private static void GetAverageEnemy(double elapsed, string curve, out double avgHp, out double avgXp, out double avgDamage)
    {
        avgHp = 0;
        avgXp = 0;
        avgDamage = 0;
        foreach (var kv in GetMix(elapsed, curve))
        {
            EnemyStats stats = Enemies[kv.Key];
            avgHp += kv.Value * stats.Hp;
            avgXp += kv.Value * stats.Xp;
            avgDamage += kv.Value * stats.Damage;
        }

        avgHp *= GetHpScale(elapsed, curve);
        avgDamage *= 1.0 + (elapsed / 60.0) * 0.08;
    }

    private static double GetHitsToDie(BuildState state, double avgDamage)
    {
        double maxHealth = GetMaxHealth(state);
        double reduction = GetDamageReduction(state);
        double finalDamage = avgDamage * (1.0 - reduction);
        return maxHealth / Math.Max(1.0, finalDamage);
    }

    private static double GetSurvivalPressure(BuildState state, int level, double combatPressure, double spawnRate, double avgDamage, double elapsed)
    {
        double maxHealth = GetMaxHealth(state);
        double reduction = GetDamageReduction(state);
        double levelUpsPerMinute = Math.Max(0.0, (level - 1) / Math.Max(1.0, elapsed / 60.0));
        double healPerMinute = levelUpsPerMinute * maxHealth * GetLevelHealPercent(state) + GetRegenPerSecond(state) * 60.0;
        double effectiveHealthPerMinute = maxHealth + healPerMinute;

        // Exposure estimates how much of the spawned wave actually pressures the player.
        // It rises when enemy HP throughput outpaces the current build's DPS.
        double exposure = Clamp(0.10 + combatPressure * 0.18, 0.08, 0.52);
        double incomingPerMinute = spawnRate * 60.0 * avgDamage * (1.0 - reduction) * exposure;
        return incomingPerMinute / Math.Max(1.0, effectiveHealthPerMinute);
    }

    private static double GetMaxHealth(BuildState state)
    {
        int hazmat;
        state.Passives.TryGetValue("HazmatSuit", out hazmat);
        return 120.0 + hazmat * 16.0;
    }

    private static double GetDamageReduction(BuildState state)
    {
        int armor;
        state.Passives.TryGetValue("NanoArmor", out armor);
        return Math.Min(0.70, armor * 0.05);
    }

    private static double GetRegenPerSecond(BuildState state)
    {
        int hazmat;
        state.Passives.TryGetValue("HazmatSuit", out hazmat);
        return hazmat * 0.28;
    }

    private static double GetLevelHealPercent(BuildState state)
    {
        int defib;
        state.Passives.TryGetValue("Defibrillator", out defib);
        return 0.14 + defib * 0.012;
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    private static double GetEffectiveDps(BuildState state, double avgHp)
    {
        double damage = 1.0;
        double fire = 1.0;
        int pierce = 0;
        int ammo;
        if (state.Passives.TryGetValue("AmmoBox", out ammo)) damage *= Math.Pow(1.07, ammo);
        int overclock;
        if (state.Passives.TryGetValue("Overclock", out overclock)) fire *= Math.Pow(1.08, overclock);
        int propellent;
        if (state.Passives.TryGetValue("Propellent", out propellent))
        {
            if (propellent >= 1) pierce++;
            if (propellent >= 3) pierce++;
            if (propellent >= 5) pierce++;
        }

        int radar;
        if (state.Passives.TryGetValue("Radar", out radar))
        {
            double critChance = radar * 0.035;
            double critDamage = 1.75 + radar * 0.08;
            damage *= 1 + critChance * (critDamage - 1);
        }

        double total = 0;
        foreach (var kv in state.Weapons)
        {
            string weapon = kv.Key;
            int level = kv.Value;
            bool evolved = state.Evolved.Contains(weapon);
            if (weapon == "Pistol")
            {
                double shots = evolved ? 8 : level >= 4 ? 2 : 1;
                double cooldown = 0.42 * (evolved ? 0.42 : level >= 4 ? 0.72 : level >= 2 ? 0.78 : 1.0) / fire;
                double shotDamage = 38 * (level >= 2 ? 1.15 : 1.0) * (1 + Math.Max(0, level - 2) * 0.08) * (evolved ? 1.55 : 1.0) * damage;
                double multi = 1 + 0.22 * pierce + (level >= 3 && !evolved ? 0.33 : 0.0) + (evolved ? 0.75 : 0.0);
                total += shots * shotDamage / cooldown * multi * (evolved ? 0.48 : 0.78);
            }
            else if (weapon == "Shotgun")
            {
                double pellets = evolved ? 24 : level >= 2 ? 7 : 5;
                double cooldown = 1.85 * (evolved ? 0.5 : level >= 4 ? 0.62 : 1.0) / fire;
                double pelletDamage = 8.5 * (1 + (level - 1) * 0.08) * (evolved ? 1.08 : 1.0) * damage;
                total += pellets * pelletDamage / cooldown * (evolved ? 0.38 : 0.76) * (1 + (level >= 3 ? 0.26 : 0.0) + (evolved ? 0.25 : 0.0)) * (1 + Math.Min(0.35, pierce * 0.08));
            }
            else if (weapon == "Tesla")
            {
                double targets = (evolved ? 12 : level >= 2 ? 5 : 3) + (evolved && level >= 4 ? 3 : level >= 4 ? 2 : 0);
                double boltDamage = 28 * (1 + (level - 1) * 0.16) * (level >= 2 ? 1.35 : 1.0) * (evolved ? 1.48 : 1.0) * damage;
                total += boltDamage * targets / 0.98 * fire * 0.56 * (1 + (level >= 4 ? 0.22 : 0.0) + (evolved ? 0.38 : 0.0)) * (1 + (level >= 5 ? 0.15 : 0.0) + (evolved ? 0.12 : 0.0));
            }
            else if (weapon == "Laser")
            {
                total += 27 * (1 + (level - 1) * 0.14) * (evolved ? 1.42 : 1.0) * damage * (1.3 + 0.45 * level + (evolved ? 4.2 : 0.0)) * (1 + (level >= 3 ? 0.25 : 0.0) + (level >= 5 ? 0.2 : 0.0));
            }
            else if (weapon == "Lightblade")
            {
                double cooldown = 1.02 * (evolved ? 0.58 : level >= 3 ? 0.86 : 1.0) / fire;
                double bladeDamage = 44 * (1 + (level - 1) * 0.13) * (evolved ? 1.6 : 1.0) * damage;
                double hits = (1.15 + 0.28 * level) * (evolved ? 2.2 : level >= 5 ? 1.45 : 1.0);
                total += bladeDamage / cooldown * hits * (1 + (level >= 2 ? 0.18 : 0.0) + (level >= 4 ? 0.2 : 0.0));
            }
            else if (weapon == "Singularity")
            {
                double count = evolved ? 3 : level >= 5 ? 2 : 1;
                double cooldown = 4.6 * (evolved ? 0.72 : 1.0) / fire;
                double tick = 0.35 * (evolved ? 0.72 : 1.0);
                double lifetime = (level >= 4 ? 4.4 : 3.35) * (evolved ? 1.35 : 1.0);
                double orbDamage = 10.5 * (1 + (level - 1) * 0.16) * (evolved ? 1.75 : 1.0) * damage;
                double targets = (1.8 + 0.55 * level) * (evolved ? 1.8 : 1.0);
                double percentDamage = (level >= 3 ? 0.012 + level * 0.002 : 0.0) * avgHp;
                total += count * (lifetime / tick) * (orbDamage + percentDamage) * targets / cooldown * 0.72;
            }
        }

        return Math.Max(20, total);
    }

    private static double Percentile(IEnumerable<double> source, double percentile)
    {
        List<double> values = source.OrderBy(v => v).ToList();
        int index = (int)Math.Floor((values.Count - 1) * percentile);
        return values[Math.Max(0, Math.Min(values.Count - 1, index))];
    }

    private static string FormatSeconds(double seconds)
    {
        if (seconds >= 900) return "none";
        int rounded = (int)Math.Round(seconds);
        return (rounded / 60).ToString("00") + ":" + (rounded % 60).ToString("00");
    }
}
"@

Add-Type -TypeDefinition $source -Language CSharp
[BalanceModel]::Run($Runs, $Minutes, $Seed, $Curve)
