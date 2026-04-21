using System.Collections;
using TMPro;
using UnityEngine;

namespace KawaiiKiller.UI
{
    public class InteractPromptUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text    promptText;
        [SerializeField] private float       fadeDuration = 0.2f;

        private Coroutine _fadeRoutine;

        private void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha          = 0f;
                canvasGroup.interactable   = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        public void Show(string message)
        {
            if (promptText != null) promptText.text = message;
            Fade(1f);
        }

        public void Hide()
        {
            Fade(0f);
        }

        private void Fade(float target)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            if (!gameObject.activeInHierarchy)
            {
                ApplyAlpha(target);
                return;
            }
            _fadeRoutine = StartCoroutine(FadeRoutine(target));
        }

        private IEnumerator FadeRoutine(float target)
        {
            if (canvasGroup == null) yield break;
            float start   = canvasGroup.alpha;
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, fadeDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            ApplyAlpha(target);
            _fadeRoutine = null;
        }

        private void ApplyAlpha(float alpha)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha          = alpha;
            canvasGroup.interactable   = alpha > 0f;
            canvasGroup.blocksRaycasts = alpha > 0f;
        }
    }
}