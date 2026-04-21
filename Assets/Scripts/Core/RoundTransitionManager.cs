using System.Collections;
using UnityEngine;
using KawaiiKiller.Cinematics;

public class RoundTransitionManager : MonoBehaviour
{
    public static RoundTransitionManager Instance { get; private set; }

    [SerializeField] private float delayBeforeTransition = 1.5f;

    private ComicManager comicManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetComicManager(ComicManager manager)
    {
        comicManager = manager;
    }

    public void BeginTransition(int completedRound)
    {
        StartCoroutine(TransitionRoutine(completedRound));
    }

    private IEnumerator TransitionRoutine(int completedRound)
    {
        yield return new WaitForSeconds(delayBeforeTransition);

        if (GameManager.Instance == null)
        {
            SceneLoader.Instance.LoadShop();
            yield break;
        }

        if (GameManager.Instance.IsStoryMode)
        {
            SceneLoader.Instance.LoadScene("Game", () =>
            {
                var found = Object.FindObjectOfType<ComicManager>();
                if (found != null)
                {
                    found.PlayRound(completedRound + 1, () =>
                    {
                        if (WaveManager.Instance != null)
                            WaveManager.Instance.StartRound(completedRound + 1);
                    });
                }
                else
                {
                    if (WaveManager.Instance != null)
                        WaveManager.Instance.StartRound(completedRound + 1);
                }
            });
            yield break;
        }

        SceneLoader.Instance.LoadShop();
    }
}