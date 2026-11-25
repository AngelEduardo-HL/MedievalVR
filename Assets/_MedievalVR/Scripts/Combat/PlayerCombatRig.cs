using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace MedievalVR.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XROrigin), typeof(DamageableTarget))]
    public sealed class PlayerCombatRig : MonoBehaviour
    {
        TrackedPoseDriver headPose;
        TrackedPoseDriver leftPose;
        TrackedPoseDriver rightPose;

        public DamageableTarget Health { get; private set; }
        public Transform Head { get; private set; }
        public Transform LeftController { get; private set; }
        public Transform RightController { get; private set; }
        public NearFarInteractor LeftHand { get; private set; }
        public NearFarInteractor RightHand { get; private set; }
        public bool HasHeadPosition => HasPosition(headPose);
        public bool HasLeftPosition => HasPosition(leftPose);
        public bool HasRightPosition => HasPosition(rightPose);

        public Quaternion BodyRotation
        {
            get
            {
                Vector3 forward = Vector3.ProjectOnPlane(Head.forward, transform.up);
                return forward.sqrMagnitude > .01f
                    ? Quaternion.LookRotation(forward, transform.up) : transform.rotation;
            }
        }

        void Awake()
        {
            Health = GetComponent<DamageableTarget>();
            Head = GetComponent<XROrigin>().Camera?.transform;
            var move = GetComponentInChildren<DynamicMoveProvider>(true);
            if (Head == null || move == null || move.leftControllerTransform == null || move.rightControllerTransform == null)
            {
                Debug.LogError("Faltan el visor o los mandos del XR Origin.", this);
                return;
            }

            LeftController = move.leftControllerTransform;
            RightController = move.rightControllerTransform;
            LeftHand = LeftController.GetComponentInChildren<NearFarInteractor>(true);
            RightHand = RightController.GetComponentInChildren<NearFarInteractor>(true);
            headPose = Head.GetComponent<TrackedPoseDriver>();
            leftPose = LeftController.GetComponent<TrackedPoseDriver>();
            rightPose = RightController.GetComponent<TrackedPoseDriver>();
        }

        public Vector3 RelativeToHead(Vector3 position)
        {
            return Quaternion.Inverse(BodyRotation) * (position - Head.position);
        }

        public bool AtChest(Transform hand)
        {
            Vector3 position = RelativeToHead(hand.position);
            return position.y >= -.58f && position.y <= -.10f;
        }

        public XRGrabInteractable HeldWeapon(XRBaseInputInteractor hand, EquipmentKind kind)
        {
            if (hand == null || !hand.isActiveAndEnabled) return null;
            foreach (var selected in hand.interactablesSelected)
                if (selected is XRGrabInteractable grab &&
                    grab.TryGetComponent(out WeaponImpact weapon) && weapon.Kind == kind)
                    return grab;
            return null;
        }

        public void ReceiveMeleeHit(float damage, Vector3 attackerPosition)
        {
            if (Health.IsDead || BlocksAttack(attackerPosition)) return;
            Health.TakeDamage(damage);
        }

        bool BlocksAttack(Vector3 attackerPosition)
        {
            if (!HasHeadPosition || !HasLeftPosition || HeldWeapon(LeftHand, EquipmentKind.Shield) == null ||
                !LeftHand.selectInput.ReadIsPerformed() || !AtChest(LeftController))
                return false;

            Vector3 shield = RelativeToHead(LeftController.position);
            Vector3 attacker = RelativeToHead(attackerPosition);
            if (shield.z < .12f || attacker.z <= 0f) return false;
            shield.y = 0f;
            attacker.y = 0f;
            return Vector3.Dot(shield.normalized, attacker.normalized) >= .5f;
        }

        static bool HasPosition(TrackedPoseDriver pose)
        {
            if (pose == null || !pose.isActiveAndEnabled) return false;
            var action = pose.trackingStateInput.action;
            return action != null && (action.ReadValue<int>() & (int)InputTrackingState.Position) != 0;
        }
    }
}
