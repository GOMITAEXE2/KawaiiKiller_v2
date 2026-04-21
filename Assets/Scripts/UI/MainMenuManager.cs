using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using KawaiiKiller.Core;
using KawaiiKiller.UI.Core;

namespace KawaiiKiller.UI.Menu
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private MenuCameraRig cameraRig;

        [Header("Panels")]
        [SerializeField] private CanvasGroup       mainPanel;
        [SerializeField] private CanvasGroup       optionsPanel;
        [SerializeField] private CanvasGroup       creditsPanel;
        [SerializeField] private ModeSelectPanelUI modeSelectPanel;
        [SerializeField] private ConfirmPopupUI    confirmPopup;

        [Header("Main Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Section Indices")]
        [SerializeField] private int mainSectionIndex      = 0;
        [SerializeField] private int modeSelectSectionIndex = 1;
        [SerializeField] private int optionsSectionIndex = 2;
        [SerializeField] private int creditsSectionIndex = 3;

        [Header("Animation")]
        [SerializeField] private float panelFadeDuration = 0.25f;

        private CanvasGroup _activePanel;

        private void Start()
        {
            cameraRig?.SnapToSection(mainSectionIndex);
            SetupButtons();
            ShowPanel(mainPanel, instant: true);
            RefreshContinueButton();
        }

        private void OnDestroy()
        {
            continueButton?.onClick.RemoveAllListeners();
            newGameButton?.onClick.RemoveAllListeners();
            optionsButton?.onClick.RemoveAllListeners();
            creditsButton?.onClick.RemoveAllListeners();
            quitButton?.onClick.RemoveAllListeners();
        }

        // ── Buttons ────────────────────────────────────────────────────────────────

        private void SetupButtons()
        {
            continueButton?.onClick.AddListener(OnContinue);
            newGameButton?.onClick.AddListener(OnNewGame);
            optionsButton?.onClick.AddListener(OnOptions);
            creditsButton?.onClick.AddListener(OnCredits);
            quitButton?.onClick.AddListener(OnQuit);
        }

        private void RefreshContinueButton()
        {
            bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasSave();
            if (continueButton != null) continueButton.interactable = hasSave;
        }

        private void OnContinue() => GameManager.Instance?.ContinueGame();

        private void OnNewGame()
        {
            bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasSave();
            if (hasSave)
            {
                confirmPopup?.Show(
                    "Nueva Partida",
                    "Se borrará la partida guardada. ¿Continuar?",
                    onConfirm: () => { confirmPopup?.Hide(); ShowModeSelect(); },
                    onCancel:  () => confirmPopup?.Hide(),
                    confirmText: "Borrar y continuar",
                    cancelText:  "Cancelar"
                );
            }
            else
            {
                ShowModeSelect();
            }
        }

        private void OnOptions()
        {
            cameraRig?.GoToSection(optionsSectionIndex);
            HidePanel(mainPanel);
            ShowPanel(optionsPanel);
        }

        private void OnCredits()
        {
            cameraRig?.GoToSection(creditsSectionIndex);
            HidePanel(mainPanel);
            ShowPanel(creditsPanel);
        }

        private void OnQuit()
        {
            confirmPopup?.Show(
                "Salir",
                "¿Salir del juego?",
                onConfirm: () => Application.Quit(),
                onCancel:  null,
                confirmText: "Salir",
                cancelText:  "Cancelar"
            );
        }

        public void GoBackToMain()
        {
            modeSelectPanel?.Hide();
            cameraRig?.GoToSection(mainSectionIndex);
            ShowPanel(mainPanel);
        }

        // ── Mode select ────────────────────────────────────────────────────────────

        private void ShowModeSelect()
        {
            cameraRig?.GoToSection(modeSelectSectionIndex);
            HidePanel(mainPanel);
            bool unlocked = SaveSystem.Instance != null && SaveSystem.Instance.IsEndlessUnlocked();
            modeSelectPanel?.Show(unlocked, OnModeSelected, GoBackToMain);
        }

        private void OnModeSelected(GameManager.GameMode mode)
        {
            modeSelectPanel?.Hide();
            _activePanel = mainPanel;
            SaveSystem.Instance?.ClearSave();
            GameManager.Instance?.StartNewGame(mode);
        }

        // ── Panel transitions ──────────────────────────────────────────────────────

        private void ShowPanel(CanvasGroup panel, bool instant = false)
        {
            if (_activePanel != null && _activePanel != panel)
                HidePanel(_activePanel, instant);

            if (panel == null) return;

            _activePanel         = panel;
            panel.gameObject.SetActive(true);

            if (instant)
            {
                panel.alpha          = 1f;
                panel.interactable   = true;
                panel.blocksRaycasts = true;
                return;
            }

            panel.alpha          = 0f;
            panel.interactable   = false;
            panel.blocksRaycasts = false;
            panel.DOFade(1f, panelFadeDuration)
                 .SetUpdate(true)
                 .OnComplete(() =>
                 {
                     panel.interactable   = true;
                     panel.blocksRaycasts = true;
                 });
        }

        private void HidePanel(CanvasGroup panel, bool instant = false)
        {
            if (panel == null) return;
            panel.interactable   = false;
            panel.blocksRaycasts = false;

            if (instant)
            {
                panel.alpha = 0f;
                panel.gameObject.SetActive(false);
                return;
            }

            panel.DOFade(0f, panelFadeDuration * 0.6f)
                 .SetUpdate(true)
                 .OnComplete(() => panel.gameObject.SetActive(false));
        }
    }
}