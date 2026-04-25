using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using KawaiiKiller.Core;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("UI Loading Screen")]
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private VideoPlayer loadingVideo;
    [SerializeField] private GameObject loadingGifIndicator;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
        }
    }

    public void LoadShop()                            => LoadScene("Shop");
    public void LoadMainMenu()                        => LoadScene("MainMenu");
    public void LoadScene(string name)                => StartCoroutine(LoadAsync(name, null));
    public void LoadScene(string name, Action onDone) => StartCoroutine(LoadAsync(name, onDone));

    private IEnumerator LoadAsync(string sceneName, Action onLoaded)
    {
        // 1. Mostrar pantalla de carga y reproducir video hacia adelante
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 1f;
            loadingCanvasGroup.blocksRaycasts = true;
        }

        if (loadingGifIndicator != null) loadingGifIndicator.SetActive(false); // Apagado hasta que empiece a cargar

        if (loadingVideo != null)
        {
            loadingVideo.playbackSpeed = 1f; // Velocidad normal hacia adelante
            loadingVideo.Play();
            
            // Esperar a que el video termine su animacion de entrada (ej: puertas cerrandose)
            while (loadingVideo.isPlaying && loadingVideo.time < loadingVideo.length - 0.1f)
            {
                yield return null;
            }
            loadingVideo.Pause(); // Pausamos el video al final de su clip
        }

        // 2. Mostrar el "GIF" de cargando
        if (loadingGifIndicator != null) loadingGifIndicator.SetActive(true);

        // 3. Cargar la escena en segundo plano
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        // 4. Activar la nueva escena
        op.allowSceneActivation = true;
        yield return op;

        onLoaded?.Invoke();

        if (GameLoopOrchestrator.Instance != null)
            GameLoopOrchestrator.Instance.Reinitialize();

        // 5. Ocultar el "GIF" porque ya cargó
        if (loadingGifIndicator != null) loadingGifIndicator.SetActive(false);

        // 6. Reproducir el video en reversa (ej: puertas abriendose)
        if (loadingVideo != null)
        {
            loadingVideo.playbackSpeed = -1f; // Reversa
            loadingVideo.Play();

            // Esperar a que el video llegue al inicio (tiempo <= 0.1)
            while (loadingVideo.isPlaying && loadingVideo.time > 0.1f)
            {
                yield return null;
            }
            loadingVideo.Stop();
        }

        // 7. Ocultar la pantalla de carga
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
        }
    }
}