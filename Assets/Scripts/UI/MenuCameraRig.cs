using DG.Tweening;
using UnityEngine;

namespace KawaiiKiller.UI.Menu
{
    public class MenuCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform   cameraTransform;
        [SerializeField] private Transform[] sectionAnchors;
        [SerializeField] private float       moveDuration = 0.8f;
        [SerializeField] private Ease        moveEase     = Ease.InOutCubic;

        private Sequence _seq;
        private int      _currentSection = -1;

        private void Awake()
        {
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void OnDestroy() => _seq?.Kill();

        public void GoToSection(int index, TweenCallback onComplete = null)
        {
            if (cameraTransform == null) return;
            if (index < 0 || index >= sectionAnchors.Length) return;
            if (index == _currentSection) { onComplete?.Invoke(); return; }

            _currentSection = index;
            Transform anchor = sectionAnchors[index];

            _seq?.Kill();
            _seq = DOTween.Sequence().SetUpdate(true);
            _seq.Join(cameraTransform.DOMove(anchor.position, moveDuration).SetEase(moveEase));
            _seq.Join(cameraTransform.DORotateQuaternion(anchor.rotation, moveDuration).SetEase(moveEase));
            if (onComplete != null) _seq.OnComplete(onComplete);
        }

        public void SnapToSection(int index)
        {
            if (cameraTransform == null) return;
            if (index < 0 || index >= sectionAnchors.Length) return;

            _seq?.Kill();
            _currentSection          = index;
            Transform anchor         = sectionAnchors[index];
            cameraTransform.position = anchor.position;
            cameraTransform.rotation = anchor.rotation;
        }
    }
}