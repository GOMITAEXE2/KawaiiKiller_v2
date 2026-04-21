using UnityEngine;
using UnityEngine.SceneManagement;
using KawaiiKiller.Core;
using KawaiiKiller.Cinematics;
using KawaiiKiller.Player;
using KawaiiKiller.Weapons;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameMode { Story, Endless }

    public GameMode CurrentMode   { get; private set; }
    public int      CurrentRound  => WaveManager.Instance?.CurrentRound ?? 0;
    public bool     IsStoryMode   => CurrentMode == GameMode.Story;
    public bool     IsEndlessMode => CurrentMode == GameMode.Endless;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MenuScene";
    [SerializeField] private string shopScene     = "Shop";
    [SerializeField] private string gameScene     = "Game";

    [Header("Cinematics")]
    [SerializeField] private CinematicManager cinematicManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewGame(GameMode mode)
    {
        CurrentMode = mode;
        SaveSystem.Instance?.ClearSave();

        if (IsStoryMode && cinematicManager != null)
        {
            SceneLoader.Instance.LoadScene(gameScene, () =>
            {
                cinematicManager.PlayIntroCinematic(OnIntroCinematicComplete);
            });
        }
        else
        {
            SceneLoader.Instance.LoadScene(gameScene, () =>
            {
                WaveManager.Instance.StartRound(1);
            });
        }
    }

    private void OnIntroCinematicComplete()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.StartRound(1);
    }

    public void ContinueGame()
    {
        var save = SaveSystem.Instance?.Load();
        if (save == null) return;

        CurrentMode = save.gameMode;

        PersistentPlayer.Instance?.CaptureStateFromSave(save);

        SceneLoader.Instance.LoadScene(shopScene);
    }

    public void RestartCurrentMode()
    {
        SceneLoader.Instance.LoadScene(gameScene, () =>
        {
            WaveManager.Instance?.StartRound(1);
        });
    }

    public void GoToNextRound()
    {
        if (IsStoryMode && cinematicManager != null && WaveManager.Instance != null)
        {
            int nextRound = WaveManager.Instance.CurrentRound + 1;
            SceneLoader.Instance.LoadScene(gameScene, () =>
            {
                cinematicManager.PlayRoundCinematic(nextRound - 1, OnRoundCinematicComplete);
            });
        }
        else
        {
            SceneLoader.Instance.LoadScene(gameScene, () =>
            {
                WaveManager.Instance?.StartRound(WaveManager.Instance.CurrentRound + 1);
            });
        }
    }

    private void OnRoundCinematicComplete()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.StartRound(WaveManager.Instance.CurrentRound + 1);
    }

    public void ConvertToEndless()
    {
        CurrentMode = GameMode.Endless;
    }

    public void GoToMainMenu()
    {
        SceneLoader.Instance.LoadScene(mainMenuScene);
    }
}