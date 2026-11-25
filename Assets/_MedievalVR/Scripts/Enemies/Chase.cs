using MedievalVR.Combat;
using UnityEngine;

public class Chase : MonoBehaviour
{
    [Header("Chase Settings")]
    public float speed = 3.5f;
    public float stopDistance = 1.8f;
    public float separationRadius = 1.2f; 
    public float separationForce = 2f;

    [SerializeField, Min(0f), Tooltip("Daño al jugador.")] float attackDamage = 12f;
    [SerializeField, Range(0f, .99f), Tooltip("Momento del golpe.")] float hitPoint = .45f;
    static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    static readonly int AttackState = Animator.StringToHash("Attack");

    Transform player;
    PlayerCombatRig playerCombat;
    Animator anim;
    int lastAttackCycle = -1;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCombat = playerObj.GetComponent<PlayerCombatRig>();
        }

        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (player == null) return;
        if (playerCombat != null && playerCombat.Health.IsDead)
        {
            if (anim != null) anim.SetBool(IsAttacking, false);
            return;
        }

        Vector3 targetPosition = playerCombat != null && playerCombat.Head != null
            ? playerCombat.Head.position : player.position;
        Vector3 targetPos = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance > .01f) transform.LookAt(targetPos);

        bool inRange = distance <= stopDistance;
        UpdateAttack(inRange);
        Vector3 move = inRange ? Vector3.zero : (targetPos - transform.position).normalized * speed;

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

    void UpdateAttack(bool inRange)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return;
        anim.SetBool(IsAttacking, inRange);
        if (playerCombat == null) return;

        var state = anim.GetCurrentAnimatorStateInfo(0);
        if (state.shortNameHash != AttackState)
        {
            lastAttackCycle = -1;
            return;
        }

        int cycle = Mathf.FloorToInt(state.normalizedTime - hitPoint);
        if (cycle < 0 || cycle <= lastAttackCycle) return;
        lastAttackCycle = cycle;
        if (inRange) playerCombat.ReceiveMeleeHit(attackDamage, transform.position);
    }
}
