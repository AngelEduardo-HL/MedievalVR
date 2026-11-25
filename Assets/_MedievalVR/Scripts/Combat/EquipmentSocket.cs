using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MedievalVR.Combat
{
    public sealed class EquipmentSocket : XRSocketInteractor
    {
        public EquipmentKind Kind { get; set; }

        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return socketActive && !hasSelection && Accepts(interactable) && base.CanHover(interactable);
        }

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return socketActive && Accepts(interactable) && base.CanSelect(interactable);
        }

        bool Accepts(IXRInteractable interactable)
        {
            return interactable is XRGrabInteractable grab &&
                grab.TryGetComponent(out WeaponImpact weapon) && weapon.Kind == Kind &&
                (!grab.TryGetComponent(out ArrowProjectile arrow) || !arrow.IsFlying);
        }
    }
}
