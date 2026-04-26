using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace KawaiiKiller.UI.Shop
{
    public class SellConfirmationPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup  canvasGroup;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private TMP_Text     itemListText;
        [SerializeField] private TMP_Text     totalValueText;
        [SerializeField] private Button       confirmButton;
        [SerializeField] private Button       cancelButton;

        [Header("Blocker")]
        [SerializeField] private GameObject   inputBlocker;

        [Header("Animation")]
        [SerializeField] private float        popDuration = 0.18f;

        private Action    _onConfirm;
        private Action    _onCancel;
        private Tween     _animTween;

        private void Awake()
        {
            Hide(true);
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
            if (cancelButton  != null) cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnDestroy()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirmClicked);
            if (cancelButton  != null) cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        public void Show(List<string> itemNames, int totalValue, Action onConfirm, Action onCancel)
        {
            _onConfirm = onConfirm;
            _onCancel  = onCancel;

            if (itemListText != null)
                itemListText.text = itemNames != null && itemNames.Count > 0
                    ? string.Join("\n", itemNames)
                    : "—";

            if (totalValueText != null)
                totalValueText.text = totalValue.ToString();

            if (inputBlocker != null) inputBlocker.SetActive(true);
            gameObject.SetActive(true);

            _animTween?.Kill();
            
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (panelRect   != null) panelRect.localScale = Vector3.zero;

            _animTween = DOTween.Sequence()
                .SetUpdate(true)
                .Join(canvasGroup.DOFade(1f, popDuration))
                .Join(panelRect.DOScale(Vector3.one, popDuration).SetEase(Ease.OutBack));
        }

        public void Hide(bool instant = false)
        {
            _animTween?.Kill();

            if (instant || !gameObject.activeInHierarchy)
            {
                ApplyHiddenState();
                return;
            }

            _animTween = DOTween.Sequence()
                .SetUpdate(true)
                .Join(canvasGroup.DOFade(0f, popDuration * 0.6f))
                .Join(panelRect.DOScale(Vector3.zero, popDuration * 0.6f).SetEase(Ease.InBack))
                .OnComplete(ApplyHiddenState);
        }

        private void OnConfirmClicked()
        {
            Action callback = _onConfirm;
            _onConfirm = null;
            _onCancel  = null;
            Hide();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            Action callback = _onCancel;
            _onConfirm = null;
            _onCancel  = null;
            Hide();
            callback?.Invoke();
        }

        private void ApplyHiddenState()
        {
            gameObject.SetActive(false);
            if (inputBlocker != null) inputBlocker.SetActive(false);
            if (canvasGroup  != null) canvasGroup.alpha = 0f;
            if (panelRect    != null) panelRect.localScale = Vector3.zero;
        }
    }
}