using UnityEngine;
using MedievalVR.Combat;
using System.Collections;

public class EnemyDeath : MonoBehaviour
{
    ScoreManager scoreManager;
    public GameObject scorePoints;
    public GameObject healthText;

    void Start()
    {
        GetComponentInChildren<DamageableTarget>().Died += OnDied;
    }

    void OnDied(DamageableTarget target)
    {
        //scoreManager.AddPoints(10);
        GetComponent<Chase>().enabled = false;
        healthText.SetActive(false);
        GetComponentInChildren<Animator>().SetTrigger("Die");
        this.GetComponentInChildren<EnemyWeapon>().UnequipWeapon();
        this.GetComponentInChildren<EnemyWeapon>().DropPlayerWeapon();
        StartCoroutine(ShowPoints(2.0f));
        Destroy(gameObject, 5f);
    }

    IEnumerator ShowPoints(float segundos)
    {
        //Fade del numero
        scorePoints.SetActive(true);
        yield return new WaitForSeconds(segundos);
        scorePoints.SetActive(false);
    }

}