using UnityEngine;

namespace MedievalVR.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WeaponImpact : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Daño por golpe.")]
        float baseDamage = 20f;
        [SerializeField, Min(0.1f), Tooltip("Velocidad mínima (m/s).")]
        float minimumImpactSpeed = 1.5f;

        Rigidbody body;
        Vector3 previousLinearVelocity;
        Vector3 previousAngularVelocity;
        Vector3 previousCenterOfMass;

        public float BaseDamage => baseDamage;
        public float MinimumImpactSpeed => minimumImpactSpeed;

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

        public float GetIncomingContactSpeed(Vector3 point)
        {
            // La colisión ya pudo cambiar la velocidad del Rigidbody.
            Vector3 velocity = previousLinearVelocity +
                Vector3.Cross(previousAngularVelocity, point - previousCenterOfMass);
            return velocity.magnitude;
        }
    }
}
