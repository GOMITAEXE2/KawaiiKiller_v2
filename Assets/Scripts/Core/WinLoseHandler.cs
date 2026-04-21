using UnityEngine;
using KawaiiKiller.Cinematics;
using KawaiiKiller.UI;

public class WinLoseHandler : MonoBehaviour
{
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private RoundScorePanel scorePanel;

    private void OnEnable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnRoundEnded += HandleRoundEnd;
    }

    private void OnDisable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnRoundEnded -= HandleRoundEnd;
    }

    public void HandlePlayerDeath()
    {
        Time.timeScale = 0f;
        gameOverScreen?.Show();
    }

    private void HandleRoundEnd(int completedRound)
    {
        if (scorePanel != null && WaveManager.Instance != null)
        {
            scorePanel.Show(
                WaveManager.Instance.KillsByType,
                WaveManager.Instance.TotalRewardThisRound,
                completedRound,
                () => { scorePanel.gameObject.SetActive(false); RoundTransitionManager.Instance?.BeginTransition(completedRound); },
                () => { scorePanel.gameObject.SetActive(false); GameManager.Instance?.GoToMainMenu(); }
            );
        }
        else
        {
            RoundTransitionManager.Instance?.BeginTransition(completedRound);
        }
    }
}