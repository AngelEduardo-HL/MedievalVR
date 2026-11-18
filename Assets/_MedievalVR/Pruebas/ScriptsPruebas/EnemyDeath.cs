using UnityEngine;
using MedievalVR.Combat;

public class EnemyDeath : MonoBehaviour
{
    void Start()
    {
        GetComponentInChildren<DamageableTarget>().Died += OnDied;
    }

    void OnDied(DamageableTarget target)
    {
        GetComponent<Chase>().enabled = false; 
        GetComponentInChildren<Animator>().SetTrigger("Die"); 
        Destroy(gameObject, 3f);
    }
}