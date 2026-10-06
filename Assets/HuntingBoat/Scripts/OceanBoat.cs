using UnityEngine;

namespace HuntingBoat
{
    public sealed class OceanBoat : MonoBehaviour
    {
        public Rigidbody Body { get; private set; }
        public Transform Visual { get; set; }
        public float Throttle { get; set; }
        public float Steering { get; set; }
        public bool Anchored { get; set; }
        public int EngineLevel { get; set; } = 1;
        public float SpeedKmh { get { return new Vector2(Body.linearVelocity.x, Body.linearVelocity.z).magnitude * 3.6f; } }
        public float Heading { get { return transform.eulerAngles.y; } }
        private float collisionCooldown;
        public System.Action<float> OnImpact;

        private void Awake()
        {
            Body = gameObject.AddComponent<Rigidbody>(); Body.mass = 900; Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.Interpolate; Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            var collider = gameObject.AddComponent<BoxCollider>(); collider.center = new Vector3(0, .5f, 0); collider.size = new Vector3(3.4f, 1.6f, 7.2f);
        }
        public static float Wave(float x, float z, float time)
        {
            return Mathf.Sin(x * .035f + time * 1.1f) * .26f + Mathf.Sin(z * .055f - time * .85f) * .16f;
        }
        private void FixedUpdate()
        {
            Vector3 velocity = Body.linearVelocity;
            float targetHeight = .45f + Wave(transform.position.x, transform.position.z, Time.time);
            Body.AddForce(Vector3.up * ((targetHeight - transform.position.y) * 14 - velocity.y * 6), ForceMode.Acceleration);
            Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
            Body.AddForce(-horizontal * (Anchored ? 3.5f : .30f), ForceMode.Acceleration);
            float forwardSpeed = Vector3.Dot(horizontal, transform.forward);
            float limit = 10f + 1.5f * (EngineLevel - 1);
            if (!Anchored && (Mathf.Abs(forwardSpeed) < limit || Mathf.Sign(Throttle) != Mathf.Sign(forwardSpeed)))
                Body.AddForce(transform.forward * Throttle * (3.8f + .3f * EngineLevel), ForceMode.Acceleration);
            Vector3 lateral = transform.right * Vector3.Dot(horizontal, transform.right);
            Body.AddForce(-lateral * 1.7f, ForceMode.Acceleration);
            if (!Anchored)
                Body.MoveRotation(Body.rotation * Quaternion.Euler(0, Steering * (12 + Mathf.Min(horizontal.magnitude, 12) * 2.5f) * Time.fixedDeltaTime, 0));
            Body.angularVelocity *= .93f;
            if (Visual) Visual.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.2f) * 2.5f, 0, -Steering * Mathf.Min(6, horizontal.magnitude) + Mathf.Sin(Time.time) * 1.8f);
            if (transform.position.magnitude > 870)
            {
                Vector3 p = transform.position; p.y = 0; Body.AddForce(-p.normalized * 8, ForceMode.Acceleration);
            }
            collisionCooldown -= Time.fixedDeltaTime;
        }
        private void OnCollisionEnter(Collision hit)
        {
            if (collisionCooldown > 0) return;
            collisionCooldown = 1;
            OnImpact?.Invoke(hit.relativeVelocity.magnitude);
        }
    }
}
