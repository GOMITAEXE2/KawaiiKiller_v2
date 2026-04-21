using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KawaiiKiller.UI.Core
{
    public class UIScreenStackManager : MonoBehaviour
    {
        public static UIScreenStackManager Instance { get; private set; }

        [SerializeField] private Image blockerImage;
        [SerializeField] private float blockerAlpha = 0.6f;

        private readonly Stack<GameObject> _screenStack = new Stack<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (blockerImage != null)
            {
                Color c = Color.black;
                c.a = 0f;
                blockerImage.color   = c;
                blockerImage.enabled = false;
            }
        }

        public void PushScreen(GameObject newScreen)
        {
            if (newScreen == null) return;

            if (_screenStack.Count > 0 && blockerImage != null)
            {
                blockerImage.enabled = true;
                Color c = blockerImage.color;
                c.a = blockerAlpha;
                blockerImage.color = c;
                blockerImage.transform.SetSiblingIndex(newScreen.transform.GetSiblingIndex() - 1);
            }

            _screenStack.Push(newScreen);

            if (newScreen.TryGetComponent<UIPanelAnimator>(out var animator))
                animator.ShowPanel();
            else
                newScreen.SetActive(true);
        }

        public void PopScreen()
        {
            if (_screenStack.Count == 0) return;

            GameObject current = _screenStack.Pop();

            if (current.TryGetComponent<UIPanelAnimator>(out var animator))
                animator.HidePanel();
            else
                current.SetActive(false);

            if (_screenStack.Count == 0 && blockerImage != null)
            {
                blockerImage.enabled = false;
                Color c = blockerImage.color;
                c.a = 0f;
                blockerImage.color = c;
            }
        }

        public void PopAll()
        {
            while (_screenStack.Count > 0)
                PopScreen();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
