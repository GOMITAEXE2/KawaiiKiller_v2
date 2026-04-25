using UnityEngine;
using UnityEngine.AI;
using KawaiiKiller.Player;
using KawaiiKiller.Weapons;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyController))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Chase, Attack }

    [Header("Attack")]
    [SerializeField] private float attackAngleTolerance = 25f;

    private NavMeshAgent   agent;
    private EnemyController controller;
    private Transform      player;

    private State  currentState = State.Chase;
    private float  attackTimer;

    private void Awake()
    {
        agent      = GetComponent<NavMeshAgent>();
        controller = GetComponent<EnemyController>();
    }

    private void OnEnable()
    {
        controller.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        controller.OnDeath -= HandleDeath;
    }

    public void Initialize()
    {
        agent.speed = controller.MoveSpeed;
        attackTimer = 0f;
        player = GameObject.FindWithTag("Player")?.transform;
        TransitionTo(State.Chase);
    }

    private void Update()
    {
        if (controller.IsDead) return;
        if (player == null) return;

        attackTimer += Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= controller.AttackRange)
        {
            if (currentState != State.Attack)
                TransitionTo(State.Attack);
        }
        else
        {
            if (currentState != State.Chase)
                TransitionTo(State.Chase);
        }

        switch (currentState)
        {
            case State.Chase:  UpdateChase();  break;
            case State.Attack: UpdateAttack(); break;
        }
    }

    private void UpdateChase()
    {
        agent.SetDestination(player.position);
    }

    private void UpdateAttack()
    {
        FaceTarget(player.position);

        if (attackTimer >= controller.AttackCooldown && IsFacingTarget(player.position))
        {
            attackTimer = 0f;
            ExecuteAttack();
        }
    }

    private void TransitionTo(State next)
    {
        currentState = next;
        agent.isStopped = (next == State.Attack);
    }

    private bool IsFacingTarget(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        return Vector3.Angle(transform.forward, dir) <= attackAngleTolerance;
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, look, Time.deltaTime * agent.angularSpeed * Mathf.Deg2Rad);
    }

    private void ExecuteAttack()
    {
        if (player != null && player.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(new DamagePayload { BaseDamage = controller.Damage });
        }
    }

    private void HandleDeath()
    {
        agent.isStopped = true;
        agent.enabled = false;
        enabled = false;
    }
}