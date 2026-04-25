using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace KawaiiKiller.Cinematics
{
    [Serializable]
    public class RoundComic
    {
        public int roundNumber;
        public List<ComicPage> pages = new List<ComicPage>();
    }

    public class ComicManager : MonoBehaviour
    {
        public static ComicManager Instance { get; private set; }
        public static bool IsPlaying { get; private set; }

        public event Action OnFinished;

        [Header("Configuration")]
        [SerializeField] private List<RoundComic> roundComics = new List<RoundComic>();
        [SerializeField] private List<ComicPage> introPages = new List<ComicPage>();
        [SerializeField] private Button continueButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private CanvasGroup mainCanvasGroup;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float pageFadeDuration = 0.4f;

        [Header("Intro Only Mode")]
        [SerializeField] private bool useIntroOnly = false;

        private List<ComicPage> _activePages;
        private int activePageIndex;
        private int activeVignetteIndex;
        private bool transitioning;
        private bool cancelled;
        private bool _hasStarted;
        private Tween transitionTween;

        [Header("Debug")]
        [SerializeField] private bool hideOnAwake = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Critical Repair: Auto-population if Inspector is empty
            if (introPages == null || introPages.Count == 0)
            {
                introPages = GetComponentsInChildren<ComicPage>(true).ToList();
            }

            // Final validation with aggressive feedback to designer
            if ((introPages == null || introPages.Count == 0) && (roundComics == null || roundComics.Count == 0))
            {
                Debug.LogError("‼️ ERROR CRÍTICO: El ComicManager no tiene páginas asignadas ni en el Inspector ni en la Jerarquía.");
            }

            // Ensure initial visibility state
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = 1f;
            }
        }

        private void OnEnable()
        {
            continueButton?.onClick.AddListener(OnAdvance);
            skipButton?.onClick.AddListener(OnSkip);
        }

        private void OnDisable()
        {
            continueButton?.onClick.RemoveListener(OnAdvance);
            skipButton?.onClick.RemoveListener(OnSkip);
        }
        
        private void OnDestroy()
        {
            continueButton?.onClick.RemoveListener(OnAdvance);
            skipButton?.onClick.RemoveListener(OnSkip);
        }

        private void OnAdvance()
        {
            Debug.Log("[ComicManager] OnAdvance called");
            if (cancelled || transitioning) { Debug.Log("[ComicManager] Cancelled or transitioning"); return; }
            if (IsAnimatingIn()) { Debug.Log("[ComicManager] Animating, cancelling"); CancelAnimation(); return; }
            Debug.Log("[ComicManager] Calling Next()");
            Next();
        }

        private void OnSkip()
        {
            if (cancelled) return;
            cancelled = true;
            ForceFinish();
        }

        private bool IsAnimatingIn() => transitionTween != null && transitionTween.IsPlaying();

        private void Next()
        {
            // Fallback: If _activePages is null (manual activation), try to use introPages
            if (_activePages == null || _activePages.Count == 0)
            {
                Debug.Log("[ComicManager] _activePages is null/empty. Attempting manual fallback to introPages.");
                _activePages = introPages;
                _hasStarted = false; // Reset for manual start
            }

            if (_activePages == null || _activePages.Count == 0) 
            { 
                Debug.LogWarning("[ComicManager] Next() called but no pages are available. Finishing.");
                Finish(); 
                return; 
            }

            ComicPage page = GetPage(activePageIndex);
            if (page == null) 
            { 
                Debug.LogError($"[ComicManager] Next() called but page at index {activePageIndex} is null. Finishing.");
                Finish(); 
                return; 
            }

            // Logic Fix: Handle the first vignette differently if it hasn't been shown yet
            if (!_hasStarted)
            {
                _hasStarted = true;
                activeVignetteIndex = 0;
                Debug.Log($"[ComicManager] Starting sequence from vignette 0 of page {activePageIndex}");
                transitionTween = page.FadeIn(activeVignetteIndex, fadeDuration);
                return;
            }

            if (activeVignetteIndex < page.VignetteCount - 1)
            {
                activeVignetteIndex++;
                Debug.Log($"[ComicManager] Advancing to vignette {activeVignetteIndex} of page {activePageIndex}");
                transitionTween = page.FadeIn(activeVignetteIndex, fadeDuration);
            }
            else if (activePageIndex < _activePages.Count - 1)
            {
                Debug.Log($"[ComicManager] Page {activePageIndex} finished. Advancing to next page.");
                transitioning = true;
                StartCoroutine(NextPage());
            }
            else
            {
                Debug.Log("[ComicManager] Last vignette of last page reached. Finishing comic.");
                Finish();
            }
        }

        private IEnumerator NextPage()
        {
            ComicPage current = GetPage(activePageIndex);
            current?.FadeOutAll(pageFadeDuration);

            yield return new WaitForSeconds(pageFadeDuration);

            current?.SetActive(false);
            activePageIndex++;
            activeVignetteIndex = 0;
            
            ComicPage next = GetPage(activePageIndex);
            if (next != null)
            {
                next.SetActive(true);
                next.Initialize();
                transitionTween = next.FadeIn(0, fadeDuration);
                Debug.Log($"[ComicManager] Page {activePageIndex} started.");
            }
            else
            {
                Debug.LogWarning($"[ComicManager] Failed to find next page at index {activePageIndex}. Finishing.");
                Finish();
            }

            transitioning = false;
        }

        private void CancelAnimation()
        {
            transitionTween?.Kill();
            transitionTween = null;
            ComicPage page = GetPage(activePageIndex);
            page?.CompleteFadeIn(activeVignetteIndex);
        }

        private void ForceFinish()
        {
            DOTween.KillAll();
            if (_activePages != null)
            {
                foreach (var page in _activePages)
                    page?.ShowAll();
            }
            Finish();
        }

        private void Finish()
        {
            IsPlaying = false;
            _hasStarted = false;
            if (_activePages != null)
            {
                foreach (var page in _activePages)
                    page?.SetActive(false);
            }
            gameObject.SetActive(false);
            OnFinished?.Invoke();
        }

        private ComicPage GetPage(int index)
        {
            if (_activePages == null || index < 0 || index >= _activePages.Count) return null;
            return _activePages[index];
        }

        public void Play() => Play(null);

        public void Play(Action onComplete)
        {
            _activePages = introPages.Count > 0 ? introPages : GetPagesForRound(1);
            InitializePlay(onComplete);
        }

        public void PlayRound(int round, Action onComplete)
        {
            if (useIntroOnly)
            {
                _activePages = introPages.Count > 0 ? introPages : GetPagesForRound(round);
            }
            else
            {
                _activePages = GetPagesForRound(round);
            }
            InitializePlay(onComplete);
        }

        private void InitializePlay(Action onComplete)
        {
            Debug.Log($"[ComicManager] InitializePlay called, _activePages count: {_activePages?.Count ?? 0}");

            if (_activePages == null || _activePages.Count == 0)
            {
                Debug.LogWarning("[ComicManager] No pages to show, finishing immediately");
                OnFinished?.Invoke();
                return;
            }

            IsPlaying = true;
            gameObject.SetActive(true);

            Debug.Log($"[ComicManager] ContinueButton: {continueButton?.name}, SkipButton: {skipButton?.name}");

            if (continueButton != null)
            {
                continueButton.interactable = true;
                Debug.Log("[ComicManager] ContinueButton enabled");
            }
            else
            {
                Debug.LogWarning("[ComicManager] ContinueButton is NULL!");
            }

            if (skipButton != null) skipButton.interactable = true;

            activePageIndex = 0;
            activeVignetteIndex = 0;
            transitioning = false;
            cancelled = false;
            _hasStarted = true; // Mark as started since InitializePlay shows first vignette

            for (int i = 0; i < _activePages.Count; i++)
            {
                if (_activePages[i] != null)
                {
                    _activePages[i].SetActive(i == 0);
                    if (i == 0) _activePages[i].Initialize();
                }
            }

            ComicPage first = GetPage(0);
            if (first != null && first.VignetteCount > 0)
            {
                Debug.Log($"[ComicManager] Fading in first vignette, vignette count: {first.VignetteCount}");
                transitionTween = first.FadeIn(0, fadeDuration);
            }
            else
            {
                Debug.LogWarning("[ComicManager] No vignettes to show in first page");
                Finish();
            }

            if (onComplete != null)
                OnFinished += onComplete;
        }

        private List<ComicPage> GetPagesForRound(int round)
        {
            var roundComic = roundComics?.FirstOrDefault(rc => rc.roundNumber == round);
            return roundComic?.pages ?? new List<ComicPage>();
        }

        public bool HasComicContent()
        {
            // Ensure lists are populated if they were empty in Awake but later filled or need to be checked
            if (introPages == null || introPages.Count == 0)
            {
                introPages = GetComponentsInChildren<ComicPage>(true).ToList();
            }

            bool hasIntro = introPages != null && introPages.Count > 0 && introPages.Any(p => p != null && p.VignetteCount > 0);
            
            // For roundComics, we check if any round has pages
            bool hasRound = roundComics != null && roundComics.Any(rc => rc.pages != null && rc.pages.Any(p => p != null && p.VignetteCount > 0));
            
            Debug.Log($"[ComicManager] Checking content: hasIntro={hasIntro}, hasRound={hasRound}");
            return hasIntro || hasRound;
        }

        public bool HasPagesForRound(int round)
        {
            return GetPagesForRound(round).Count > 0;
        }

        public int PageCount => _activePages?.Count ?? 0;
    }
}