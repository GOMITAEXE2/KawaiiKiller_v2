using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
using DG.Tweening;
using KawaiiKiller.Modifiers;
using KawaiiKiller.Player;
using KawaiiKiller.Weapons;
using KawaiiKiller.Items;

namespace KawaiiKiller.UI.Shop
{
    public class ShopUIManager : MonoBehaviour, IPointerClickHandler
    {
        public event Action<string>  OnDebugTransaction;
        public event Action<int,int> OnDebugEconomy;
        public event Action<int,int> OnDebugReroll;
        public event Action<int>     OnDebugSlots;
        public event Action<string>  OnDebugFeedback;

        [Serializable]
        private enum CardOrigin { Store, InventoryWeapon, InventoryModifier }

        private struct CardContext
        {
            public CardOrigin     Origin;
            public int            WeaponIndex;
            public int            ModifierSlotIndex;
            public WeaponInstance SourceWeapon;
        }

        // ── Inspector ─────────────────────────────────────────────────────────────
        [Header("Dependencies")]
        [SerializeField] private KawaiiKiller.Player.Player player;
        [SerializeField] private PlayerEconomy              playerEconomy;
        [SerializeField] private PlayerWeaponController     playerWeaponController;
        [SerializeField] private ShopDatabaseSO             shopDatabase;

        [Header("Prefabs")]
        [SerializeField] private ItemCardUI             itemCardPrefab;
        [SerializeField] private WeaponInventoryBlockUI weaponInventoryBlockPrefab;

        [Header("Hierarchy")]
        [SerializeField] private Transform   dragLayer;
        [SerializeField] private Transform   inventoryBlocksRoot;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Store Slots")]
        [SerializeField] private List<ShopDropZoneUI> storeSlots      = new List<ShopDropZoneUI>();
        [SerializeField] private List<ShopDropZoneUI> extraStoreSlots = new List<ShopDropZoneUI>();

        [Header("Sell Zone")]
        [SerializeField] private SellZoneUI              sellZone;
        [SerializeField] private SellConfirmationPanelUI sellConfirmationPanel;

        [Header("Inventory Panel")]
        [SerializeField] private RectTransform inventoryPanel;
        [SerializeField] private CanvasGroup   inventoryCanvasGroup;
        [SerializeField] private Button        inventoryHandleButton;
        [SerializeField] private float         inventorySlideDuration = 0.25f;
        [SerializeField] private ScrollRect    inventoryScrollRect;

        [Header("Inventory Slide Positions")]
        [SerializeField] private Vector2 inventoryHiddenPos  = new Vector2(-400f, 0f);
        [SerializeField] private Vector2 inventoryVisiblePos = new Vector2(0f,    0f);
        [SerializeField] private Vector2 inventoryPeekPos    = new Vector2(-360f, 0f);

        [Header("Detail Panel")]
        [SerializeField] private ItemDetailPanelUI detailPanel;

        [Header("Feedback")]
        [SerializeField] private TMP_Text feedbackText;

        [Header("Reroll")]
        [SerializeField] private Button   rerollButton;
        [SerializeField] private TMP_Text rerollCostText;
        [SerializeField] private int      rerollBaseCost      = 50;
        [SerializeField] private int      rerollCostIncrement = 25;

        [Header("Shop Animation")]
        [SerializeField] private float openAnimDuration  = 0.35f;
        [SerializeField] private Ease  openEase          = Ease.OutBack;
        [SerializeField] private float closeAnimDuration = 0.2f;

        [Header("Sell")]
        [SerializeField] private float sellMultiplier = 0.7f;

        // ── Runtime state ─────────────────────────────────────────────────────────
        private ObjectPool<ItemCardUI>                      _cardPool;
        private readonly List<ItemCardUI>                   _activeCards      = new List<ItemCardUI>();
        private readonly Dictionary<ItemCardUI,CardContext> _cardContexts     = new Dictionary<ItemCardUI,CardContext>();
        private readonly List<WeaponInventoryBlockUI>       _activeBlocks     = new List<WeaponInventoryBlockUI>();
        private readonly List<ScriptableObject>             _currentStorePool = new List<ScriptableObject>();
        private readonly List<ScriptableObject>             _weightedPool     = new List<ScriptableObject>();

        private Tween     _inventoryTween;
        private Tween     _shopOpenTween;
        private Coroutine _layoutRebuildRoutine;
        private bool      _inventoryPinned;
        private bool      _extraSlotsUnlocked;
        private int       _rerollCount;
        private int       _currentRerollCost;
        private ItemCardUI _detailOwner;

        public bool IsOpen             { get; private set; }
        public bool IsInventoryVisible { get; private set; }

        // ── Unity lifecycle ───────────────────────────────────────────────────────
        private void Awake()
        {
            _cardPool = new ObjectPool<ItemCardUI>(
                CreateCard, OnGetCard, OnReleaseCard, OnDestroyCard, true, 8, 512);

            if (inventoryHandleButton != null) inventoryHandleButton.onClick.AddListener(ToggleInventoryPinned);
            if (rerollButton          != null) rerollButton.onClick.AddListener(DoReroll);

            sellZone?.Bind(this);
            BindStoreSlots();
            SetExtraSlotsVisible(false);
            ResetRerollCost();

            ApplyPanelState(false);
            ApplyInventoryState(false);
        }

        private void OnEnable()
        {
            ItemCardUI.OnCardClickedAt            += HandleCardClickedAt;
            ItemCardUI.OnCardDragStateChanged     += HandleCardDragStateChanged;
            ItemCardUI.OnCardExitedInventoryPanel += HandleCardExitedInventoryPanel;

            if (playerWeaponController != null)
            {
                playerWeaponController.OnLoadoutChanged            += RefreshInventory;
                playerWeaponController.OnCurrentWeaponIndexChanged += HandleWeaponChanged;
            }
        }

        private void Start()
        {
            RefreshAll();
        }

        private void OnDisable()
        {
            ItemCardUI.OnCardClickedAt            -= HandleCardClickedAt;
            ItemCardUI.OnCardDragStateChanged     -= HandleCardDragStateChanged;
            ItemCardUI.OnCardExitedInventoryPanel -= HandleCardExitedInventoryPanel;

            if (inventoryHandleButton != null) inventoryHandleButton.onClick.RemoveListener(ToggleInventoryPinned);
            if (rerollButton          != null) rerollButton.onClick.RemoveListener(DoReroll);

            if (playerWeaponController != null)
            {
                playerWeaponController.OnLoadoutChanged            -= RefreshInventory;
                playerWeaponController.OnCurrentWeaponIndexChanged -= HandleWeaponChanged;
            }
        }

        private void OnDestroy()
        {
            _inventoryTween?.Kill();
            _shopOpenTween?.Kill();
        }

        // ── IPointerClickHandler ──────────────────────────────────────────────────
        public void OnPointerClick(PointerEventData eventData)
        {
            CloseDetailPanel();
        }

        // ── Public API ────────────────────────────────────────────────────────────
        public void OpenShopUI()
        {
            IsOpen = true;
            player?.SetUIMode(true);
            RefreshAll();
            AnimateShopOpen();
            if (_inventoryPinned) ShowInventory();
            else                  PeekInventory();
        }

        public void CloseShopUI()
        {
            IsOpen           = false;
            _inventoryPinned = false;
            player?.SetUIMode(false);
            AnimateShopClose();
            FullHideInventory();
            CloseDetailPanel();
            sellConfirmationPanel?.Hide(true);
        }

        public void ToggleShopUI()
        {
            if (IsOpen) CloseShopUI();
            else        OpenShopUI();
        }

        public void CloseShopUIButton() => CloseShopUI();
        public void DoRerollButton()    => DoReroll();

        public void ToggleInventoryPinned()
        {
            if (!IsOpen) return;
            _inventoryPinned = !_inventoryPinned;
            if (_inventoryPinned) ShowInventory();
            else                  PeekInventory();
        }

        public void HandleInventoryHoverEnter()
        {
            if (!IsOpen || _inventoryPinned || !ItemCardUI.IsAnyDragging) return;
            ShowInventory();
        }

        public void HandleInventoryHoverExit()
        {
            if (!IsOpen || _inventoryPinned || ItemCardUI.IsAnyDragging) return;
            PeekInventory();
        }

        public void NotifyShopEntered()
        {
            ResetRerollCost();
            RefreshAll();
        }

        public void RefreshAll()
        {
            CheckExtraSlotUnlock();
            RebuildWeightedPool();
            RefreshStore();
            RefreshInventory();
            ClearFeedback();
            UpdateRerollUI();
        }

        public void RefreshStore()
        {
            ReleaseCardsByOrigin(CardOrigin.Store);
            if (shopDatabase == null || itemCardPrefab == null) return;
            if (_currentStorePool.Count == 0) RollNewStoreItems(BuildActiveSlotList().Count);

            var allSlots = BuildActiveSlotList();
            int limit    = Mathf.Min(allSlots.Count, _currentStorePool.Count);
            for (int i = 0; i < limit; i++)
            {
                ShopDropZoneUI zone = allSlots[i];
                if (zone == null) continue;
                zone.Bind(this, ShopDropZonePurpose.StoreSlot, i);
                zone.SetOccupiedVisual(true);
                ScriptableObject data = _currentStorePool[i];
                if (data is not IStorable storable) continue;
                ItemCardUI card = SpawnCard(data, storable.Price, CardOrigin.Store,
                    -1, -1, null, zone.CardAnchor);
                card?.MarkDropAccepted();
            }
            OnDebugSlots?.Invoke(limit);
        }

        public void RefreshInventory()
        {
            ReleaseCardsByOrigin(CardOrigin.InventoryWeapon);
            ReleaseCardsByOrigin(CardOrigin.InventoryModifier);
            DestroyAllBlocks();
            RebuildInventoryBlocks();
            NotifyScrollContentChanged();
        }

        public bool TryHandleDrop(ItemCardUI card, ShopDropZoneUI zone)
        {
            if (card == null || zone == null) return false;
            if (!_cardContexts.TryGetValue(card, out CardContext context)) return false;
            if (card.Data == null) return false;

            bool success = zone.Purpose switch
            {
                ShopDropZonePurpose.WeaponInventorySlot   => TryProcessWeaponDrop(card, context, zone),
                ShopDropZonePurpose.ModifierInventorySlot => TryProcessModifierDrop(card, context, zone),
                _                                         => false
            };

            if (!success) return false;
            card.MarkDropAccepted();
            CloseDetailPanel();

            if (zone.Purpose == ShopDropZonePurpose.ModifierInventorySlot
                && context.Origin != CardOrigin.Store)
                IconizeCardInZone(zone);

            RefreshAll();
            return true;
        }

        // ── Sell via SellZone ─────────────────────────────────────────────────────
        public void TryRequestSell(ItemCardUI card)
        {
            if (card == null || card.Data == null) return;
            if (!_cardContexts.TryGetValue(card, out CardContext context)) return;

            if (context.Origin == CardOrigin.Store)
            {
                PublishFeedback("Los ítems de la tienda no se pueden vender");
                return;
            }

            BuildSellPreview(card, context, out List<string> itemNames, out int totalValue);

            if (itemNames.Count == 0) { PublishFeedback("Este ítem no se puede vender"); return; }

            sellConfirmationPanel?.Show(itemNames, totalValue,
                onConfirm: () => ExecuteSell(card, context),
                onCancel:  () => PublishFeedback("Venta cancelada"));
        }

        private void BuildSellPreview(ItemCardUI card, CardContext context,
            out List<string> itemNames, out int totalValue)
        {
            itemNames  = new List<string>();
            totalValue = 0;

            if (context.Origin == CardOrigin.InventoryWeapon && card.Data is WeaponDataSO weaponData)
            {
                itemNames.Add($"Arma: {weaponData.Nombre}  ({weaponData.Precio} G)");
                int sum = weaponData.Precio;
                WeaponInstance instance = GetWeaponInstance(context.WeaponIndex);
                if (instance?.ModifierSlots != null)
                    foreach (var slot in instance.ModifierSlots)
                    {
                        if (slot == null || slot.IsEmpty || slot.Data == null) continue;
                        itemNames.Add($"  + {slot.Data.Nombre}  ({slot.Data.Precio} G)");
                        sum += slot.Data.Precio;
                    }
                totalValue = Mathf.FloorToInt(sum * Mathf.Clamp01(sellMultiplier));
                return;
            }

            if (context.Origin == CardOrigin.InventoryModifier && card.Data is ModifierDataSO modData)
            {
                itemNames.Add($"Mejora: {modData.Nombre}  ({modData.Precio} G)");
                totalValue = Mathf.FloorToInt(modData.Precio * Mathf.Clamp01(sellMultiplier));
            }
        }

        private void ExecuteSell(ItemCardUI card, CardContext context)
        {
            if (card == null || card.Data == null) return;

            if (context.Origin == CardOrigin.InventoryWeapon && card.Data is WeaponDataSO)
            {
                WeaponInstance instance = GetWeaponInstance(context.WeaponIndex);
                int sum = 0;
                if (instance?.Data != null) sum += instance.Data.Precio;
                if (instance?.ModifierSlots != null)
                    foreach (var slot in instance.ModifierSlots)
                        if (slot != null && !slot.IsEmpty && slot.Data != null)
                            sum += slot.Data.Precio;

                if (!playerWeaponController.TryRemoveWeaponAt(context.WeaponIndex, out _))
                { PublishFeedback("No se pudo vender el arma"); return; }

                int value = Mathf.FloorToInt(sum * Mathf.Clamp01(sellMultiplier));
                if (value > 0) playerEconomy.AddMoney(value);
                OnDebugTransaction?.Invoke($"Venta arma+mejoras: {value} G");
                OnDebugEconomy?.Invoke(playerEconomy.CurrentMoney, value);
                PublishFeedback($"Venta completada: +{value} G");
                RefreshAll();
                return;
            }

            if (context.Origin == CardOrigin.InventoryModifier && card.Data is ModifierDataSO modData)
            {
                WeaponInstance weapon = context.SourceWeapon ?? playerWeaponController.CurrentWeapon;
                if (weapon == null) { PublishFeedback("No se pudo vender la mejora"); return; }
                int slotIndex = context.ModifierSlotIndex;
                if (slotIndex < 0 || slotIndex >= weapon.ModifierSlots.Count)
                { PublishFeedback("No se pudo vender la mejora"); return; }
                weapon.UnequipModifier(slotIndex);
                int value = Mathf.FloorToInt(modData.Precio * Mathf.Clamp01(sellMultiplier));
                if (value > 0) playerEconomy.AddMoney(value);
                OnDebugTransaction?.Invoke($"Venta mejora: {modData.Nombre} por {value} G");
                OnDebugEconomy?.Invoke(playerEconomy.CurrentMoney, value);
                PublishFeedback($"Mejora vendida: +{value} G");
                RefreshAll();
            }
        }

        private WeaponInstance GetWeaponInstance(int index)
        {
            if (playerWeaponController == null) return null;
            IReadOnlyList<WeaponInstance> loadout = playerWeaponController.Loadout;
            if (index < 0 || index >= loadout.Count) return null;
            return loadout[index];
        }

        // ── Inventory blocks ──────────────────────────────────────────────────────
        /// <summary>
        /// Construye un bloque por cada slot de la capacidad máxima.
        ///
        /// Caso "solo fallback": el jugador no tiene armas reales (solo puños como
        /// placeholder de gameplay) → se muestran TODOS los slots como vacíos.
        ///
        /// Caso normal: se muestran las armas reales en orden y slots vacíos al final.
        /// </summary>
        private void RebuildInventoryBlocks()
        {
            if (weaponInventoryBlockPrefab == null || inventoryBlocksRoot == null
                || playerWeaponController == null) return;

            IReadOnlyList<WeaponInstance> loadout        = playerWeaponController.Loadout;
            int                           capacity       = playerWeaponController.CurrentCapacity;
            bool                          onlyFallback   = playerWeaponController.IsUsingFallbackOnly;

            // Cuando onlyFallback=true, el loadout tiene solo puños y tratamos
            // toda la capacidad como vacía. Cuando es false, mostramos las armas reales.
            int realWeaponCount = onlyFallback ? 0 : loadout.Count;

            for (int i = 0; i < capacity; i++)
            {
                WeaponInventoryBlockUI block = Instantiate(weaponInventoryBlockPrefab, inventoryBlocksRoot);
                _activeBlocks.Add(block);

                if (i < realWeaponCount)
                {
                    // ── Slot con arma real ─────────────────────────────────────────
                    WeaponInstance instance = loadout[i];
                    block.Build(this, instance, i);

                    if (instance?.Data == null) continue;

                    ItemCardUI weaponCard = SpawnCard(
                        instance.Data, instance.Data.Price,
                        CardOrigin.InventoryWeapon, i, -1, instance,
                        block.WeaponCardAnchor
                    );
                    if (weaponCard != null)
                        weaponCard.inventoryPanelBounds = inventoryPanel;

                    if (instance.ModifierSlots == null) continue;

                    for (int j = 0; j < instance.ModifierSlots.Count; j++)
                    {
                        ModifierSlot slot = instance.ModifierSlots[j];
                        if (slot == null || slot.IsEmpty || slot.Data == null) continue;

                        List<ShopDropZoneUI> zones = slot.AcceptedType == SlotType.Upgrade
                            ? block.UpgradeZones : block.AttachmentZones;
                        ShopDropZoneUI targetZone = FindZoneBySlotIndex(zones, j);
                        if (targetZone == null) continue;

                        ItemCardUI modCard = SpawnCard(
                            slot.Data, slot.Data.Price,
                            CardOrigin.InventoryModifier, i, j, instance,
                            targetZone.CardAnchor
                        );
                        if (modCard != null)
                        {
                            modCard.SetIconized(true);
                            modCard.inventoryPanelBounds = inventoryPanel;
                        }
                    }
                }
                else
                {
                    // ── Slot vacío: acepta drops de armas desde la tienda ──────────
                    block.BuildEmpty(this, i);
                }
            }
        }

        /// <summary>
        /// Fuerza el recálculo del layout del inventario.
        ///
        /// Se hace en una coroutine de DOS frames porque:
        ///   - Frame 0: los bloques viejos están marcados para Destroy pero aún vivos.
        ///              (ya los sacamos del parent con SetParent(null), así que el
        ///               LayoutGroup no los ve, pero Unity no ha procesado el Destroy)
        ///   - Frame 1: los bloques viejos están destruidos; los nuevos están activos.
        ///              Aquí recalculamos el layout correctamente.
        ///   - Frame 2 (segundo yield): forzamos un segundo rebuild por seguridad,
        ///              porque grupos anidados (upgradesSection, etc.) pueden necesitar
        ///              un ciclo extra para propagar sus tamaños al parent.
        /// </summary>
        private void NotifyScrollContentChanged()
        {
            if (_layoutRebuildRoutine != null)
                StopCoroutine(_layoutRebuildRoutine);

            if (gameObject.activeInHierarchy)
                _layoutRebuildRoutine = StartCoroutine(RebuildLayoutRoutine());
        }

        private IEnumerator RebuildLayoutRoutine()
        {
            // Frame 1: esperamos a que Unity procese los Destroy
            yield return null;

            // Rebuildeamos cada bloque desde adentro hacia afuera
            ForceRebuildAllBlocks();

            // Frame 2: segundo rebuild por si los grupos anidados necesitan otro ciclo
            yield return null;

            ForceRebuildAllBlocks();

            if (inventoryScrollRect != null)
                inventoryScrollRect.verticalNormalizedPosition = 1f;

            _layoutRebuildRoutine = null;
        }

        private void ForceRebuildAllBlocks()
        {
            Canvas.ForceUpdateCanvases();

            // Primero rebuildeamos cada bloque individualmente (sus layouts internos)
            foreach (var block in _activeBlocks)
            {
                if (block == null) continue;
                RectTransform rt = block.GetComponent<RectTransform>();
                if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }

            // Luego rebuildeamos el contenedor (inventoryBlocksRoot)
            if (inventoryBlocksRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(inventoryBlocksRoot as RectTransform);

            // Y también el content del ScrollRect por si es distinto al root
            if (inventoryScrollRect != null && inventoryScrollRect.content != null
                && inventoryScrollRect.content != inventoryBlocksRoot as RectTransform)
                LayoutRebuilder.ForceRebuildLayoutImmediate(inventoryScrollRect.content);
        }

        private ShopDropZoneUI FindZoneBySlotIndex(List<ShopDropZoneUI> zones, int slotIndex)
        {
            foreach (var z in zones) if (z != null && z.SlotIndex == slotIndex) return z;
            return null;
        }

        /// <summary>
        /// Destruye todos los bloques del inventario.
        ///
        /// IMPORTANTE: llamamos SetParent(null) ANTES de Destroy.
        /// Destroy() es diferido (se ejecuta al final del frame). Si no desparentamos
        /// primero, el VerticalLayoutGroup de inventoryBlocksRoot ve bloques viejos
        /// Y nuevos simultáneamente en el mismo frame, corrompiendo el layout.
        /// SetParent(null) saca el bloque del LayoutGroup de forma inmediata.
        /// </summary>
        private void DestroyAllBlocks()
        {
            foreach (var block in _activeBlocks)
            {
                if (block == null) continue;
                block.transform.SetParent(null, false); // sale del LayoutGroup inmediatamente
                Destroy(block.gameObject);
            }
            _activeBlocks.Clear();
        }

        // ── Detail panel ──────────────────────────────────────────────────────────
        private void HandleCardClickedAt(ItemCardUI card, Vector2 screenPos)
        {
            if (card == null) { CloseDetailPanel(); return; }
            if (_detailOwner == card) { CloseDetailPanel(); return; }
            _detailOwner = card;
            detailPanel?.Show(card.Data, screenPos);
        }

        private void CloseDetailPanel()
        {
            _detailOwner = null;
            detailPanel?.Hide();
        }

        // ── Drag state / panel visibility ─────────────────────────────────────────
        private void HandleCardDragStateChanged(ItemCardUI card, bool dragging)
        {
            if (!IsOpen) return;
            if (dragging) { ShowInventory(); return; }
            if (!_inventoryPinned) PeekInventory();
        }

        private void HandleCardExitedInventoryPanel(ItemCardUI card, bool exited)
        {
            if (!IsOpen) return;

            if (exited)
            {
                SetInventoryInteractable(false);
                SlidePanelTo(inventoryHiddenPos, inventorySlideDuration * 0.4f);
            }
            else
            {
                SetInventoryInteractable(true);
                if (ItemCardUI.IsAnyDragging && !_inventoryPinned)
                    ShowInventory();
            }
        }

        private void HandleWeaponChanged(int index) => RefreshInventory();

        // ── Animación tienda ──────────────────────────────────────────────────────
        private void AnimateShopOpen()
        {
            if (panelCanvasGroup == null) return;
            _shopOpenTween?.Kill();

            panelCanvasGroup.gameObject.SetActive(true);
            panelCanvasGroup.alpha                = 0f;
            panelCanvasGroup.interactable         = false;
            panelCanvasGroup.blocksRaycasts       = false;
            panelCanvasGroup.transform.localScale = Vector3.one * 0.88f;

            _shopOpenTween = DOTween.Sequence()
                .SetUpdate(true)
                .Join(panelCanvasGroup.DOFade(1f, openAnimDuration))
                .Join(panelCanvasGroup.transform.DOScale(Vector3.one, openAnimDuration).SetEase(openEase))
                .OnComplete(() =>
                {
                    panelCanvasGroup.interactable   = true;
                    panelCanvasGroup.blocksRaycasts = true;
                });
        }

        private void AnimateShopClose()
        {
            if (panelCanvasGroup == null) return;
            _shopOpenTween?.Kill();

            panelCanvasGroup.interactable   = false;
            panelCanvasGroup.blocksRaycasts = false;

            _shopOpenTween = DOTween.Sequence().SetUpdate(true)
                .Join(panelCanvasGroup.DOFade(0f, closeAnimDuration))
                .Join(panelCanvasGroup.transform
                    .DOScale(Vector3.one * 0.88f, closeAnimDuration).SetEase(Ease.InBack));
        }

        // ── Animación inventario ──────────────────────────────────────────────────
        private void ShowInventory()
        {
            if (inventoryPanel == null) return;
            IsInventoryVisible = true;
            SetInventoryInteractable(true);
            SlidePanelTo(inventoryVisiblePos, inventorySlideDuration);
        }

        private void PeekInventory()
        {
            if (inventoryPanel == null) return;
            IsInventoryVisible = false;
            SetInventoryInteractable(true);
            SlidePanelTo(inventoryPeekPos, inventorySlideDuration * 0.7f);
        }

        private void FullHideInventory()
        {
            if (inventoryPanel == null) return;
            IsInventoryVisible = false;
            SetInventoryInteractable(false);
            SlidePanelTo(inventoryHiddenPos, inventorySlideDuration * 0.7f);
        }

        private void SlidePanelTo(Vector2 target, float duration)
        {
            if (inventoryPanel == null) return;
            _inventoryTween?.Kill();
            inventoryPanel.gameObject.SetActive(true);
            bool toVisible  = target == inventoryVisiblePos;
            _inventoryTween = inventoryPanel
                .DOAnchorPos(target, duration)
                .SetEase(toVisible ? Ease.OutCubic : Ease.InCubic)
                .SetUpdate(true);
        }

        private void SetInventoryInteractable(bool value)
        {
            if (inventoryCanvasGroup == null) return;
            inventoryCanvasGroup.alpha          = 1f;
            inventoryCanvasGroup.interactable   = value;
            inventoryCanvasGroup.blocksRaycasts = value;
        }

        private void ApplyPanelState(bool visible)
        {
            if (panelCanvasGroup == null) return;
            panelCanvasGroup.alpha          = visible ? 1f : 0f;
            panelCanvasGroup.interactable   = visible;
            panelCanvasGroup.blocksRaycasts = visible;
        }

        private void ApplyInventoryState(bool visible)
        {
            if (inventoryPanel == null) return;
            inventoryPanel.anchoredPosition = visible ? inventoryVisiblePos : inventoryPeekPos;
            inventoryPanel.gameObject.SetActive(true);
            SetInventoryInteractable(true);
        }

        // ── Reroll ────────────────────────────────────────────────────────────────
        private void DoReroll()
        {
            if (!IsOpen) return;
            if (playerEconomy == null || !playerEconomy.TrySpend(_currentRerollCost))
            { PublishFeedback("Fondos insuficientes para reroll"); return; }
            OnDebugEconomy?.Invoke(playerEconomy.CurrentMoney, -_currentRerollCost);
            _rerollCount++;
            int usedCost        = _currentRerollCost;
            _currentRerollCost += rerollCostIncrement;
            RollNewStoreItems(BuildActiveSlotList().Count);
            RefreshStore();
            UpdateRerollUI();
            PublishFeedback($"Reroll realizado. Próximo costo: {_currentRerollCost}");
            OnDebugReroll?.Invoke(usedCost, _rerollCount);
        }

        private void ResetRerollCost()
        {
            _rerollCount       = 0;
            _currentRerollCost = rerollBaseCost;
            _currentStorePool.Clear();
            UpdateRerollUI();
        }

        private void UpdateRerollUI()
        {
            if (rerollCostText != null) rerollCostText.text = _currentRerollCost.ToString();
        }

        private void RollNewStoreItems(int count)
        {
            _currentStorePool.Clear();
            if (_weightedPool.Count == 0) return;
            var available = new List<ScriptableObject>(_weightedPool);
            int limit     = Mathf.Min(count, available.Count);
            for (int i = 0; i < limit; i++)
            {
                float total = 0f;
                foreach (var item in available) total += GetItemWeight(item);
                float roll       = UnityEngine.Random.Range(0f, total);
                float cumulative = 0f;
                for (int j = 0; j < available.Count; j++)
                {
                    cumulative += GetItemWeight(available[j]);
                    if (roll <= cumulative)
                    {
                        _currentStorePool.Add(available[j]);
                        available.RemoveAt(j);
                        break;
                    }
                }
            }
        }

        private float GetItemWeight(ScriptableObject data)
        {
            if (shopDatabase?.RaritySettings == null) return 1f;
            ItemRarity rarity = data switch
            {
                WeaponDataSO w   => w.Rareza,
                ModifierDataSO m => m.Rareza,
                _                => ItemRarity.Simple
            };
            return shopDatabase.RaritySettings.GetWeight(rarity);
        }

        private void RebuildWeightedPool()
        {
            _weightedPool.Clear();
            if (shopDatabase == null) return;
            if (shopDatabase.AvailableWeapons     != null)
                foreach (var w in shopDatabase.AvailableWeapons)     if (w != null) _weightedPool.Add(w);
            if (shopDatabase.AvailableUpgrades    != null)
                foreach (var m in shopDatabase.AvailableUpgrades)    if (m != null) _weightedPool.Add(m);
            if (shopDatabase.AvailableAttachments != null)
                foreach (var a in shopDatabase.AvailableAttachments) if (a != null) _weightedPool.Add(a);
        }

        // ── Extra slot unlock ─────────────────────────────────────────────────────
        private void CheckExtraSlotUnlock()
        {
            if (_extraSlotsUnlocked || playerWeaponController == null) return;
            foreach (var instance in playerWeaponController.Loadout)
            {
                if (instance == null) continue;
                foreach (var slot in instance.ModifierSlots)
                {
                    if (slot == null || slot.IsEmpty || slot.EquippedModifier?.Efectos == null) continue;
                    foreach (var effect in slot.EquippedModifier.Efectos)
                    {
                        if (effect is not ShopSlotUnlockEffectSO) continue;
                        _extraSlotsUnlocked = true;
                        SetExtraSlotsVisible(true);
                        return;
                    }
                }
            }
        }

        private void SetExtraSlotsVisible(bool visible)
        {
            if (extraStoreSlots == null) return;
            foreach (var slot in extraStoreSlots) if (slot != null) slot.gameObject.SetActive(visible);
        }

        // ── Store slot helpers ────────────────────────────────────────────────────
        private void BindStoreSlots()
        {
            if (storeSlots != null)
                for (int i = 0; i < storeSlots.Count; i++)
                    storeSlots[i]?.Bind(this, ShopDropZonePurpose.StoreSlot, i);
            if (extraStoreSlots != null)
                for (int i = 0; i < extraStoreSlots.Count; i++)
                    extraStoreSlots[i]?.Bind(this, ShopDropZonePurpose.StoreSlot, storeSlots.Count + i);
        }

        private List<ShopDropZoneUI> BuildActiveSlotList()
        {
            var list = new List<ShopDropZoneUI>();
            if (storeSlots != null) list.AddRange(storeSlots);
            if (_extraSlotsUnlocked && extraStoreSlots != null) list.AddRange(extraStoreSlots);
            return list;
        }

        // ── Drop handling ─────────────────────────────────────────────────────────
        private bool TryProcessWeaponDrop(ItemCardUI card, CardContext context, ShopDropZoneUI zone)
        {
            if (playerWeaponController == null || playerEconomy == null) return false;
            if (card.Data is not WeaponDataSO weaponData)
            { PublishFeedback("Drop inválido para arma"); return false; }

            if (context.Origin == CardOrigin.Store)
            {
                // Verificar capacidad (sin contar el fallback que será removido)
                bool wouldBeEmpty = playerWeaponController.IsUsingFallbackOnly;
                int  realCount    = wouldBeEmpty ? 0 : playerWeaponController.Loadout.Count;
                if (realCount >= playerWeaponController.CurrentCapacity)
                { PublishFeedback("Inventario de armas lleno"); return false; }

                int price = Mathf.Max(0, card.Price);
                if (!playerEconomy.TrySpend(price)) { PublishFeedback("Fondos insuficientes"); return false; }
                if (!playerWeaponController.TryAddWeaponToInventory(weaponData))
                { playerEconomy.AddMoney(price); PublishFeedback("No se pudo añadir arma"); return false; }

                OnDebugTransaction?.Invoke($"Compra arma: {weaponData.Nombre} por {price}");
                OnDebugEconomy?.Invoke(playerEconomy.CurrentMoney, -price);
                ConsumeStoreItem(card);
                PublishFeedback("Compra de arma completada");
                return true;
            }

            if (context.Origin == CardOrigin.InventoryWeapon)
            {
                if (zone.SlotIndex < 0 || zone.SlotIndex >= playerWeaponController.Loadout.Count)
                { PublishFeedback("Slot inválido"); return false; }
                if (!playerWeaponController.SelectWeaponByIndex(zone.SlotIndex))
                { PublishFeedback("No se pudo seleccionar arma"); return false; }
                OnDebugTransaction?.Invoke($"Arma seleccionada: slot {zone.SlotIndex}");
                PublishFeedback("Arma activa actualizada");
                return true;
            }

            PublishFeedback("Drop inválido para arma");
            return false;
        }

        private bool TryProcessModifierDrop(ItemCardUI card, CardContext context, ShopDropZoneUI zone)
        {
            if (playerWeaponController == null || playerEconomy == null) return false;
            if (card.Data is not ModifierDataSO modifierData)
            { PublishFeedback("Solo se aceptan mejoras"); return false; }

            WeaponInstance weapon = zone.LogicalWeapon ?? playerWeaponController.CurrentWeapon;
            if (weapon == null) { PublishFeedback("No hay arma activa"); return false; }

            int targetSlot = zone.SlotIndex;
            if (targetSlot < 0 || targetSlot >= weapon.ModifierSlots.Count)
            { PublishFeedback("Slot inválido"); return false; }

            ModifierSlot target = weapon.ModifierSlots[targetSlot];
            if (target == null || !target.CanEquip(modifierData))
            { PublishFeedback("Tipo de slot incompatible"); return false; }

            if (context.Origin == CardOrigin.Store)
            {
                if (!target.IsEmpty) { PublishFeedback("El slot objetivo ya está ocupado"); return false; }
                int price = Mathf.Max(0, card.Price);
                if (!playerEconomy.TrySpend(price)) { PublishFeedback("Fondos insuficientes"); return false; }
                if (!weapon.TryEquipModifier(targetSlot, modifierData))
                { playerEconomy.AddMoney(price); PublishFeedback("No se pudo equipar mejora"); return false; }
                OnDebugTransaction?.Invoke($"Compra mejora: {modifierData.Nombre} por {price}");
                OnDebugEconomy?.Invoke(playerEconomy.CurrentMoney, -price);
                ConsumeStoreItem(card);
                CheckExtraSlotUnlock();
                PublishFeedback("Mejora comprada y equipada");
                return true;
            }

            if (context.Origin == CardOrigin.InventoryModifier)
            {
                if (context.SourceWeapon == null || context.SourceWeapon != weapon)
                { PublishFeedback("La mejora pertenece a otra arma"); return false; }
                int sourceSlot = context.ModifierSlotIndex;
                if (sourceSlot < 0 || sourceSlot >= weapon.ModifierSlots.Count) return false;
                if (sourceSlot == targetSlot) return true;
                ModifierSlot source = weapon.ModifierSlots[sourceSlot];
                if (source == null || source.IsEmpty || source.Data == null) return false;
                if (!target.IsEmpty) { PublishFeedback("El slot objetivo ya está ocupado"); return false; }
                ModifierDataSO sourceData = source.Data;
                if (!weapon.TryEquipModifier(targetSlot, sourceData))
                { PublishFeedback("No se pudo mover mejora"); return false; }
                weapon.UnequipModifier(sourceSlot);
                OnDebugTransaction?.Invoke($"Mejora movida: {sourceData.Nombre} slot {sourceSlot}→{targetSlot}");
                PublishFeedback("Mejora movida");
                return true;
            }

            PublishFeedback("Drop inválido para mejora");
            return false;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private void ConsumeStoreItem(ItemCardUI card)
        {
            if (card == null) return;
            if (!_cardContexts.TryGetValue(card, out CardContext ctx)) return;
            if (ctx.Origin != CardOrigin.Store) return;

            int poolIndex = _currentStorePool.IndexOf(card.Data);
            if (poolIndex >= 0) _currentStorePool.RemoveAt(poolIndex);

            _activeCards.Remove(card);
            _cardContexts.Remove(card);
            _cardPool.Release(card);
        }

        private void IconizeCardInZone(ShopDropZoneUI zone)
        {
            if (zone == null) return;
            foreach (var card in _activeCards)
            {
                if (card.transform.parent == zone.CardAnchor)
                {
                    card.SetIconized(true);
                    return;
                }
            }
        }

        private void PublishFeedback(string message)
        {
            if (feedbackText != null) feedbackText.text = message;
            OnDebugFeedback?.Invoke(message);
        }

        private void ClearFeedback()
        {
            if (feedbackText != null) feedbackText.text = string.Empty;
        }

        private string ResolveName(ScriptableObject data) => data switch
        {
            WeaponDataSO w   => w.Nombre,
            ModifierDataSO m => m.Nombre,
            _                => data.name
        };

        // ── Card pool ─────────────────────────────────────────────────────────────
        private ItemCardUI SpawnCard(ScriptableObject data, int price, CardOrigin origin,
            int weaponIndex, int modifierSlotIndex, WeaponInstance sourceWeapon, Transform parent)
        {
            if (data == null || parent == null) return null;
            if (data is not IStorable storable) return null;
            ItemCardUI card = _cardPool.Get();
            card.SetParentAndReset(parent);
            card.Bind(data, ResolveName(data), price, storable.Icon, dragLayer);
            _activeCards.Add(card);
            _cardContexts[card] = new CardContext
            {
                Origin            = origin,
                WeaponIndex       = weaponIndex,
                ModifierSlotIndex = modifierSlotIndex,
                SourceWeapon      = sourceWeapon
            };
            return card;
        }

        private void ReleaseCardsByOrigin(CardOrigin origin)
        {
            for (int i = _activeCards.Count - 1; i >= 0; i--)
            {
                ItemCardUI card = _activeCards[i];
                if (card == null) continue;
                if (!_cardContexts.TryGetValue(card, out CardContext ctx)) continue;
                if (ctx.Origin != origin) continue;
                if (_detailOwner == card) CloseDetailPanel();
                _activeCards.RemoveAt(i);
                _cardContexts.Remove(card);
                _cardPool.Release(card);
            }
        }

        private ItemCardUI CreateCard()
        {
            if (itemCardPrefab == null) return null;
            return Instantiate(itemCardPrefab);
        }

        private void OnGetCard(ItemCardUI card)    { if (card != null) card.gameObject.SetActive(true); }

        private void OnReleaseCard(ItemCardUI card)
        {
            if (card == null) return;
            card.ClearBinding();
            card.gameObject.SetActive(false);
            card.transform.SetParent(transform, false);
        }

        private void OnDestroyCard(ItemCardUI card) { if (card != null) Destroy(card.gameObject); }
    }
}