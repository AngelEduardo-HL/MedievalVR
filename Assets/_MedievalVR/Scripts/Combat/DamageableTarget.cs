using System;
using System.Collections.Generic;
using UnityEngine;

namespace MedievalVR.Combat
{
    [DisallowMultipleComponent]
    public sealed class DamageableTarget : MonoBehaviour
    {
        [SerializeField, Min(1f)] float maximumHealth = 100f;
        [SerializeField, Min(0f), Tooltip("Daño por defecto.")]
        float defaultImpactDamage = 20f;
        [SerializeField, Min(0f), Tooltip("Velocidad mínima (m/s).")]
        float minimumImpactSpeed = 0.5f;
        [SerializeField, Min(0f), Tooltip("Cooldown por objeto (s).")]
        float sameObjectHitCooldown = 0.25f;
        [SerializeField] bool destroyOnDeath = true;
        [SerializeField] Renderer targetRenderer;
        [SerializeField, Min(0f)] float hitFlashDuration = 0.08f;
        [SerializeField] float currentHealth;

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int ColorProperty = Shader.PropertyToID("_Color");
        readonly Dictionary<int, float> lastHitTimes = new Dictionary<int, float>();
        MaterialPropertyBlock propertyBlock;
        Color originalColor;
        float flashUntil;
        bool flashing;

        public float CurrentHealth => currentHealth;
        public float MaximumHealth => maximumHealth;
        public bool IsDead => currentHealth <= 0f;
        public event Action<DamageableTarget> HealthChanged;
        public event Action<DamageableTarget> Died;

        void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            var material = targetRenderer != null ? targetRenderer.sharedMaterial : null;
            originalColor = material != null && material.HasProperty(BaseColor)
                ? material.GetColor(BaseColor)
                : material != null && material.HasProperty(ColorProperty)
                    ? material.GetColor(ColorProperty) : Color.red;
            ResetHealth();
        }

        void OnCollisionEnter(Collision collision)
        {
            Rigidbody otherBody = collision.rigidbody;
            if (IsDead || otherBody == null || otherBody.transform.IsChildOf(transform)) return;

            float impactSpeed = collision.relativeVelocity.magnitude;
            var weapon = otherBody.GetComponent<WeaponImpact>();
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 point = collision.GetContact(i).point;
                float contactSpeed = weapon != null
                    ? weapon.GetContactSpeed(point) : otherBody.GetPointVelocity(point).magnitude;
                impactSpeed = Mathf.Max(impactSpeed, contactSpeed);
            }

            if (float.IsNaN(impactSpeed) || float.IsInfinity(impactSpeed) || impactSpeed < minimumImpactSpeed)
                return;

            int sourceId = otherBody.GetInstanceID();
            if (lastHitTimes.TryGetValue(sourceId, out float lastHit) &&
                Time.time - lastHit < sameObjectHitCooldown) return;

            float damage = weapon != null ? weapon.BaseDamage : defaultImpactDamage;
            if (damage <= 0f) return;
            lastHitTimes[sourceId] = Time.time;
            TakeDamage(damage);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount)) return;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            flashUntil = Time.time + hitFlashDuration;
            flashing = true;
            SetColor(Color.white);
            HealthChanged?.Invoke(this);

            if (IsDead)
            {
                Died?.Invoke(this);
                if (destroyOnDeath) Destroy(gameObject);
            }
        }

        [ContextMenu("Restablecer vida")]
        public void ResetHealth()
        {
            currentHealth = maximumHealth;
            lastHitTimes.Clear();
            flashing = false;
            if (propertyBlock != null) SetColor(originalColor);
            HealthChanged?.Invoke(this);
        }

        void Update()
        {
            if (!flashing || Time.time < flashUntil) return;
            flashing = false;
            SetColor(originalColor);
        }

        void SetColor(Color color)
        {
            if (targetRenderer == null) return;
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColor, color);
            propertyBlock.SetColor(ColorProperty, color);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        void OnValidate()
        {
            maximumHealth = Mathf.Max(1f, maximumHealth);
            defaultImpactDamage = Mathf.Max(0f, defaultImpactDamage);
            minimumImpactSpeed = Mathf.Max(0f, minimumImpactSpeed);
            sameObjectHitCooldown = Mathf.Max(0f, sameObjectHitCooldown);
            if (!Application.isPlaying) currentHealth = maximumHealth;
        }
    }
}
