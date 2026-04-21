using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace KawaiiKiller.Cinematics
{
    public class ComicManager : MonoBehaviour
    {
        public static ComicManager Instance { get; private set; }

        public event Action OnFinished;

        [Header("Configuration")]
        [SerializeField] private List<ComicPage> pages;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button skipButton;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float pageFadeDuration = 0.4f;

        private int activePage;
        private int activeVignette;
        private bool transitioning;
        private bool cancelled;
        private bool initialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            initialized = false;
        }

        private void Start()
        {
            continueButton?.onClick.AddListener(OnAdvance);
            skipButton?.onClick.AddListener(OnSkip);
        }

        private void OnDestroy()
        {
            continueButton?.onClick.RemoveListener(OnAdvance);
            skipButton?.onClick.RemoveListener(OnSkip);
        }

        private void OnAdvance()
        {
            if (cancelled || transitioning) return;

            if (IsAnimatingIn())
            {
                CancelAnimation();
                return;
            }

            Next();
        }

        private void OnSkip()
        {
            if (cancelled) return;
            cancelled = true;
            ForceFinish();
        }

        private bool IsAnimatingIn() => transitionTween != null && transitionTween.IsPlaying();

        private Tween transitionTween;

        private void Next()
        {
            ComicPage page = GetPage(activePage);
            if (page == null) { Finish(); return; }

            if (activeVignette < page.VignetteCount - 1)
            {
                activeVignette++;
                page.FadeIn(activeVignette, fadeDuration);
            }
            else if (activePage < pages.Count - 1)
            {
                transitioning = true;
                StartCoroutine(NextPage());
            }
            else
            {
                Finish();
            }
        }

        private IEnumerator NextPage()
        {
            ComicPage current = GetPage(activePage);
            current?.FadeOutAll(pageFadeDuration);

            yield return new WaitForSeconds(pageFadeDuration);

            current?.SetActive(false);
            activePage++;
            activeVignette = 0;
            transitioning = false;

            ComicPage next = GetPage(activePage);
            next?.SetActive(true);
            next?.Initialize();
            next?.FadeIn(0, fadeDuration);
        }

        private void CancelAnimation()
        {
            transitionTween?.Kill();
            transitionTween = null;

            ComicPage page = GetPage(activePage);
            page?.CompleteFadeIn(activeVignette);
        }

        private void ForceFinish()
        {
            DOTween.KillAll();

            foreach (var page in pages)
            {
                page?.ShowAll();
            }

            Finish();
        }

        private void Finish()
        {
            foreach (var page in pages)
            {
                page?.SetActive(false);
            }

            gameObject.SetActive(false);
            OnFinished?.Invoke();
        }

        private ComicPage GetPage(int index)
        {
            return (index >= 0 && index < pages.Count) ? pages[index] : null;
        }

        public void Play() => Play(null);

        public void Play(Action onComplete)
        {
            if (pages == null || pages.Count == 0)
            {
                OnFinished?.Invoke();
                return;
            }

            gameObject.SetActive(true);
            activePage = 0;
            activeVignette = 0;
            transitioning = false;
            cancelled = false;

            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i == 0);
                    if (i == 0) pages[i].Initialize();
                }
            }

            ComicPage first = GetPage(activePage);
            if (first != null && first.VignetteCount > 0)
            {
                first.FadeIn(0, fadeDuration);
            }
            else
            {
                Finish();
            }

            if (onComplete != null)
                OnFinished += onComplete;
        }

        public void PlayRound(int round, Action onComplete)
        {
            Play(onComplete);
        }
    }
}