using UnityEngine;
using ZombieOverdrive.Audio;
using ZombieOverdrive.Combat;
using ZombieOverdrive.Core;
using ZombieOverdrive.Enemies;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Pickups
{
    public enum ResourcePickupType
    {
        Gold,
        Chicken,
        Magnet,
        Bomb
    }

    public class ResourcePickup : MonoBehaviour
    {
        [SerializeField] private ResourcePickupType type;
        [SerializeField] private int goldValue = 10;
        [SerializeField] private float collectDistance = 0.32f;
        [SerializeField] private float pullSpeed = 10f;
        [SerializeField] private LayerMask enemyMask;
        [SerializeField] private Sprite goldSprite;
        [SerializeField] private Sprite chickenSprite;
        [SerializeField] private Sprite magnetSprite;
        [SerializeField] private Sprite bombSprite;

        private Poolable poolable;
        private Transform pullTarget;
        private PlayerCollector collector;

        public ResourcePickupType Type => type;

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        public void Configure(ResourcePickupType pickupType, int value, LayerMask mask)
        {
            type = pickupType;
            goldValue = Mathf.Max(1, value);
            enemyMask = mask;
            pullTarget = null;
            collector = null;
            UpdateVisual();
        }

        public void PullTo(Transform target, PlayerCollector targetCollector)
        {
            pullTarget = target;
            collector = targetCollector;
        }

        private void Update()
        {
            if (pullTarget == null)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, pullTarget.position, pullSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, pullTarget.position) <= collectDistance)
            {
                Collect();
            }
        }

        private void Collect()
        {
            if (collector != null)
            {
                switch (type)
                {
                    case ResourcePickupType.Chicken:
                        collector.HealPercent(0.2f);
                        GameAudio.Play(GameSound.Heal, 0.85f);
                        break;
                    case ResourcePickupType.Magnet:
                        collector.PullAllPickups();
                        GameAudio.Play(GameSound.Magnet, 0.85f);
                        break;
                    case ResourcePickupType.Bomb:
                        TriggerBomb();
                        GameAudio.Play(GameSound.Bomb, 0.9f);
                        break;
                    default:
                        collector.AddRunGold(goldValue);
                        GameAudio.Play(GameSound.Gold, 0.55f);
                        break;
                }
            }

            Release();
        }

        private void TriggerBomb()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 12f, enemyMask);
            for (int i = 0; i < hits.Length; i++)
            {
                EnemyHealth health = hits[i].GetComponent<EnemyHealth>();
                if (health == null || !health.IsAlive)
                {
                    continue;
                }

                if (health.IsBoss)
                {
                    health.TakeDamage(350f);
                    EnemyController controller = hits[i].GetComponent<EnemyController>();
                    if (controller != null)
                    {
                        Vector2 away = (hits[i].transform.position - transform.position).normalized;
                        controller.ApplyKnockback(away * 5f);
                    }
                }
                else
                {
                    health.TakeDamage(99999f);
                }
            }

            CombatVisuals.SpawnBombBlast(transform.position, 4.5f);
        }

        private void Release()
        {
            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void UpdateVisual()
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite == null)
            {
                return;
            }

            switch (type)
            {
                case ResourcePickupType.Chicken:
                    if (chickenSprite != null) sprite.sprite = chickenSprite;
                    sprite.color = new Color(1f, 0.36f, 0.24f, 1f);
                    transform.localScale = Vector3.one * 0.46f;
                    break;
                case ResourcePickupType.Magnet:
                    if (magnetSprite != null) sprite.sprite = magnetSprite;
                    sprite.color = new Color(0.35f, 0.95f, 1f, 1f);
                    transform.localScale = Vector3.one * 0.5f;
                    break;
                case ResourcePickupType.Bomb:
                    if (bombSprite != null) sprite.sprite = bombSprite;
                    sprite.color = new Color(0.12f, 0.12f, 0.14f, 1f);
                    transform.localScale = Vector3.one * 0.52f;
                    break;
                default:
                    if (goldSprite != null) sprite.sprite = goldSprite;
                    sprite.color = new Color(1f, 0.82f, 0.18f, 1f);
                    transform.localScale = Vector3.one * 0.36f;
                    break;
            }
        }
    }
}
