using KawaiiKiller.Modifiers;
using KawaiiKiller.Weapons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KawaiiKiller.UI.Shop
{
    public enum ShopDropZonePurpose
    {
        StoreSlot,
        WeaponInventorySlot,
        ModifierInventorySlot
    }

    public class ShopDropZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private ShopDropZonePurpose purpose;
        [SerializeField] private Transform           cardAnchor;
        [SerializeField] private int                 slotIndex = -1;
        [SerializeField] private Image               slotStateImage;
        [SerializeField] private Color               occupiedColor     = new Color(0.35f, 0.35f, 0.35f, 0.95f);
        [SerializeField] private Color               emptyColor        = new Color(0.2f,  0.2f,  0.2f,  0.5f);
        [SerializeField] private Color               hoverValidColor   = new Color(0f,    0.91f, 0.59f, 0.7f);
        [SerializeField] private Color               hoverInvalidColor = new Color(1f,    0.12f, 0.43f, 0.5f);

        private ShopUIManager  _manager;
        private ModifierSlot   _logicalModifierSlot;
        private WeaponInstance _logicalWeapon;
        private bool           _occupied;

        public ShopDropZonePurpose Purpose             => purpose;
        public int                 SlotIndex           => slotIndex;
        public Transform           CardAnchor          => cardAnchor != null ? cardAnchor : transform;
        public ModifierSlot        LogicalModifierSlot => _logicalModifierSlot;
        public WeaponInstance      LogicalWeapon       => _logicalWeapon;

        public void Bind(ShopUIManager manager, ShopDropZonePurpose newPurpose, int newSlotIndex = -1,
            ModifierSlot modifierSlot = null, WeaponInstance weapon = null)
        {
            _manager             = manager;
            purpose              = newPurpose;
            slotIndex            = newSlotIndex;
            _logicalModifierSlot = modifierSlot;
            _logicalWeapon       = weapon;
        }

        public void SetOccupiedVisual(bool occupied)
        {
            _occupied = occupied;
            if (slotStateImage == null) return;
            slotStateImage.color = occupied ? occupiedColor : emptyColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!ItemCardUI.IsAnyDragging || slotStateImage == null) return;
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent<ItemCardUI>(out var card)) return;
            slotStateImage.color = IsValidDrop(card) ? hoverValidColor : hoverInvalidColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (slotStateImage == null) return;
            slotStateImage.color = _occupied ? occupiedColor : emptyColor;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (slotStateImage != null)
                slotStateImage.color = _occupied ? occupiedColor : emptyColor;
            if (_manager == null || eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent<ItemCardUI>(out var card)) return;
            if (_manager.TryHandleDrop(card, this))
                card.MarkDropAccepted();
        }

        private bool IsValidDrop(ItemCardUI card)
        {
            if (card?.Data == null) return false;
            return purpose switch
            {
                ShopDropZonePurpose.StoreSlot             => card.Data is WeaponDataSO || card.Data is ModifierDataSO,
                ShopDropZonePurpose.WeaponInventorySlot   => card.Data is WeaponDataSO,
                ShopDropZonePurpose.ModifierInventorySlot => card.Data is ModifierDataSO mod && _logicalModifierSlot != null && _logicalModifierSlot.CanEquip(mod),
                _                                         => false
            };
        }
    }
}