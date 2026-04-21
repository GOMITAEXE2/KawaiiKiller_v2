using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawn Limits")]
    [SerializeField] private int   maxSimultaneous    = 50;
    [SerializeField] private float initialSpawnDelay  = 0.3f;
    [SerializeField] private float deathSpawnCooldown = 1.5f;

    [Header("Enemy Pool")]
    [SerializeField] private List<EnemyPoolEntry> enemyEntries;

    [Header("Spawn Points")]
    [SerializeField] private SpawnPoint[] spawnPoints;

    private int remainingToSpawn;
    private int currentRound;
    private bool _spawningEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _spawningEnabled = true;
    }

    public void SetSpawningEnabled(bool enabled) => _spawningEnabled = enabled;

    public void BeginSpawning(int total, int round)
    {
        if (!_spawningEnabled)
        {
            Debug.Log("[EnemySpawner] Spawning is disabled, ignoring BeginSpawning.");
            return;
        }

        remainingToSpawn = total;
        currentRound     = round;

        Debug.Log($"[EnemySpawner] BeginSpawning: total={total}, round={round}");

        if (enemyEntries == null || enemyEntries.Count == 0)
        {
            Debug.LogError("[EnemySpawner] No enemy entries configured!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] No spawn points configured!");
            return;
        }

        StartCoroutine(InitialFillRoutine());
    }

    public void NotifyEnemyDied()
    {
        if (remainingToSpawn > 0)
            StartCoroutine(CooldownSpawnRoutine());
    }

    // Llena hasta el límite simultáneo al inicio de la ronda
    private IEnumerator InitialFillRoutine()
    {
        while (remainingToSpawn > 0 && WaveManager.Instance.EnemiesAlive < maxSimultaneous)
        {
            TrySpawnOne();
            yield return new WaitForSeconds(initialSpawnDelay);
        }
    }

    private IEnumerator CooldownSpawnRoutine()
    {
        yield return new WaitForSeconds(deathSpawnCooldown);

        if (remainingToSpawn > 0 && WaveManager.Instance.EnemiesAlive < maxSimultaneous)
            TrySpawnOne();
    }

    private void TrySpawnOne()
    {
        if (remainingToSpawn <= 0)
        {
            Debug.Log($"[EnemySpawner] TrySpawnOne: no more to spawn (remaining={remainingToSpawn})");
            return;
        }

        if (enemyEntries == null || enemyEntries.Count == 0)
        {
            Debug.LogError("[EnemySpawner] TrySpawnOne: enemyEntries empty!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] TrySpawnOne: spawnPoints empty!");
            return;
        }

        SpawnPoint point = GetWeightedSpawnPoint();
        EnemyPoolEntry entry = GetEnemyEntry();

        if (point == null)
        {
            Debug.LogError("[EnemySpawner] TrySpawnOne: GetWeightedSpawnPoint returned null!");
            return;
        }

        if (entry == null)
        {
            Debug.LogError("[EnemySpawner] TrySpawnOne: GetEnemyEntry returned null!");
            return;
        }

        if (entry.prefab == null)
        {
            Debug.LogError($"[EnemySpawner] TrySpawnOne: entry prefab is null for {entry.enemyName}!");
            return;
        }

        Debug.Log($"[EnemySpawner] Spawning {entry.enemyName} at {point.name}");

        SetLastSpawnedEntry(entry);

        GameObject enemy = Instantiate(entry.prefab, point.transform.position, point.transform.rotation);
        enemy.name = entry.enemyName;

        var enemyInfo = enemy.AddComponent<EnemyInfo>();
        enemyInfo.Initialize(entry.enemyName, entry.isElite);

        if (enemy.TryGetComponent<EnemyController>(out var controller))
            controller.InitializeForRound(currentRound);

        if (enemy.TryGetComponent<EnemyAI>(out var ai))
            ai.Initialize();

        WaveManager.Instance.RegisterEnemySpawned();
        remainingToSpawn--;
    }

    private SpawnPoint GetWeightedSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;

        float total = 0f;
        foreach (var sp in spawnPoints) total += sp.Weight;

        float roll = Random.Range(0f, total);
        float acc  = 0f;

        foreach (var sp in spawnPoints)
        {
            acc += sp.Weight;
            if (roll <= acc) return sp;
        }

        return spawnPoints[spawnPoints.Length - 1];
    }

    private EnemyPoolEntry _lastSpawnedEntry;

    private EnemyPoolEntry GetEnemyEntry()
    {
        bool elites = WaveManager.Instance.ElitesUnlocked;

        var valid = enemyEntries.FindAll(e => !e.isElite || elites);
        if (valid.Count == 0) return null;

        float total = 0f;
        foreach (var e in valid) total += e.spawnWeight;

        float roll = Random.Range(0f, total);
        float acc  = 0f;

        foreach (var e in valid)
        {
            acc += e.spawnWeight;
            if (roll <= acc) return e;
        }

        return valid[valid.Count - 1];
    }

    public void SetLastSpawnedEntry(EnemyPoolEntry entry)
    {
        _lastSpawnedEntry = entry;
    }

    public bool IsEnemyElite(string enemyType)
    {
        foreach (var entry in enemyEntries)
        {
            if (entry.enemyName == enemyType) return entry.isElite;
        }
        return false;
    }

    public string GetLastEnemyTypeSpawned()
    {
        return _lastSpawnedEntry?.enemyName ?? "Unknown";
    }
}

[System.Serializable]
public class EnemyPoolEntry
{
    public string     enemyName;
    public GameObject prefab;
    [Range(0f, 10f)]
    public float      spawnWeight = 1f;
    public bool       isElite;
}