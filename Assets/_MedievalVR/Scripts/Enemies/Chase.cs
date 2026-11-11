using UnityEngine;

public class Chase : MonoBehaviour
{
    [Header("Chase Settings")]
    public float speed = 5f; 
    public float stopDistance = 2f;

    bool isMoving = false;
    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; //Ubica al jugador
        isMoving = true;
    }

    void Update()
    {
        if (player == null) return;
        ChaseTarget();
    }

    public void ChaseTarget() 
    {
        if (!isMoving) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopDistance) //Persigue al jugador :)
        {

            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.LookAt(player);
        }

    }



}
