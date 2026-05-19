using UnityEngine;
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
    }
}
