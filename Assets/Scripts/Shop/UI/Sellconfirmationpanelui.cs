using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        private Coroutine _animRoutine;

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

            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _animRoutine = StartCoroutine(PopIn());
        }

        public void Hide(bool instant = false)
        {
            if (_animRoutine != null) { StopCoroutine(_animRoutine); _animRoutine = null; }

            if (instant || !gameObject.activeInHierarchy)
            {
                ApplyHiddenState();
                return;
            }

            _animRoutine = StartCoroutine(PopOut());
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

        private IEnumerator PopIn()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (panelRect   != null) panelRect.localScale = Vector3.zero;

            float elapsed  = 0f;
            float duration = Mathf.Max(0.01f, popDuration);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t  = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                if (panelRect   != null) panelRect.localScale = Vector3.one * ease;
                if (canvasGroup != null) canvasGroup.alpha    = ease;
                yield return null;
            }
            if (panelRect   != null) panelRect.localScale = Vector3.one;
            if (canvasGroup != null) canvasGroup.alpha    = 1f;
            _animRoutine = null;
        }

        private IEnumerator PopOut()
        {
            float elapsed  = 0f;
            float duration = Mathf.Max(0.01f, popDuration * 0.6f);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t  = Mathf.Clamp01(elapsed / duration);
                if (panelRect   != null) panelRect.localScale = Vector3.one * (1f - t);
                if (canvasGroup != null) canvasGroup.alpha    = 1f - t;
                yield return null;
            }
            ApplyHiddenState();
            _animRoutine = null;
        }
    }
}