using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KawaiiKiller.UI.Shop
{
    public class SellZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image  backgroundImage;
        [SerializeField] private Color  normalColor   = new Color(0.20f, 0.20f, 0.20f, 0.60f);
        [SerializeField] private Color  hoverColor    = new Color(0.85f, 0.30f, 0.30f, 0.80f);

        private ShopUIManager _manager;

        public void Bind(ShopUIManager manager)
        {
            _manager = manager;
            SetVisual(normalColor);
        }

        public void OnDrop(PointerEventData eventData)
        {
            SetVisual(normalColor);
            if (_manager == null || eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent<ItemCardUI>(out var card)) return;
            _manager.TryRequestSell(card);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ItemCardUI.IsAnyDragging) SetVisual(hoverColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetVisual(normalColor);
        }

        private void SetVisual(Color color)
        {
            if (backgroundImage != null) backgroundImage.color = color;
        }
    }
}