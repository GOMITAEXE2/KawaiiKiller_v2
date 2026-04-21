using System.Collections;
using UnityEngine;

public class RoundTransitionManager : MonoBehaviour
{
    public static RoundTransitionManager Instance { get; private set; }

    [SerializeField] private float delayBeforeTransition = 1.5f;

    private CinematicManager cinematicManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetCinematicManager(CinematicManager manager)
    {
        cinematicManager = manager;
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

        if (GameManager.Instance.IsStoryMode && cinematicManager != null)
        {
            bool hasCinematic = cinematicManager.HasCinematicForRound(completedRound + 1);

            if (hasCinematic)
            {
                SceneLoader.Instance.LoadScene("Game", () =>
                {
                    var newCinematic = Object.FindObjectOfType<CinematicManager>();
                    if (newCinematic != null)
                    {
                        newCinematic.PlayRoundCinematic(completedRound + 1, () =>
                        {
                            if (WaveManager.Instance != null)
                                WaveManager.Instance.StartRound(completedRound + 1);
                        });
                    }
                });
                yield break;
            }
        }

        SceneLoader.Instance.LoadShop();
    }
}