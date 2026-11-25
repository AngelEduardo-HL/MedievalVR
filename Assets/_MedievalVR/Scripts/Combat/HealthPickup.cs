using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MedievalVR.Combat
{
    public sealed class HealthPickup : XRSimpleInteractable
    {
        [SerializeField, Min(1f), Tooltip("Vida que recupera.")]
        float healing = 30f;

        bool collected;

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            var player = interactor.transform.GetComponentInParent<PlayerCombatRig>();
            return !collected && player != null && player.Health.CanHeal && base.IsSelectableBy(interactor);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            var player = args.interactorObject.transform.GetComponentInParent<PlayerCombatRig>();
            if (!collected && player != null && player.Health.TryHeal(healing))
            {
                collected = true;
                Destroy(gameObject);
            }
        }
    }
}
