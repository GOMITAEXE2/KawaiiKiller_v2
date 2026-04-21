using UnityEngine;
using DG.Tweening;

namespace KawaiiKiller.UI.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class UIPanelAnimator : MonoBehaviour
    {
        public enum AnimationType { Slide, Pop, SlideFromTop }

        [SerializeField] private AnimationType animationType  = AnimationType.Slide;
        [SerializeField] private Vector2       offScreenPosition;
        [SerializeField] private Vector2       onScreenPosition;
        [SerializeField] private float         animationDuration = 0.3f;
        [SerializeField] private Ease          easeIn            = Ease.OutBack;
        [SerializeField] private Ease          easeOut           = Ease.InBack;

        private CanvasGroup   _canvasGroup;
        private RectTransform _rectTransform;
        private Sequence      _sequence;

        private void Awake()
        {
            _canvasGroup   = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void ShowPanel()
        {
            _sequence?.Kill();
            gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha          = 0f;

            _sequence = DOTween.Sequence().SetUpdate(true);

            switch (animationType)
            {
                case AnimationType.Pop:
                    _rectTransform.localScale = Vector3.one * 0.85f;
                    _sequence.Join(_rectTransform.DOScale(Vector3.one, animationDuration).SetEase(easeIn));
                    break;

                case AnimationType.SlideFromTop:
                    _rectTransform.anchoredPosition = offScreenPosition;
                    _sequence.Join(_rectTransform.DOAnchorPos(onScreenPosition, animationDuration).SetEase(easeIn));
                    break;

                default:
                    _rectTransform.anchoredPosition = offScreenPosition;
                    _sequence.Join(_rectTransform.DOAnchorPos(onScreenPosition, animationDuration).SetEase(easeIn));
                    break;
            }

            _sequence.Join(_canvasGroup.DOFade(1f, animationDuration));
            _sequence.OnComplete(() => _canvasGroup.blocksRaycasts = true);
        }

        public void HidePanel()
        {
            _sequence?.Kill();
            _canvasGroup.blocksRaycasts = false;

            _sequence = DOTween.Sequence().SetUpdate(true);

            switch (animationType)
            {
                case AnimationType.Pop:
                    _sequence.Join(_rectTransform.DOScale(Vector3.one * 0.85f, animationDuration * 0.6f).SetEase(easeOut));
                    break;

                default:
                    _sequence.Join(_rectTransform.DOAnchorPos(offScreenPosition, animationDuration * 0.6f).SetEase(easeOut));
                    break;
            }

            _sequence.Join(_canvasGroup.DOFade(0f, animationDuration * 0.6f));
            _sequence.OnComplete(() => gameObject.SetActive(false));
        }

        private void OnDestroy() => _sequence?.Kill();
    }
}
