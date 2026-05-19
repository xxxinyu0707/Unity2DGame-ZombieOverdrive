using System.Collections.Generic;
using UnityEngine;

namespace ZombieOverdrive.Utility
{
    public class GameObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int prewarmCount = 24;

        private readonly Queue<GameObject> available = new Queue<GameObject>();

        public GameObject Prefab => prefab;

        public void Configure(GameObject sourcePrefab, int initialCount)
        {
            prefab = sourcePrefab;
            prewarmCount = Mathf.Max(0, initialCount);
            Prewarm();
        }

        private void Awake()
        {
            Prewarm();
        }

        private void Prewarm()
        {
            if (prefab == null || available.Count > 0)
            {
                return;
            }

            for (int i = 0; i < prewarmCount; i++)
            {
                GameObject instance = CreateInstance();
                Release(instance);
            }
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogError($"{name} has no prefab assigned.");
                return null;
            }

            GameObject instance = available.Count > 0 ? available.Dequeue() : CreateInstance();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        public T Get<T>(Vector3 position, Quaternion rotation) where T : Component
        {
            GameObject instance = Get(position, rotation);
            return instance != null ? instance.GetComponent<T>() : null;
        }

        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform);
            available.Enqueue(instance);
        }

        private GameObject CreateInstance()
        {
            GameObject instance = Instantiate(prefab, transform);
            Poolable poolable = instance.GetComponent<Poolable>();
            if (poolable == null)
            {
                poolable = instance.AddComponent<Poolable>();
            }

            poolable.SetOwner(this);
            return instance;
        }
    }
}
