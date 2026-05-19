using UnityEngine;

namespace ZombieOverdrive.Combat
{
    [RequireComponent(typeof(LineRenderer))]
    public class TransientLine : MonoBehaviour
    {
        private LineRenderer line;
        private float timer;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
        }

        public void Show(Vector3 start, Vector3 end, Color color, float width, float seconds)
        {
            if (line == null)
            {
                line = GetComponent<LineRenderer>();
            }

            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            timer = seconds;
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
