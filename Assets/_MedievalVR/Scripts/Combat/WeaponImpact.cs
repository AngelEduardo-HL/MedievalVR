using UnityEngine;

namespace MedievalVR.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WeaponImpact : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Daño por golpe.")]
        float baseDamage = 20f;

        Rigidbody body;
        Vector3 previousLinearVelocity;
        Vector3 previousAngularVelocity;
        Vector3 previousCenterOfMass;

        public float BaseDamage => baseDamage;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.maxAngularVelocity = 30f;
        }

        void FixedUpdate()
        {
            previousLinearVelocity = body.linearVelocity;
            previousAngularVelocity = body.angularVelocity;
            previousCenterOfMass = body.worldCenterOfMass;
        }

        public float GetContactSpeed(Vector3 point)
        {
            // Keep the incoming angular motion even if the collision solver has
            // already slowed the Rigidbody by the time OnCollisionEnter runs.
            Vector3 incomingVelocity = previousLinearVelocity +
                Vector3.Cross(previousAngularVelocity, point - previousCenterOfMass);
            return Mathf.Max(body.GetPointVelocity(point).magnitude, incomingVelocity.magnitude);
        }
    }
}
