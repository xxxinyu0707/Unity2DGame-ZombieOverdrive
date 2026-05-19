using UnityEngine;
using ZombieOverdrive.Enemies;

namespace ZombieOverdrive.Combat
{
    public class LaserWeapon : WeaponBase
    {
        [SerializeField] private float baseDamagePerSecond = 32f;
        [SerializeField] private float range = 10f;
        [SerializeField] private float beamWidth = 0.45f;
        [SerializeField] private LayerMask enemyMask;

        private readonly RaycastHit2D[] hits = new RaycastHit2D[64];
        private LineRenderer beam;

        public override WeaponId Id => WeaponId.Laser;

        private void Update()
        {
            if (!IsUnlocked || Stats == null || Movement == null)
            {
                if (beam != null)
                {
                    beam.enabled = false;
                }

                return;
            }

            FireBeam();
        }

        protected override void OnInitialized()
        {
            beam = gameObject.AddComponent<LineRenderer>();
            beam.material = new Material(Shader.Find("Sprites/Default"));
            beam.positionCount = 2;
            beam.startColor = new Color(1f, 0.2f, 0.12f, 0.9f);
            beam.endColor = new Color(1f, 0.2f, 0.12f, 0.15f);
            beam.startWidth = 0.11f;
            beam.endWidth = 0.04f;
            beam.enabled = false;
            beam.sortingOrder = 9;
        }

        private void FireBeam()
        {
            float width = beamWidth * AreaMultiplier * (Level >= 2 ? 1.35f : 1f);
            int count = Physics2D.CircleCastNonAlloc(transform.position, width, AimDirection, hits, range, enemyMask);
            float damage = RollDamage(baseDamagePerSecond * (1f + (Level - 1) * 0.18f)) * Time.deltaTime;
            if (beam != null)
            {
                beam.enabled = true;
                beam.SetPosition(0, transform.position);
                beam.SetPosition(1, transform.position + (Vector3)(AimDirection * range));
            }

            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = hits[i].collider.GetComponent<EnemyHealth>();
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                enemy.TakeDamage(damage);
            }
        }
    }
}
