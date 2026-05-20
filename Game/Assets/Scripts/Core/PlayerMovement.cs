using UnityEngine;

namespace ZombieOverdrive.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float acceleration = 42f;
        [SerializeField] private float deceleration = 54f;

        private Rigidbody2D body;
        private PlayerStats stats;
        private Vector2 rawInput;
        private Vector2 currentVelocity;

        public Vector2 AimDirection { get; private set; } = Vector2.right;

        public void Initialize(PlayerStats playerStats)
        {
            stats = playerStats;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.freezeRotation = true;
        }

        private void Update()
        {
            rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (rawInput.sqrMagnitude > 1f)
            {
                rawInput.Normalize();
            }

            UpdateAimDirection();
        }

        private void FixedUpdate()
        {
            float speed = stats != null ? stats.MoveSpeed : 4f;
            Vector2 targetVelocity = rawInput * speed;
            float rate = rawInput.sqrMagnitude > 0.001f ? acceleration : deceleration;
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);
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
