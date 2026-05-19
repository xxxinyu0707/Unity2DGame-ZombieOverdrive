using UnityEngine;
using ZombieOverdrive.Core;

namespace ZombieOverdrive.Combat
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] private string displayName;

        protected PlayerStats Stats { get; private set; }
        protected PlayerMovement Movement { get; private set; }

        public abstract WeaponId Id { get; }
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? Id.ToString() : displayName;
        public int Level { get; private set; }
        public bool IsUnlocked => Level > 0;
        public bool IsEvolved { get; private set; }

        public void Initialize(PlayerStats playerStats, PlayerMovement playerMovement)
        {
            Stats = playerStats;
            Movement = playerMovement;
            OnInitialized();
        }

        public void UnlockOrLevel()
        {
            Level = Mathf.Clamp(Level + 1, 1, 5);
            enabled = true;
            OnLevelChanged();
        }

        public bool CanLevel => Level < 5;

        public void Evolve()
        {
            if (IsEvolved)
            {
                return;
            }

            IsEvolved = true;
            OnEvolved();
        }

        protected virtual void Awake()
        {
            enabled = false;
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnLevelChanged()
        {
        }

        protected virtual void OnEvolved()
        {
        }

        protected Vector2 AimDirection => Movement != null ? Movement.AimDirection : Vector2.right;

        protected float RollDamage(float baseDamage)
        {
            return Stats != null ? Stats.RollDamage(baseDamage) : baseDamage;
        }

        protected float FireRateMultiplier => Stats != null ? Mathf.Max(0.1f, Stats.fireRateMultiplier) : 1f;

        protected float AreaMultiplier => Stats != null ? Mathf.Max(0.2f, Stats.areaMultiplier) : 1f;
    }
}
