using UnityEngine;

namespace ZombieOverdrive.Core
{
    public class InfiniteGround2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float tileSize = 30f;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null || tileSize <= 0f)
            {
                return;
            }

            int centerX = Mathf.FloorToInt(target.position.x / tileSize);
            int centerY = Mathf.FloorToInt(target.position.y / tileSize);
            Vector2 center = new Vector2(centerX * tileSize, centerY * tileSize);

            int index = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (index >= transform.childCount)
                    {
                        return;
                    }

                    Transform tile = transform.GetChild(index);
                    int tileX = centerX + x;
                    int tileY = centerY + y;
                    tile.position = new Vector3(center.x + x * tileSize, center.y + y * tileSize, 1f);
                    SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.color = GetStableTileColor(tileX, tileY);
                    }

                    index++;
                }
            }
        }

        private static Color GetStableTileColor(int x, int y)
        {
            unchecked
            {
                int hash = x * 73856093 ^ y * 19349663;
                hash ^= hash >> 13;
                hash *= 1274126177;
                float tone = 0.88f + Mathf.Abs(hash % 9) * 0.012f;
                return new Color(tone, tone, tone, 1f);
            }
        }
    }
}
