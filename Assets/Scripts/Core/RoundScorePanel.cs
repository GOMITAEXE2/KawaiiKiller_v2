using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace KawaiiKiller.UI
{
    public class RoundScorePanel : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRect;

        [Header("Grade")]
        [SerializeField] private TMP_Text gradeText;

        [Header("Kill Counts")]
        [SerializeField] private TMP_Text smallKillsText;
        [SerializeField] private TMP_Text mediumKillsText;
        [SerializeField] private TMP_Text largeKillsText;
        [SerializeField] private TMP_Text eliteKillsText;
        [SerializeField] private TMP_Text bossKillsText;
        [SerializeField] private TMP_Text totalKillsText;

        [Header("Reward")]
        [SerializeField] private TMP_Text totalPaidText;

        [Header("Extra Stats")]
        [SerializeField] private TMP_Text totalDamageText;
        [SerializeField] private TMP_Text maxHitText;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button saveAndMenuButton;

        private Action _onContinue;
        private Action _onSaveAndMenu;

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
            if (saveAndMenuButton != null)
                saveAndMenuButton.onClick.AddListener(OnSaveAndMenuClicked);
        }

        public void Show(
            IReadOnlyDictionary<string, int> killsByType,
            int totalReward,
            float totalDamage,
            float maxHit,
            int round,
            Action onContinue,
            Action onSaveAndMenu)
        {
            _onContinue = onContinue;
            _onSaveAndMenu = onSaveAndMenu;

            int small = killsByType.ContainsKey("Small") ? killsByType["Small"] : 0;
            int medium = killsByType.ContainsKey("Medium") ? killsByType["Medium"] : 0;
            int large = killsByType.ContainsKey("Large") ? killsByType["Large"] : 0;
            int elite = killsByType.ContainsKey("Elite") ? killsByType["Elite"] : 0;
            int boss = killsByType.ContainsKey("Boss") ? killsByType["Boss"] : 0;
            int total = small + medium + large + elite + boss;

            if (smallKillsText != null) smallKillsText.text = small.ToString();
            if (mediumKillsText != null) mediumKillsText.text = medium.ToString();
            if (largeKillsText != null) largeKillsText.text = large.ToString();
            if (eliteKillsText != null) eliteKillsText.text = elite.ToString();
            if (bossKillsText != null) bossKillsText.text = boss.ToString();
            if (totalKillsText != null) totalKillsText.text = total.ToString();

            if (totalPaidText != null) totalPaidText.text = $"{totalReward} G";
            
            if (totalDamageText != null) totalDamageText.text = totalDamage.ToString("N0");
            if (maxHitText != null) maxHitText.text = maxHit.ToString("N1");

            string grade = CalculateGrade(total, round);
            if (gradeText != null) gradeText.text = grade;

            gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;

            panelRect.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;

            panelRect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            canvasGroup.DOFade(1f, 0.3f);
        }

        private string CalculateGrade(int totalKills, int round)
        {
            int expectedBase = WaveManager.Instance != null
                ? WaveManager.Instance.TotalThisRound / 2
                : 5;

            float ratio = (float)totalKills / expectedBase;

            if (ratio >= 1.5f) return "S+";
            if (ratio >= 1.25f) return "S";
            if (ratio >= 1.0f) return "A";
            if (ratio >= 0.8f) return "B";
            if (ratio >= 0.6f) return "C";
            if (ratio >= 0.4f) return "D";
            return "F";
        }

        public void OnContinueClicked()
        {
            _onContinue?.Invoke();
        }

        public void OnSaveAndMenuClicked()
        {
            _onSaveAndMenu?.Invoke();
        }
    }
}