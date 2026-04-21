using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KawaiiKiller.UI.Menu
{
    public class ModeSelectPanelUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup    canvasGroup;
        [SerializeField] private RectTransform  panelRect;
        [SerializeField] private Button         storyButton;
        [SerializeField] private Button         endlessButton;
        [SerializeField] private GameObject     endlessLockedOverlay;
        [SerializeField] private TMP_Text       endlessLockedText;
        [SerializeField] private Button         backButton;
        [SerializeField] private float          popDuration = 0.2f;

        private Action<GameManager.GameMode> _onSelect;
        private Action                       _onBack;
        private Sequence                     _seq;

        private void Awake()
        {
            ApplyHiddenState();
            storyButton?.onClick.AddListener(OnStory);
            endlessButton?.onClick.AddListener(OnEndless);
            backButton?.onClick.AddListener(OnBack);
        }

        private void OnDestroy()
        {
            _seq?.Kill();
            storyButton?.onClick.RemoveListener(OnStory);
            endlessButton?.onClick.RemoveListener(OnEndless);
            backButton?.onClick.RemoveListener(OnBack);
        }

        public void Show(bool endlessUnlocked, Action<GameManager.GameMode> onSelect, Action onBack)
        {
            _onSelect = onSelect;
            _onBack   = onBack;

            if (endlessLockedOverlay != null) endlessLockedOverlay.SetActive(!endlessUnlocked);
            if (endlessButton        != null) endlessButton.interactable = endlessUnlocked;

            gameObject.SetActive(true);
            panelRect.localScale = Vector3.zero;
            canvasGroup.alpha    = 0f;

            _seq?.Kill();
            _seq = DOTween.Sequence().SetUpdate(true);
            _seq.Join(panelRect.DOScale(Vector3.one, popDuration).SetEase(Ease.OutBack));
            _seq.Join(canvasGroup.DOFade(1f, popDuration));
        }

        public void Hide()
        {
            _seq?.Kill();
            _seq = DOTween.Sequence().SetUpdate(true);
            _seq.Join(panelRect.DOScale(Vector3.zero, popDuration * 0.6f).SetEase(Ease.InBack));
            _seq.Join(canvasGroup.DOFade(0f, popDuration * 0.6f));
            _seq.OnComplete(ApplyHiddenState);
        }

        private void OnStory()   => _onSelect?.Invoke(GameManager.GameMode.Story);
        private void OnEndless() => _onSelect?.Invoke(GameManager.GameMode.Endless);
        private void OnBack()    => _onBack?.Invoke();

        private void ApplyHiddenState()
        {
            gameObject.SetActive(false);
            if (canvasGroup != null) canvasGroup.alpha    = 0f;
            if (panelRect   != null) panelRect.localScale = Vector3.zero;
        }
    }
}