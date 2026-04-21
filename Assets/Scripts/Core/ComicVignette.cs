using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

namespace KawaiiKiller.Cinematics
{
    public class ComicVignette : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image image;

        [Header("Dialogue")]
        [SerializeField] private bool hasDialogue;
        [SerializeField] private TextMeshProUGUI dialogueText;

        private Tween currentTween;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (image == null)
                image = GetComponent<Image>();
        }

        public void Reset()
        {
            currentTween?.Kill();
            currentTween = null;

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        public void Show()
        {
            currentTween?.Kill();

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        public Tween FadeIn(float duration)
        {
            currentTween?.Kill();

            if (canvasGroup != null)
            {
                currentTween = canvasGroup.DOFade(1f, duration)
                    .SetUpdate(true);
                return currentTween;
            }
            return null;
        }

        public void CompleteFadeIn()
        {
            currentTween?.Kill();
            currentTween = null;

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        public Tween FadeOut(float duration)
        {
            currentTween?.Kill();

            if (canvasGroup != null)
            {
                currentTween = canvasGroup.DOFade(0f, duration)
                    .SetUpdate(true);
                return currentTween;
            }
            return null;
        }
    }
}