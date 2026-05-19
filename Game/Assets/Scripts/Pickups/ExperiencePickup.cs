using UnityEngine;
using ZombieOverdrive.Audio;
using ZombieOverdrive.Core;
using ZombieOverdrive.Utility;

namespace ZombieOverdrive.Pickups
{
    public class ExperiencePickup : MonoBehaviour
    {
        [SerializeField] private float pullSpeed = 9f;
        [SerializeField] private float collectDistance = 0.25f;

        private Poolable poolable;
        private int value = 1;
        private Transform pullTarget;
        private LevelSystem levelSystem;

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        public void SetValue(int xpValue)
        {
            value = Mathf.Max(1, xpValue);
            pullTarget = null;
            levelSystem = null;
            UpdateVisual();
        }

        public void PullTo(Transform target, LevelSystem targetLevelSystem)
        {
            pullTarget = target;
            levelSystem = targetLevelSystem;
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
                levelSystem.AddExperience(value);
                GameAudio.Play(GameSound.Pickup, value >= 5 ? 0.65f : 0.35f);
                if (poolable != null)
                {
                    poolable.Release();
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void UpdateVisual()
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite == null)
            {
                return;
            }

            if (value >= 100)
            {
                sprite.color = new Color(1f, 0.2f, 0.18f, 1f);
                transform.localScale = Vector3.one * 0.6f;
            }
            else if (value >= 20)
            {
                sprite.color = new Color(1f, 0.75f, 0.12f, 1f);
                transform.localScale = Vector3.one * 0.52f;
            }
            else if (value >= 5)
            {
                sprite.color = new Color(0.2f, 1f, 0.45f, 1f);
                transform.localScale = Vector3.one * 0.43f;
            }
            else
            {
                sprite.color = new Color(0.25f, 0.55f, 1f, 1f);
                transform.localScale = Vector3.one * 0.35f;
            }
        }
    }
}
