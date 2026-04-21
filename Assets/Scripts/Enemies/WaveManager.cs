using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Scaling")]
    [SerializeField] private int baseEnemyCount = 10;
    [SerializeField, Range(1f, 2f)] private float scaleFactorPerRound = 1.20f;
    [SerializeField] private int eliteUnlockRound = 10;

    [Header("Rewards")]
    [SerializeField] private int baseRewardPerKill = 10;
    [SerializeField] private float eliteMultiplier = 2f;

    public int CurrentRound    { get; private set; }
    public int TotalThisRound  { get; private set; }
    public int EnemiesKilled   { get; private set; }
    public int EnemiesAlive    { get; private set; }
    public bool ElitesUnlocked => CurrentRound >= eliteUnlockRound;

    public event Action<int> OnRoundStarted;
    public event Action<int> OnRoundEnded;

    private Dictionary<string, int> _killsByType = new Dictionary<string, int>();
    private Dictionary<string, int> _rewardByType = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> KillsByType => _killsByType;
    public int TotalRewardThisRound { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Ensure the object is a root object before calling DontDestroyOnLoad
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void StartRound(int roundNumber)
    {
        CurrentRound   = roundNumber;
        TotalThisRound = CalculateTotalEnemies(roundNumber);
        EnemiesKilled  = 0;
        EnemiesAlive   = 0;
        TotalRewardThisRound = 0;
        _killsByType.Clear();

        Debug.Log($"[WaveManager] StartRound: {roundNumber}, total enemies: {TotalThisRound}");
        Debug.Log($"[WaveManager] EnemySpawner.Instance: {EnemySpawner.Instance?.name ?? "NULL"}");

        OnRoundStarted?.Invoke(CurrentRound);

        if (EnemySpawner.Instance == null)
        {
            Debug.LogError("[WaveManager] EnemySpawner.Instance is NULL!");
            return;
        }

        EnemySpawner.Instance.BeginSpawning(TotalThisRound, CurrentRound);
    }

    public void RegisterEnemySpawned()
    {
        EnemiesAlive++;
    }

    public void RegisterEnemyDeath(string enemyType)
    {
        EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);
        EnemiesKilled++;

        if (!_killsByType.ContainsKey(enemyType))
            _killsByType[enemyType] = 0;
        _killsByType[enemyType]++;

        int reward = CalculateReward(enemyType);
        TotalRewardThisRound += reward;

        KawaiiKiller.Player.PlayerEconomy.Instance?.AddMoney(reward);
        KawaiiKiller.Player.PlayerWeaponController.Instance?.CurrentWeapon?.NotifyEnemyKilled(null);

        EnemySpawner.Instance.NotifyEnemyDied();

        if (EnemiesKilled >= TotalThisRound && EnemiesAlive <= 0)
            EndRound();
    }

    public int CalculateReward(string enemyType)
    {
        bool isElite = EnemySpawner.Instance.IsEnemyElite(enemyType);
        int baseReward = _rewardByType.ContainsKey(enemyType) ? _rewardByType[enemyType] : baseRewardPerKill;
        return Mathf.RoundToInt(baseReward * (isElite ? eliteMultiplier : 1f));
    }

    public void SetRewardForType(string enemyType, int reward)
    {
        _rewardByType[enemyType] = reward;
    }

    public int CalculateTotalEnemies(int round)
    {
        return Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(scaleFactorPerRound, round - 1));
    }

    private void EndRound()
    {
        OnRoundEnded?.Invoke(CurrentRound);
    }
}