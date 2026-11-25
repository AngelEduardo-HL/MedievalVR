using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MedievalVR.Combat
{
    [RequireComponent(typeof(PlayerCombatRig))]
    public sealed class BodyInventory : MonoBehaviour
    {
        [SerializeField] XRGrabInteractable arrowPrefab;
        [SerializeField, Min(1)] int arrowCapacity = 6;
        [SerializeField, Min(0)] int startingArrows = 6;
        [SerializeField] Vector3 beltOffset = new Vector3(.30f, -.65f, .03f);
        [SerializeField] Vector3 bowOffset = new Vector3(-.25f, -.33f, -.28f);
        [SerializeField] Vector3 quiverOffset = new Vector3(.24f, -.23f, -.25f);

        PlayerCombatRig rig;
        EquipmentSocket belt;
        EquipmentSocket bowMount;
        EquipmentSocket quiver;

        public int Arrows { get; private set; }

        void Start()
        {
            rig = GetComponent<PlayerCombatRig>();
            if (rig.Head == null || rig.RightHand == null || arrowPrefab == null)
            {
                Debug.LogError("Falta el mando derecho o el prefab de flecha.", this);
                enabled = false;
                return;
            }

            Arrows = Mathf.Clamp(startingArrows, 0, arrowCapacity);
            var manager = rig.RightHand.interactionManager;
            belt = CreateSocket("Cinturón", EquipmentKind.BeltWeapon, .17f, manager);
            bowMount = CreateSocket("Soporte del arco", EquipmentKind.Bow, .20f, manager);
            quiver = CreateSocket("Carcaj", EquipmentKind.Arrow, .21f, manager);
            quiver.selectEntered.AddListener(StoreArrow);
            PositionSockets();
        }

        void LateUpdate()
        {
            if (rig.HasHeadPosition) PositionSockets();
        }

        void PositionSockets()
        {
            Quaternion heading = rig.BodyRotation;
            Quaternion hanging = heading * Quaternion.Euler(0f, 0f, 180f);
            Vector3 head = rig.Head.position;
            Vector3 waist = head + heading * beltOffset;
            float floorClearance = Vector3.Dot(waist - transform.position, transform.up);
            waist += transform.up * Mathf.Max(0f, .30f - floorClearance);

            belt.transform.SetPositionAndRotation(waist, hanging);
            bowMount.transform.SetPositionAndRotation(head + heading * bowOffset, hanging);
            quiver.transform.SetPositionAndRotation(head + heading * quiverOffset, heading);
            quiver.socketActive = Arrows < arrowCapacity;
        }

        public bool IsReachingForQuiver(Vector3 handPosition)
        {
            return quiver != null && rig.RelativeToHead(handPosition).z < -.10f &&
                (quiver.transform.position - handPosition).sqrMagnitude <= .28f * .28f;
        }

        public bool TryDrawArrow()
        {
            var hand = rig.RightHand;
            if (Arrows <= 0 || hand == null || !hand.isActiveAndEnabled || hand.hasSelection ||
                hand.interactionManager == null) return false;

            var arrow = Instantiate(arrowPrefab, hand.transform.position, hand.transform.rotation);
            var handAttach = hand.GetAttachTransform(arrow);
            var arrowAttach = arrow.GetAttachTransform(hand);
            Quaternion correction = handAttach.rotation * Quaternion.Inverse(arrowAttach.rotation);
            Vector3 offset = arrow.transform.position - arrowAttach.position;
            arrow.transform.SetPositionAndRotation(
                handAttach.position + correction * offset, correction * arrow.transform.rotation);

            hand.interactionManager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)arrow);
            if (!hand.IsSelecting(arrow))
            {
                Destroy(arrow.gameObject);
                return false;
            }

            Arrows--;
            quiver.socketActive = true;
            return true;
        }

        void StoreArrow(SelectEnterEventArgs args)
        {
            if (Arrows >= arrowCapacity) return;
            Arrows++;
            quiver.socketActive = Arrows < arrowCapacity;
            Destroy(args.interactableObject.transform.gameObject);
        }

        EquipmentSocket CreateSocket(string socketName, EquipmentKind kind, float radius, XRInteractionManager manager)
        {
            var holder = new GameObject(socketName);
            holder.SetActive(false);
            holder.transform.SetParent(transform, false);
            var trigger = holder.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = radius;
            var socket = holder.AddComponent<EquipmentSocket>();
            socket.Kind = kind;
            socket.interactionManager = manager;
            socket.showInteractableHoverMeshes = false;
            socket.recycleDelayTime = .35f;
            holder.SetActive(true);
            return socket;
        }
    }
}
