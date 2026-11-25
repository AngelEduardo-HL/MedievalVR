using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MedievalVR.Combat
{
    public enum EquipmentKind { Other, BeltWeapon, Bow, Arrow, Shield }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WeaponImpact : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Daño por golpe.")]
        float baseDamage = 20f;
        [SerializeField, Min(0.1f), Tooltip("Velocidad mínima (m/s).")]
        float minimumImpactSpeed = 1.5f;
        [SerializeField] EquipmentKind equipmentKind;

        Rigidbody body;
        XRGrabInteractable grab;
        Vector3 previousLinearVelocity;
        Vector3 previousAngularVelocity;
        Vector3 previousCenterOfMass;

        public float BaseDamage => baseDamage;
        public float MinimumImpactSpeed => minimumImpactSpeed;
        public EquipmentKind Kind => equipmentKind;
        public bool CanDealImpactDamage => isActiveAndEnabled && !(grab?.firstInteractorSelecting is XRSocketInteractor);

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            TryGetComponent(out grab);
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
