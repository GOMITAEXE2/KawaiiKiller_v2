using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KawaiiKiller.UI.Core
{
    public class ConfirmPopupUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup    canvasGroup;
        [SerializeField] private RectTransform  panelRect;
        [SerializeField] private TMP_Text       titleText;
        [SerializeField] private TMP_Text       messageText;
        [SerializeField] private Button         confirmButton;
        [SerializeField] private Button         cancelButton;
        [SerializeField] private TMP_Text       confirmLabel;
        [SerializeField] private TMP_Text       cancelLabel;
        [SerializeField] private float          popDuration = 0.18f;

        private Action  _onConfirm;
        private Action  _onCancel;
        private Sequence _seq;

        private void Awake()
        {
            ApplyHiddenState();
            confirmButton?.onClick.AddListener(OnConfirm);
            cancelButton?.onClick.AddListener(OnCancel);
        }

        private void OnDestroy()
        {
            _seq?.Kill();
            confirmButton?.onClick.RemoveListener(OnConfirm);
            cancelButton?.onClick.RemoveListener(OnCancel);
        }

        public void Show(string title, string message, Action onConfirm, Action onCancel,
            string confirmText = "Confirmar", string cancelText = "Cancelar")
        {
            _onConfirm = onConfirm;
            _onCancel  = onCancel;

            if (titleText    != null) titleText.text    = title;
            if (messageText  != null) messageText.text  = message;
            if (confirmLabel != null) confirmLabel.text = confirmText;
            if (cancelLabel  != null) cancelLabel.text  = cancelText;

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

        private void OnConfirm()
        {
            var cb = _onConfirm;
            _onConfirm = null;
            _onCancel  = null;
            Hide();
            cb?.Invoke();
        }

        private void OnCancel()
        {
            var cb = _onCancel;
            _onConfirm = null;
            _onCancel  = null;
            Hide();
            cb?.Invoke();
        }

        private void ApplyHiddenState()
        {
            gameObject.SetActive(false);
            if (canvasGroup != null) canvasGroup.alpha        = 0f;
            if (panelRect   != null) panelRect.localScale     = Vector3.zero;
        }
    }
}