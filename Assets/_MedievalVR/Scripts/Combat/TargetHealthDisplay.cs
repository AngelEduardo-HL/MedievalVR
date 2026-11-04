using TMPro;
using UnityEngine;

namespace MedievalVR.Combat
{
    [DisallowMultipleComponent]
    public sealed class TargetHealthDisplay : MonoBehaviour
    {
        [SerializeField] DamageableTarget target;
        [SerializeField] RectTransform fill;
        [SerializeField] TMP_Text label;
        Camera viewCamera;

        void OnEnable()
        {
            if (target == null) return;
            target.HealthChanged += Refresh;
            Refresh(target);
        }

        void OnDisable()
        {
            if (target != null) target.HealthChanged -= Refresh;
        }

        void Refresh(DamageableTarget health)
        {
            if (fill != null)
                fill.anchorMax = new Vector2(Mathf.Clamp01(health.CurrentHealth / health.MaximumHealth), 1f);
            if (label != null)
                label.text = $"Cubo de prueba\n{Mathf.CeilToInt(health.CurrentHealth)} / {Mathf.CeilToInt(health.MaximumHealth)}";
        }

        void LateUpdate()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            if (viewCamera == null) return;
            Vector3 forward = transform.position - viewCamera.transform.position;
            if (forward.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }
}
