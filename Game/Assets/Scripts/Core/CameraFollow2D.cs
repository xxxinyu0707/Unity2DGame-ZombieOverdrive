using UnityEngine;

namespace ZombieOverdrive.Core
{
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private float lookAheadWeight = 0.2f;
        [SerializeField] private float maxLookAhead = 3f;

        private Vector3 velocity;
        private float shakeTimer;
        private float shakeMagnitude;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 targetPosition = target.position;
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 lookAhead = Vector3.ClampMagnitude(mouseWorld - target.position, maxLookAhead);
                targetPosition = target.position + lookAhead * lookAheadWeight;
            }

            targetPosition.z = transform.position.z;

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                targetPosition += (Vector3)Random.insideUnitCircle * shakeMagnitude * (shakeTimer / 0.25f);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }

        public void Shake(float magnitude, float duration = 0.25f)
        {
            shakeMagnitude = magnitude;
            shakeTimer = duration;
        }
    }
}
