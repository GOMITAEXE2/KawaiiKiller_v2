using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace KawaiiKiller.UI.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Button))]
    public class UIButtonReactive : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler,  IPointerUpHandler
    {
        [SerializeField] private UIThemeSO theme;
        [SerializeField] private float hoverScale    = 1.05f;
        [SerializeField] private float clickScale    = 0.95f;
        [SerializeField] private float tweenDuration = 0.15f;

        private Vector3 _originalScale;
        private Image   _image;

        private void Awake()
        {
            _originalScale = transform.localScale;
            TryGetComponent(out _image);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            transform.DOKill();
            transform.DOScale(_originalScale * hoverScale, tweenDuration)
                     .SetEase(Ease.OutBack)
                     .SetUpdate(true);

            if (_image != null && theme != null)
                _image.DOColor(theme.HoverInteractable, tweenDuration).SetUpdate(true);
        }

        public void OnPointerExit(PointerEventData e)
        {
            transform.DOKill();
            transform.DOScale(_originalScale, tweenDuration)
                     .SetEase(Ease.OutSine)
                     .SetUpdate(true);

            if (_image != null && theme != null)
                _image.DOColor(theme.PrimaryInteractable, tweenDuration).SetUpdate(true);
        }

        public void OnPointerDown(PointerEventData e)
        {
            transform.DOKill();
            transform.DOScale(_originalScale * clickScale, tweenDuration * 0.5f)
                     .SetEase(Ease.OutSine)
                     .SetUpdate(true);

            if (_image != null && theme != null)
                _image.DOColor(theme.ClickInteractable, tweenDuration * 0.5f).SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData e)
        {
            transform.DOKill();
            transform.DOScale(_originalScale * hoverScale, tweenDuration)
                     .SetEase(Ease.OutBack)
                     .SetUpdate(true);

            if (_image != null && theme != null)
                _image.DOColor(theme.HoverInteractable, tweenDuration).SetUpdate(true);
        }

        private void OnDestroy() => transform.DOKill();
    }
}
