using UnityEngine;
using MedievalVR.Combat;

public class EnemyDeath : MonoBehaviour
{
    ScoreManager scoreManager; 

    void Start()
    {
        GetComponentInChildren<DamageableTarget>().Died += OnDied;
    }

    void OnDied(DamageableTarget target)
    {
        scoreManager.AddPoints(10);
        GetComponent<Chase>().enabled = false; 
        this.GetComponentInChildren<EnemyWeapon>().UnequipWeapon();
        GetComponentInChildren<Animator>().SetTrigger("Die");
        Destroy(gameObject, 3f);
    }

}