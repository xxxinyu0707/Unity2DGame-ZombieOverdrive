using UnityEngine;

namespace ZombieOverdrive.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float smoothTime = 0.05f;

        private Rigidbody2D body;
        private PlayerStats stats;
        private Vector2 currentVelocity;
        private Vector2 smoothVelocity;

        public Vector2 AimDirection { get; private set; } = Vector2.right;

        public void Initialize(PlayerStats playerStats)
        {
            stats = playerStats;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (rawInput.sqrMagnitude > 1f)
            {
                rawInput.Normalize();
            }

            float speed = stats != null ? stats.MoveSpeed : 4f;
            currentVelocity = Vector2.SmoothDamp(currentVelocity, rawInput * speed, ref smoothVelocity, smoothTime);
            UpdateAimDirection();
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + currentVelocity * Time.fixedDeltaTime);
        }

        private void UpdateAimDirection()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 aim = mouseWorld - transform.position;
            if (aim.sqrMagnitude > 0.001f)
            {
                AimDirection = aim.normalized;
            }
        }
    }
}
