using System;
using UnityEngine;

namespace ZombieOverdrive.UI
{
    [Serializable]
    public struct UpgradeIconEntry
    {
        public string Id;
        public Sprite Sprite;
    }

    public class UpgradeIconLibrary : MonoBehaviour
    {
        [SerializeField] private UpgradeIconEntry[] icons;

        public Sprite Get(string id)
        {
            if (string.IsNullOrEmpty(id) || icons == null)
            {
                return null;
            }

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i].Id == id)
                {
                    return icons[i].Sprite;
                }
            }

            return null;
        }
    }
}
