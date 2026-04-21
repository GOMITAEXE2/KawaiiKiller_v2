using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KawaiiKiller.Cinematics
{
    public class ComicPanelSystem : MonoBehaviour
    {
        [System.Serializable]
        public class ComicPanel
        {
            public Sprite   image;
            [TextArea(2, 4)]
            public string   caption;
            public float    displayDuration = 3f;
        }

        [Header("References")]
        [SerializeField] private CanvasGroup    canvasGroup;
        [SerializeField] private Image          panelImage;
        [SerializeField] private Image          panelBackground;
        [SerializeField] private TMP_Text       captionText;
        [SerializeField] private Button         continueButton;
        [SerializeField] private Button         skipButton;

        [Header("Panels")]
        [SerializeField] private List<ComicPanel> panels;

        [Header("Transition")]
        [SerializeField] private float fadeDuration = 0.4f;

        private int          _currentIndex;
        private bool         _skipped;
        private bool         _advanceRequested;
        private Action       _onComplete;
        private List<ComicPanel> _currentPanels;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);

            continueButton?.onClick.AddListener(OnContinueClicked);
            skipButton?.onClick.AddListener(Skip);
        }

        private void OnDestroy()
        {
            continueButton?.onClick.RemoveListener(OnContinueClicked);
            skipButton?.onClick.RemoveListener(Skip);
        }

        private void OnContinueClicked()
        {
            _advanceRequested = true;
        }

        public IEnumerator Play(List<ComicPanel> overridePanels = null, Action onComplete = null)
        {
            _currentPanels = overridePanels ?? panels;
            if (_currentPanels == null || _currentPanels.Count == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            _skipped = false;
            _advanceRequested = false;
            _currentIndex = 0;
            _onComplete = onComplete;

            gameObject.SetActive(true);

            yield return StartCoroutine(FadeIn());

            while (_currentIndex < _currentPanels.Count && !_skipped)
            {
                yield return StartCoroutine(ShowCurrentPanel());
                _currentIndex++;
            }

            yield return StartCoroutine(FadeOut());
            gameObject.SetActive(false);

            _onComplete?.Invoke();
            _onComplete = null;
        }

        private IEnumerator ShowCurrentPanel()
        {
            ComicPanel panel = _currentPanels[_currentIndex];

            panelImage.sprite = panel.image;
            captionText.text = panel.caption;

            if (panelBackground != null)
                panelBackground.sprite = panel.image;

            _advanceRequested = false;

            float elapsed = 0f;
            while (elapsed < panel.displayDuration && !_advanceRequested && !_skipped)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                if (panelImage != null)
                {
                    Color c = panelImage.color;
                    c.a = Mathf.Lerp(0f, 1f, t);
                    panelImage.color = c;
                }
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                if (panelImage != null)
                {
                    Color c = panelImage.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    panelImage.color = c;
                }
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        private void Skip()
        {
            _skipped = true;
        }
    }
}