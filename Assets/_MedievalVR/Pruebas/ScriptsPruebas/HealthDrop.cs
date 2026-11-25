using MedievalVR.Combat;
using UnityEngine;

public class HealthDrop : MonoBehaviour
{
    [Min(1)] public int addHealthAmount = 20;

    bool collected;

    public void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        var player = other.GetComponentInParent<DamageableTarget>();
        if (player == null || !player.CompareTag("Player") || !player.TryHeal(addHealthAmount)) return;

        collected = true;
        Destroy(gameObject);
    }
}
