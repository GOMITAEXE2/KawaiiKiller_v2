using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using KawaiiKiller.Cinematics;
using KawaiiKiller.Player;
using KawaiiKiller.Weapons;

namespace KawaiiKiller.Core
{
    public class GameLoopOrchestrator : MonoBehaviour
    {
        public static GameLoopOrchestrator Instance { get; private set; }

        public event Action OnGameplayReady;
        public event Action OnRoundComplete;

        [Header("Scenes")]
        [SerializeField] private string gameScene = "Game";
        [SerializeField] private string shopScene = "Shop";

        [Header("Cinematics")]
        [SerializeField] private bool hasIntroComic = true;

        private Player.Player _player;
        private EnemySpawner _enemySpawner;
        private WaveManager _waveManager;
        private ComicManager _comicManager;

        private bool _inputEnabled;
        private bool _spawningEnabled;
        private int _currentRound;

        public bool InputEnabled => _inputEnabled;
        public bool SpawningEnabled => _spawningEnabled;
        public int CurrentRound => _currentRound;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _inputEnabled = false;
            _spawningEnabled = false;
            Time.timeScale = 1f; // Ensure time is running at start
        }

        private void Start()
        {
            // Auto-initialize if we start the game directly from the Game scene in the editor
            if (SceneManager.GetActiveScene().name == gameScene)
            {
                Debug.Log("[Orchestrator] Direct start detected in Game scene. Reinitializing...");
                Reinitialize();
            }
        }

        public void Reinitialize()
        {
            ResolveReferences();
            LockInput();
            LockSpawning();
            EvaluateAndStart();
        }

        private void ResolveReferences()
        {
            _player = FindObjectOfType<Player.Player>();
            _enemySpawner = FindObjectOfType<EnemySpawner>();
            _waveManager = FindObjectOfType<WaveManager>();
            _comicManager = FindObjectOfType<ComicManager>();

            Debug.Log($"[Orchestrator] Resolved: Player={_player?.name}, Spawner={_enemySpawner?.name}, Wave={_waveManager?.name}, Comic={_comicManager?.name}");

            if (_player == null)
            {
                Debug.LogError("[Orchestrator] Player not found!");
            }
            else
            {
                Debug.Log("[Orchestrator] Calling LockInput()");
                LockInput();
            }
        }

        private void EvaluateAndStart()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[Orchestrator] GameManager.Instance is null!");
                return;
            }

            bool isStory = GameManager.Instance.IsStoryMode;
            bool isFirstRound = _currentRound <= 1; // Only play intro on round 1
            bool hasComicContent = _comicManager != null && _comicManager.HasComicContent();

            if (isStory && isFirstRound && hasComicContent && hasIntroComic)
            {
                StartCinematicSequence();
            }
            else
            {
                string reason = !isStory ? "IsStoryMode is FALSE" : 
                               (!isFirstRound ? $"Current round is {_currentRound} (Intro only on Round 1)" : 
                               (!hasComicContent ? "No Comic Content found" : "hasIntroComic is FALSE"));
                
                Debug.Log($"[Orchestrator] Skipping cinematic sequence. Reason: {reason}");
                StartGameplay();
            }
        }

        private void StartCinematicSequence()
        {
            if (_comicManager == null)
            {
                Debug.LogWarning("[Orchestrator] ComicManager is null, skipping cinematic.");
                StartGameplay();
                return;
            }

            _comicManager.OnFinished -= HandleCinematicFinished;
            _comicManager.OnFinished += HandleCinematicFinished;
            _comicManager.Play(null);
        }

        private void HandleCinematicFinished()
        {
            if (_comicManager != null)
                _comicManager.OnFinished -= HandleCinematicFinished;

            StartGameplay();
        }

        private void StartGameplay()
        {
            UnlockInput();
            UnlockSpawning();

            if (_waveManager != null)
            {
                _currentRound = _waveManager.CurrentRound > 0 ? _waveManager.CurrentRound : 1;
                _waveManager.StartRound(_currentRound);
            }
            else
            {
                Debug.LogWarning("[Orchestrator] WaveManager is null!");
            }

            OnGameplayReady?.Invoke();
        }

        public void LockInput()
        {
            _inputEnabled = false;
            _player?.SetUIMode(true); // Unlock cursor for UI/Cinematics
        }

        public void UnlockInput()
        {
            _inputEnabled = true;
            _player?.SetUIMode(false); // Lock cursor for Gameplay
        }

        public void LockSpawning()
        {
            _spawningEnabled = false;
            if (_enemySpawner != null) _enemySpawner.SetSpawningEnabled(false);
        }

        public void UnlockSpawning()
        {
            _spawningEnabled = true;
            if (_enemySpawner != null) _enemySpawner.SetSpawningEnabled(true);
        }

        public void NextRound()
        {
            _currentRound++;

            if (GameManager.Instance == null || !GameManager.Instance.IsStoryMode)
            {
                ReloadGameScene(() => _waveManager?.StartRound(_currentRound));
                return;
            }

            bool hasRoundComic = _comicManager != null && _comicManager.HasPagesForRound(_currentRound);
            if (hasRoundComic)
            {
                ReloadGameScene(() =>
                {
                    ResolveReferences();
                    if (_comicManager != null)
                    {
                        _comicManager.OnFinished -= HandleCinematicFinished;
                        _comicManager.OnFinished += HandleCinematicFinished;
                        
                        Debug.Log($"[Orchestrator] Starting round comic for round {_currentRound}");
                        _comicManager.PlayRound(_currentRound, null);
                    }
                    else
                    {
                        StartGameplay();
                    }
                });
            }
            else
            {
                _waveManager?.StartRound(_currentRound);
            }
        }

        private void ReloadGameScene(Action onLoaded)
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(gameScene, onLoaded);
            }
            else
            {
                Debug.LogWarning("[Orchestrator] SceneLoader is null! Falling back to standard SceneManager (Editor testing).");
                StartCoroutine(FallbackLoadScene(gameScene, onLoaded));
            }
        }

        private IEnumerator FallbackLoadScene(string sceneName, Action onLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone) yield return null;
            
            onLoaded?.Invoke();
            Reinitialize();
        }

        public void GoToShop()
        {
            UnlockInput();
            LockSpawning();
            
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(shopScene);
            }
            else
            {
                Debug.LogWarning("[Orchestrator] SceneLoader is null! Falling back to standard SceneManager (Editor testing).");
                SceneManager.LoadScene(shopScene);
            }
        }

        public void CompleteRound()
        {
            SaveCurrentProgress();
            OnRoundComplete?.Invoke();
        }

        private void SaveCurrentProgress()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsStoryMode) return;

            var economy = FindObjectOfType<Player.Player>()?.GetComponent<PlayerEconomy>();
            var weaponController = FindObjectOfType<PlayerWeaponController>();

            if (economy != null && weaponController != null)
            {
                Debug.Log($"[Orchestrator] Saving progress: Round {_currentRound}, Money {economy.CurrentMoney}");
                SaveSystem.Instance?.Save(economy, weaponController);
            }
        }
    }
}