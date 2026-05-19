using UnityEngine;

namespace ZombieOverdrive.Utility
{
    public class Poolable : MonoBehaviour
    {
        public GameObjectPool Owner { get; private set; }

        public void SetOwner(GameObjectPool owner)
        {
            Owner = owner;
        }

        public void Release()
        {
            if (Owner != null)
            {
                Owner.Release(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
