using UnityEngine;
using KawaiiKiller.UI;

namespace KawaiiKiller.Core
{
    public class WinLoseHandler : MonoBehaviour
    {
        [SerializeField] private GameOverScreen gameOverScreen;
        [SerializeField] private RoundScorePanel scorePanel;

        private void Start()
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
            Debug.Log($"[WinLoseHandler] HandleRoundEnd received for round {completedRound}");
            if (scorePanel != null && WaveManager.Instance != null)
            {
                scorePanel.Show(
                    WaveManager.Instance.KillsByType,
                    WaveManager.Instance.TotalRewardThisRound,
                    WaveManager.Instance.TotalDamageDealt,
                    WaveManager.Instance.MaxDamageInSingleHit,
                    completedRound,
                    OnContinueFromScore,
                    () => { GameLoopOrchestrator.Instance?.CompleteRound(); scorePanel.gameObject.SetActive(false); GameManager.Instance?.GoToMainMenu(); }
                );
            }
            else
            {
                OnContinueFromScore();
            }
        }

        private void OnContinueFromScore()
        {
            scorePanel?.gameObject.SetActive(false);

            if (GameLoopOrchestrator.Instance != null)
            {
                GameLoopOrchestrator.Instance.CompleteRound();
                GameLoopOrchestrator.Instance.GoToShop();
            }
        }
    }
}