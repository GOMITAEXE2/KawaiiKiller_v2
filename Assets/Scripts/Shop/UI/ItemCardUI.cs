using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using KawaiiKiller.Items;
using KawaiiKiller.Weapons;
using KawaiiKiller.Modifiers;

namespace KawaiiKiller.UI.Shop
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class ItemCardUI : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const int PriceTruncateThreshold = 9999;
        private const int NameTruncateThreshold  = 10;

        private static readonly Color ColorSimple     = new Color(1f,    0.122f, 0.431f, 1f);
        private static readonly Color ColorRara       = new Color(0.420f, 0.373f, 1f,    1f);
        private static readonly Color ColorExtraña    = new Color(0.85f,  0.20f,  0.20f, 1f);
        private static readonly Color ColorLegendaria = new Color(1f,    0.816f,  0f,    1f);

        public static event Action<ItemCardUI, Vector2> OnCardClickedAt;
        public static event Action<ItemCardUI, bool>    OnCardDragStateChanged;

        // Disparado cuando la card cruza el límite del panel de inventario durante un drag.
        public static event Action<ItemCardUI, bool>    OnCardExitedInventoryPanel;

        public static bool IsAnyDragging { get; private set; }

        [SerializeField] private Image    iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Image    outlineImage;

        [Header("Iconized mode")]
        [SerializeField] private GameObject fullCardRoot;
        [SerializeField] private GameObject iconizedRoot;
        [SerializeField] private Image      iconizedImage;

        [Header("Drag spring")]
        [SerializeField] private float springStiffness = 18f;
        [SerializeField] private float springDamping   = 6f;
        [SerializeField] private float dragScaleTarget = 1.08f;
        [SerializeField] private float scaleSpeed      = 12f;

        // Asignado externamente para detectar salida del panel durante el drag.
        [HideInInspector] public RectTransform inventoryPanelBounds;

        public ScriptableObject Data  { get; private set; }
        public int              Price { get; private set; }

        private CanvasGroup   _canvasGroup;
        private RectTransform _rectTransform;
        private Transform     _originalParent;
        private int           _originalSiblingIndex;
        private Vector2       _originalAnchoredPosition;
        private Transform     _dragLayer;
        private bool          _dropAccepted;
        private bool          _isDragging;
        private bool          _isIconized;
        private bool          _wasIconizedBeforeDrag;
        private bool          _detailOpen;
        private bool          _exitedPanel;

        private Vector2 _springPos;
        private Vector2 _springVel;
        private Vector2 _cursorTarget;
        private float   _currentScale = 1f;

        private void Awake()
        {
            _canvasGroup   = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!_isDragging) return;

            float dt      = Time.unscaledDeltaTime;
            Vector2 force = (_cursorTarget - _springPos) * springStiffness - _springVel * springDamping;
            _springVel   += force * dt;
            _springPos   += _springVel * dt;
            transform.position = _springPos;

            _currentScale        = Mathf.Lerp(_currentScale, dragScaleTarget, dt * scaleSpeed);
            transform.localScale = Vector3.one * _currentScale;

            CheckPanelBoundsExit();
        }

        private void CheckPanelBoundsExit()
        {
            if (inventoryPanelBounds == null) return;

            bool outside = !RectTransformUtility.RectangleContainsScreenPoint(
                inventoryPanelBounds,
                _cursorTarget,
                null
            );

            if (outside == _exitedPanel) return;
            _exitedPanel = outside;
            OnCardExitedInventoryPanel?.Invoke(this, outside);
        }

        public void Bind(ScriptableObject data, string displayName, int price, Sprite icon, Transform dragLayer)
        {
            Data                   = data;
            Price                  = Mathf.Max(0, price);
            _dragLayer             = dragLayer;
            _dropAccepted          = false;
            _isDragging            = false;
            _detailOpen            = false;
            _exitedPanel           = false;
            _wasIconizedBeforeDrag = false;
            _currentScale          = 1f;
            transform.localScale        = Vector3.one;
            _canvasGroup.alpha          = 1f;
            _canvasGroup.blocksRaycasts = true;

            if (outlineImage != null) outlineImage.color = ResolveRarityColor(data);

            if (nameText != null)
                nameText.text = string.IsNullOrWhiteSpace(displayName)
                    ? "Item"
                    : displayName.Length > NameTruncateThreshold ? "???" : displayName;

            if (priceText != null)
                priceText.text = Price > PriceTruncateThreshold ? "$$$" : Price.ToString();

            if (iconImage != null)
            {
                iconImage.sprite  = icon;
                iconImage.enabled = icon != null;
            }

            SetIconized(false);
        }

        public void ClearBinding()
        {
            Data                   = null;
            Price                  = 0;
            _dropAccepted          = false;
            _isDragging            = false;
            _detailOpen            = false;
            _exitedPanel           = false;
            _wasIconizedBeforeDrag = false;
            _currentScale          = 1f;
            inventoryPanelBounds   = null;
            transform.localScale        = Vector3.one;
            _canvasGroup.alpha          = 1f;
            _canvasGroup.blocksRaycasts = true;
            if (nameText  != null) nameText.text  = string.Empty;
            if (priceText != null) priceText.text = string.Empty;
            if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
            SetIconized(false);
        }

        public void SetParentAndReset(Transform parent)
        {
            transform.SetParent(parent, false);
            _rectTransform.anchorMin        = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax        = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot            = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localRotation    = Quaternion.identity;
            _rectTransform.localScale       = Vector3.one;
            _currentScale = 1f;
        }

        public void MarkDropAccepted() => _dropAccepted = true;

        public void SetIconized(bool iconized)
        {
            _isIconized = iconized;
            if (fullCardRoot  != null) fullCardRoot.SetActive(!iconized);
            if (iconizedRoot  != null) iconizedRoot.SetActive(iconized);
            if (iconized && iconizedImage != null && iconImage != null)
                iconizedImage.sprite = iconImage.sprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (_isDragging) return;
            _detailOpen = !_detailOpen;
            OnCardClickedAt?.Invoke(
                _detailOpen ? this : null,
                _detailOpen ? eventData.position : Vector2.zero
            );
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Data == null) return;

            _isDragging            = true;
            _dropAccepted          = false;
            _exitedPanel           = false;
            _wasIconizedBeforeDrag = _isIconized;
            IsAnyDragging          = true;

            OnCardDragStateChanged?.Invoke(this, true);

            _originalParent           = transform.parent;
            _originalSiblingIndex     = transform.GetSiblingIndex();
            _originalAnchoredPosition = _rectTransform.anchoredPosition;

            if (_dragLayer != null) transform.SetParent(_dragLayer, true);
            _canvasGroup.blocksRaycasts = false;

            // Siempre muestra la versión completa durante el drag.
            if (_isIconized) SetIconized(false);

            OnCardClickedAt?.Invoke(null, Vector2.zero);
            _springPos    = transform.position;
            _springVel    = Vector2.zero;
            _cursorTarget = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _cursorTarget = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            if (!_isDragging) return;

            _isDragging          = false;
            IsAnyDragging        = false;
            _exitedPanel         = false;
            _currentScale        = 1f;
            transform.localScale = Vector3.one;

            OnCardDragStateChanged?.Invoke(this, false);
            OnCardExitedInventoryPanel?.Invoke(this, false);

            if (_dropAccepted) return;

            if (_originalParent != null)
            {
                transform.SetParent(_originalParent, false);
                transform.SetSiblingIndex(_originalSiblingIndex);
                _rectTransform.anchoredPosition = _originalAnchoredPosition;
            }

            // Restaura el estado iconized que tenía antes de iniciar el drag.
            if (_wasIconizedBeforeDrag)
                SetIconized(true);
        }

        private static Color ResolveRarityColor(ScriptableObject data)
        {
            ItemRarity rarity = data switch
            {
                WeaponDataSO w   => w.Rareza,
                ModifierDataSO m => m.Rareza,
                _                => ItemRarity.Simple
            };
            return rarity switch
            {
                ItemRarity.Simple     => ColorSimple,
                ItemRarity.Rara       => ColorRara,
                ItemRarity.Extraña    => ColorExtraña,
                ItemRarity.Legendaria => ColorLegendaria,
                _                     => ColorSimple
            };
        }
    }
}