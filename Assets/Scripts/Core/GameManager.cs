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

    [Header("Navigation")]
    [SerializeField] private string mainMenuScene = "MenuScene";
    [SerializeField] private string shopScene     = "Shop";
    [SerializeField] private string gameScene     = "Game";

    [Header("Comic")]
    [SerializeField] private ComicManager comicManager;

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

        if (IsStoryMode && comicManager != null)
        {
            SceneLoader.Instance.LoadScene(gameScene, () =>
            {
                comicManager.Play(OnIntroComplete);
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

    private void OnIntroComplete()
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
        if (IsStoryMode && comicManager != null && WaveManager.Instance != null)
        {
            int next = WaveManager.Instance.CurrentRound + 1;
            SceneLoader.Instance.LoadScene(gameScene, () =>
            {
                comicManager.PlayRound(next, OnRoundComplete);
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

    private void OnRoundComplete()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.StartRound(WaveManager.Instance.CurrentRound + 1);
    }

    public void ConvertToEndless() => CurrentMode = GameMode.Endless;

    public void GoToMainMenu() => SceneLoader.Instance.LoadScene(mainMenuScene);
}