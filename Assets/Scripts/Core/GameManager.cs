using UnityEngine;
using UnityEngine.SceneManagement;
using KawaiiKiller.Core;
using KawaiiKiller.Player;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameMode { Story, Endless }

    public GameMode CurrentMode   { get; private set; }
    public int      CurrentRound  => WaveManager.Instance?.CurrentRound ?? 0;
    public bool     IsStoryMode   => CurrentMode == GameMode.Story;
    public bool     IsEndlessMode => CurrentMode == GameMode.Endless;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MenuScene";
    [SerializeField] private string shopScene     = "Shop";
    [SerializeField] private string gameScene     = "Game";

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
        SceneLoader.Instance.LoadScene(gameScene, null);
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
        SceneLoader.Instance.LoadScene(gameScene, null);
    }

    public void GoToNextRound()
    {
        if (GameLoopOrchestrator.Instance != null)
            GameLoopOrchestrator.Instance.NextRound();
    }

    public void ConvertToEndless() => CurrentMode = GameMode.Endless;

    public void GoToMainMenu() => SceneLoader.Instance.LoadScene(mainMenuScene);
}