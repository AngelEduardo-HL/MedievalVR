using UnityEngine;

public class Chase : MonoBehaviour
{
    [Header("Chase Settings")]
    public float speed = 3.5f;
    public float stopDistance = 1.8f;
    public float separationRadius = 1.2f; 
    public float separationForce = 2f;


    Transform player;
    Animator anim;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distance = Vector3.Distance(transform.position, targetPos);

        transform.LookAt(targetPos);

        Vector3 move = Vector3.zero;
        if (distance > stopDistance)
        {
            move = (targetPos - transform.position).normalized * speed;
            if (anim != null) anim.SetBool("IsAttacking", false);
        }
        else
        {
            if (anim != null) anim.SetBool("IsAttacking", true);
        }

        Collider[] nearby = Physics.OverlapSphere(transform.position, separationRadius);
        foreach (Collider col in nearby)
        {
            if (col.gameObject != gameObject && col.GetComponent<Chase>() != null)
            {
                Vector3 pushAway = (transform.position - col.transform.position);
                pushAway.y = 0f;
                move += pushAway.normalized * separationForce;
            }
        }

        transform.position += move * Time.deltaTime;
    }
}
