using UnityEngine;
using MedievalVR.Combat;

public class EnemyDeath : MonoBehaviour
{
    DamageableTarget health;

    void OnEnable()
    {
        health = GetComponentInChildren<DamageableTarget>();
        if (health != null) health.Died += OnDied;
    }

    void OnDisable()
    {
        if (health != null) health.Died -= OnDied;
    }

    void OnDied(DamageableTarget target)
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPoints(10);
        if (TryGetComponent(out Chase chase)) chase.enabled = false;
        GetComponentInChildren<EnemyWeapon>()?.UnequipWeapon();
        var anim = GetComponentInChildren<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null) anim.SetTrigger("Die");
        Destroy(gameObject, 3f);
    }

}
