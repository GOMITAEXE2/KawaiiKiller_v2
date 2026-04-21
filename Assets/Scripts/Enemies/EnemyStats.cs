using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "KawaiiKiller/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Base Stats")]
    public float baseHealth        = 100f;
    public float baseDamage        = 10f;
    public float baseMoveSpeed     = 3.5f;
    public float baseAttackRange   = 1.5f;
    public float baseAttackCooldown = 1f;

    [Header("Round Scaling")]
    [SerializeField, Range(0f, 1f)] private float healthScalePerRound = 0.15f;
    [SerializeField, Range(0f, 1f)] private float damageScalePerRound = 0.10f;

    public float GetHealth(int round) =>
        baseHealth * Mathf.Pow(1f + healthScalePerRound, round - 1);

    public float GetDamage(int round) =>
        baseDamage * Mathf.Pow(1f + damageScalePerRound, round - 1);

    public float GetMoveSpeed(int round) => baseMoveSpeed;

    public float GetAttackRange(int round)   => baseAttackRange;
    public float GetAttackCooldown(int round) => baseAttackCooldown;
}