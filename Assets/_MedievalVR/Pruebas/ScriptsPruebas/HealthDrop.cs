using UnityEngine;

public class HealthDrop : MonoBehaviour
{
    public int addHealthAmount = 20; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Agarrar la vida del jugador
            HealPlayer(addHealthAmount);//Se 
            gameObject.SetActive(false); 
        }
    }

    public void HealPlayer(int playerHealth)
    {
        playerHealth += addHealthAmount;
    }


}
