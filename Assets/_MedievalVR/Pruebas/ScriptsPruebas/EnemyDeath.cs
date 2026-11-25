using UnityEngine;
using MedievalVR.Combat;
using System.Collections;

public class EnemyDeath : MonoBehaviour
{
    public GameObject scorePoints;
    public GameObject healthText;

    public GameObject healthDrop;

    public int scoreValue = 10;
    int healthDropChance = 30; 

    void Start()
    {
        GetComponentInChildren<DamageableTarget>().Died += OnDied;
    }

    void OnDied(DamageableTarget target)
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.AddPoints(scoreValue);
        }

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
        DropHealth();
    }

    public void DropHealth()
    {
        if (Random.Range(0, 100) < healthDropChance)
        {
            Instantiate(healthDrop, transform.position, Quaternion.identity);
        }
        else 
        { 
            return; 
        }
    }


}