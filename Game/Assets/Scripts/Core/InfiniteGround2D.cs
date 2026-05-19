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

            Vector2 center = new Vector2(
                Mathf.Round(target.position.x / tileSize) * tileSize,
                Mathf.Round(target.position.y / tileSize) * tileSize);

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
                    tile.position = new Vector3(center.x + x * tileSize, center.y + y * tileSize, 1f);
                    index++;
                }
            }
        }
    }
}
