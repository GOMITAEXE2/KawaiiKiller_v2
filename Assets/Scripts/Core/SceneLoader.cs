using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using KawaiiKiller.Core;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadShop()                            => LoadScene("Shop");
    public void LoadMainMenu()                        => LoadScene("MainMenu");
    public void LoadScene(string name)                => StartCoroutine(LoadAsync(name, null));
    public void LoadScene(string name, Action onDone) => StartCoroutine(LoadAsync(name, onDone));

    private IEnumerator LoadAsync(string sceneName, Action onLoaded)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;
        yield return op;

        onLoaded?.Invoke();

        if (GameLoopOrchestrator.Instance != null)
            GameLoopOrchestrator.Instance.Reinitialize();
    }
}