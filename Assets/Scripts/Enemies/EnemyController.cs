using UnityEngine;
using KawaiiKiller.Weapons;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyStats stats;

    public float MaxHealth      { get; private set; }
    public float CurrentHealth  { get; private set; }
    public float Damage         { get; private set; }
    public float MoveSpeed      { get; private set; }
    public float AttackRange    { get; private set; }
    public float AttackCooldown { get; private set; }
    public EnemyStats Stats => stats;
    public bool  IsDead         { get; private set; }

    private EnemyInfo _enemyInfo;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action              OnDeath;

    private void Awake()
    {
        _enemyInfo = GetComponent<EnemyInfo>();
    }

    public void InitializeForRound(int round)
    {
        MaxHealth      = stats.GetHealth(round);
        CurrentHealth  = MaxHealth;
        Damage         = stats.GetDamage(round);
        MoveSpeed      = stats.GetMoveSpeed(round);
        AttackRange    = stats.GetAttackRange(round);
        AttackCooldown = stats.GetAttackCooldown(round);
        IsDead         = false;
    }

    public void TakeDamage(DamagePayload payload)
    {
        TakeDamage(payload.BaseDamage);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        // Notify damage tracking
        WaveManager.Instance?.RegisterDamage(amount);

        if (CurrentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        OnDeath?.Invoke();

        string enemyType = (_enemyInfo != null && !string.IsNullOrEmpty(_enemyInfo.EnemyType)) ? _enemyInfo.EnemyType : "Unknown";
        EnemyCategory category = _enemyInfo != null ? _enemyInfo.Category : EnemyCategory.Small;
        WaveManager.Instance.RegisterEnemyDeath(enemyType, category);

        Destroy(gameObject, 0.15f);
    }
}