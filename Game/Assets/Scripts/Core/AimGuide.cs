using UnityEngine;

namespace ZombieOverdrive.Core
{
    [RequireComponent(typeof(LineRenderer))]
    public class AimGuide : MonoBehaviour
    {
        private const int MaxDashSegments = 24;

        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private Transform player;
        [SerializeField] private SpriteRenderer crosshair;
        [SerializeField] private float startOffset = 0.65f;
        [SerializeField] private float maxDistance = 8f;
        [SerializeField] private float dashLength = 0.35f;
        [SerializeField] private float gapLength = 0.25f;

        private readonly LineRenderer[] dashLines = new LineRenderer[MaxDashSegments];
        private Camera mainCamera;

        private void Awake()
        {
            dashLines[0] = GetComponent<LineRenderer>();
            ConfigureDash(dashLines[0]);
            for (int i = 1; i < dashLines.Length; i++)
            {
                GameObject segment = new GameObject("Aim Dash " + i);
                segment.transform.SetParent(transform, false);
                dashLines[i] = segment.AddComponent<LineRenderer>();
                ConfigureDash(dashLines[i]);
            }
        }

        public void Initialize(PlayerMovement movement, Transform playerTransform, SpriteRenderer cursorRenderer)
        {
            playerMovement = movement;
            player = playerTransform;
            crosshair = cursorRenderer;
        }

        private void LateUpdate()
        {
            if (player == null)
            {
                SetDashCount(0);
                return;
            }

            mainCamera = mainCamera != null ? mainCamera : Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 direction = playerMovement != null ? playerMovement.AimDirection : (mouseWorld - player.position).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector2.right;
            }

            float distance = Mathf.Min(Vector2.Distance(player.position, mouseWorld), maxDistance);
            Vector3 start = player.position + (Vector3)(direction.normalized * startOffset);
            Vector3 end = player.position + (Vector3)(direction.normalized * Mathf.Max(startOffset, distance));
            DrawDashedLine(start, end);

            if (crosshair != null)
            {
                crosshair.transform.position = mouseWorld;
                crosshair.transform.rotation = Quaternion.Euler(0f, 0f, Time.unscaledTime * -90f);
            }
        }

        private void DrawDashedLine(Vector3 start, Vector3 end)
        {
            Vector3 vector = end - start;
            float length = vector.magnitude;
            if (length <= 0.05f)
            {
                SetDashCount(0);
                return;
            }

            Vector3 direction = vector / length;
            int dashCount = Mathf.Min(MaxDashSegments, Mathf.CeilToInt(length / (dashLength + gapLength)));
            SetDashCount(dashCount);
            for (int i = 0; i < dashCount; i++)
            {
                float dashStart = i * (dashLength + gapLength);
                float dashEnd = Mathf.Min(dashStart + dashLength, length);
                dashLines[i].SetPosition(0, start + direction * dashStart);
                dashLines[i].SetPosition(1, start + direction * dashEnd);
            }
        }

        private void SetDashCount(int count)
        {
            for (int i = 0; i < dashLines.Length; i++)
            {
                if (dashLines[i] != null)
                {
                    dashLines[i].enabled = i < count;
                }
            }
        }

        private static void ConfigureDash(LineRenderer dash)
        {
            dash.useWorldSpace = true;
            dash.material = new Material(Shader.Find("Sprites/Default"));
            dash.positionCount = 2;
            dash.startColor = new Color(0.88f, 0.95f, 1f, 0.52f);
            dash.endColor = new Color(0.88f, 0.95f, 1f, 0.12f);
            dash.startWidth = 0.035f;
            dash.endWidth = 0.035f;
            dash.sortingOrder = 14;
            dash.enabled = false;
        }
    }
}
