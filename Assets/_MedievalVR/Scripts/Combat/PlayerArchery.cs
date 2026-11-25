using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MedievalVR.Combat
{
    [RequireComponent(typeof(PlayerCombatRig), typeof(BodyInventory))]
    public sealed class PlayerArchery : MonoBehaviour
    {
        [SerializeField, Min(.05f), Tooltip("Tensado mínimo (m).")] float minimumDraw = .10f;
        [SerializeField, Min(.10f), Tooltip("Tensado máximo (m).")] float maximumDraw = .30f;
        [SerializeField, Min(8f), Tooltip("Velocidad máxima de flecha.")] float maximumArrowSpeed = 24f;

        PlayerCombatRig rig;
        BodyInventory inventory;
        CharacterController bodyCollider;
        bool previousGrip;
        bool drawingFromQuiver;
        float quiverStartDepth;

        XRGrabInteractable bowGrab;
        BowString bowString;
        XRGrabInteractable arrowGrab;
        ArrowProjectile arrow;
        bool trackPosition;
        bool trackRotation;
        bool throwOnDetach;
        bool originalKinematic;
        bool detectCollisions;
        Vector3 direction;
        float draw;

        void Awake()
        {
            rig = GetComponent<PlayerCombatRig>();
            inventory = GetComponent<BodyInventory>();
            bodyCollider = GetComponent<CharacterController>();
        }

        void LateUpdate()
        {
            var rightHand = rig.RightHand;
            bool grip = rightHand != null && rightHand.isActiveAndEnabled && rightHand.selectInput.ReadIsPerformed();
            bool pressed = grip && !previousGrip;
            previousGrip = grip;

            if (rightHand == null || rig.Health.IsDead || !rig.HasHeadPosition || !rig.HasRightPosition)
            {
                drawingFromQuiver = false;
                FinishDraw(false);
                return;
            }

            UpdateQuiver(grip, pressed);
            UpdateBow(grip);
        }

        void UpdateQuiver(bool grip, bool pressed)
        {
            if (!grip || rig.RightHand.hasSelection)
            {
                drawingFromQuiver = false;
                return;
            }

            Vector3 handPosition = rig.RightController.position;
            float depth = rig.RelativeToHead(handPosition).z;
            if (pressed && inventory.IsReachingForQuiver(handPosition))
            {
                drawingFromQuiver = true;
                quiverStartDepth = depth;
            }

            if (drawingFromQuiver && depth - quiverStartDepth >= .22f)
            {
                drawingFromQuiver = false;
                inventory.TryDrawArrow();
            }
        }

        void UpdateBow(bool grip)
        {
            var leftBow = rig.HeldWeapon(rig.LeftHand, EquipmentKind.Bow);
            if (arrow != null)
            {
                if (!rig.HasLeftPosition || leftBow != bowGrab ||
                    (grip && !rig.RightHand.IsSelecting(arrowGrab)))
                {
                    FinishDraw(false);
                    return;
                }

                UpdateDrawPose();
                if (!grip)
                    FinishDraw(draw >= minimumDraw && !arrowGrab.isSelected);
                return;
            }

            if (!grip || !rig.HasLeftPosition || leftBow == null ||
                !rig.AtChest(rig.LeftController) || !rig.AtChest(rig.RightController) ||
                !leftBow.TryGetComponent(out BowString cord))
                return;

            var heldArrow = rig.HeldWeapon(rig.RightHand, EquipmentKind.Arrow);
            if (heldArrow == null || !heldArrow.TryGetComponent(out ArrowProjectile shaft) ||
                (cord.StringRest - rig.RightController.position).sqrMagnitude > .18f * .18f)
                return;

            BeginDraw(leftBow, cord, heldArrow, shaft);
            UpdateDrawPose();
        }

        void BeginDraw(XRGrabInteractable bow, BowString cord, XRGrabInteractable heldArrow, ArrowProjectile shaft)
        {
            bowGrab = bow;
            bowString = cord;
            arrowGrab = heldArrow;
            arrow = shaft;
            trackPosition = heldArrow.trackPosition;
            trackRotation = heldArrow.trackRotation;
            throwOnDetach = heldArrow.throwOnDetach;

            heldArrow.trackPosition = false;
            heldArrow.trackRotation = false;
            heldArrow.throwOnDetach = false;
            var body = arrow.Body;
            originalKinematic = body.isKinematic;
            detectCollisions = body.detectCollisions;
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.isKinematic = true;
            body.detectCollisions = false;
        }

        void UpdateDrawPose()
        {
            Vector3 rest = bowString.ArrowRest;
            Vector3 pull = rest - rig.RightController.position;
            float reach = pull.magnitude;
            float restingLength = Vector3.Distance(rest, bowString.StringRest);
            draw = Mathf.Clamp(reach - restingLength, 0f, maximumDraw);
            if (reach < .01f) return;

            direction = pull / reach;
            var tail = rest - direction * (restingLength + draw);
            arrow.Body.position = tail;
            arrow.Body.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            bowString.DrawTo(tail);
        }

        void FinishDraw(bool fire)
        {
            if (bowString != null) bowString.ResetDraw();
            if (arrow != null)
            {
                arrowGrab.trackPosition = trackPosition;
                arrowGrab.trackRotation = trackRotation;
                arrowGrab.throwOnDetach = throwOnDetach;
                arrow.Body.isKinematic = arrowGrab.isSelected && originalKinematic;
                arrow.Body.detectCollisions = detectCollisions;

                if (fire)
                {
                    float tension = Mathf.InverseLerp(0f, maximumDraw, draw);
                    arrow.Launch(direction, Mathf.Lerp(8f, maximumArrowSpeed, tension), bodyCollider, bowGrab);
                }
            }

            bowGrab = null;
            bowString = null;
            arrowGrab = null;
            arrow = null;
            draw = 0f;
        }

        void OnDisable() => FinishDraw(false);
    }
}
