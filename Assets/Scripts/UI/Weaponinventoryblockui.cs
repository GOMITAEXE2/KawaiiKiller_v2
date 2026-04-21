using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KawaiiKiller.Modifiers;
using KawaiiKiller.Weapons;

namespace KawaiiKiller.UI.Shop
{
    public class WeaponInventoryBlockUI : MonoBehaviour
    {
        public enum SlotLayoutMode { Grid, HorizontalWrap, Vertical }

        [Header("Weapon Card Area")]
        [SerializeField] private Transform weaponCardAnchor;
        [SerializeField] private Image     weaponBlockOutline;

        [Header("Modifier Sections")]
        [SerializeField] private Transform  upgradesRoot;
        [SerializeField] private Transform  attachmentsRoot;
        [SerializeField] private GameObject upgradesSection;
        [SerializeField] private GameObject attachmentsSection;

        [Header("Slot Prefab")]
        [SerializeField] private ShopDropZoneUI modifierSlotPrefab;

        [Header("Empty Slot Placeholder")]
        [SerializeField] private GameObject emptyWeaponSlotPrefab;

        [Header("Layout")]
        [SerializeField] private SlotLayoutMode layoutMode     = SlotLayoutMode.Grid;
        [SerializeField] private int            gridColumns    = 4;
        [SerializeField] private TextAnchor     childAlignment = TextAnchor.MiddleCenter;
        [SerializeField] private float          cellSpacing    = 4f;

        [Header("Style")]
        [SerializeField] private Color upgradesSectionColor    = new Color(0.85f, 0.20f, 0.20f, 0.25f);
        [SerializeField] private Color attachmentsSectionColor = new Color(0.25f, 0.45f, 0.85f, 0.25f);
        [SerializeField] private Color emptySlotOutlineColor   = new Color(0.3f,  0.3f,  0.3f,  0.5f);

        private readonly List<ShopDropZoneUI> _upgradeZones    = new List<ShopDropZoneUI>();
        private readonly List<ShopDropZoneUI> _attachmentZones = new List<ShopDropZoneUI>();

        // Drop zone del slot vacío (para recibir armas de la tienda)
        private ShopDropZoneUI _emptyWeaponDropZone;

        public Transform            WeaponCardAnchor => weaponCardAnchor;
        public List<ShopDropZoneUI> UpgradeZones     => _upgradeZones;
        public List<ShopDropZoneUI> AttachmentZones  => _attachmentZones;

        // ── Build con arma ────────────────────────────────────────────────────────
        /// <summary>Construye el bloque para un slot de arma ocupado.</summary>
        public void Build(ShopUIManager manager, WeaponInstance weapon, int weaponIndex)
        {
            ClearSlots();
            SetOutlineColor(Color.white);

            if (weapon == null || weapon.ModifierSlots == null)
            {
                upgradesSection?.SetActive(false);
                attachmentsSection?.SetActive(false);
                return;
            }

            bool hasUpgrades = false, hasAttachments = false;

            for (int i = 0; i < weapon.ModifierSlots.Count; i++)
            {
                ModifierSlot slot = weapon.ModifierSlots[i];
                if (slot == null) continue;
                if (slot.AcceptedType == SlotType.Upgrade)
                {
                    _upgradeZones.Add(BuildModifierSlot(manager, weapon, i, upgradesRoot));
                    hasUpgrades = true;
                }
                else
                {
                    _attachmentZones.Add(BuildModifierSlot(manager, weapon, i, attachmentsRoot));
                    hasAttachments = true;
                }
            }

            SetSectionVisual(upgradesSection,    hasUpgrades,    upgradesSectionColor);
            SetSectionVisual(attachmentsSection, hasAttachments, attachmentsSectionColor);
            if (hasUpgrades)    ApplyLayout(upgradesRoot);
            if (hasAttachments) ApplyLayout(attachmentsRoot);
        }

        // ── Build vacío ───────────────────────────────────────────────────────────
        /// <summary>
        /// Construye un slot vacío. Muestra el visual decorativo de "libre" y
        /// crea una ShopDropZoneUI de tipo WeaponInventorySlot para recibir armas.
        /// </summary>
        public void BuildEmpty(ShopUIManager manager, int weaponSlotIndex)
        {
            ClearSlots();
            SetOutlineColor(emptySlotOutlineColor);
            upgradesSection?.SetActive(false);
            attachmentsSection?.SetActive(false);

            if (weaponCardAnchor == null) return;

            // Visual decorativo del slot vacío
            if (emptyWeaponSlotPrefab != null)
                Instantiate(emptyWeaponSlotPrefab, weaponCardAnchor);

            // Drop zone funcional para arrastrar armas desde la tienda
            if (modifierSlotPrefab != null && manager != null)
            {
                _emptyWeaponDropZone = Instantiate(modifierSlotPrefab, weaponCardAnchor);
                _emptyWeaponDropZone.Bind(manager, ShopDropZonePurpose.WeaponInventorySlot, weaponSlotIndex);
                _emptyWeaponDropZone.SetOccupiedVisual(false);
            }
        }

        public void SetOutlineColor(Color color)
        {
            if (weaponBlockOutline != null) weaponBlockOutline.color = color;
        }

        // ── Helpers internos ──────────────────────────────────────────────────────
        private ShopDropZoneUI BuildModifierSlot(ShopUIManager manager, WeaponInstance weapon,
            int slotIndex, Transform parent)
        {
            ShopDropZoneUI zone = Instantiate(modifierSlotPrefab, parent);
            ModifierSlot   slot = weapon.ModifierSlots[slotIndex];
            zone.Bind(manager, ShopDropZonePurpose.ModifierInventorySlot, slotIndex, slot, weapon);
            zone.SetOccupiedVisual(slot != null && !slot.IsEmpty);
            return zone;
        }

        private void ApplyLayout(Transform root)
        {
            if (root == null) return;
            switch (layoutMode)
            {
                case SlotLayoutMode.Grid:
                    EnsureLayoutGroup<GridLayoutGroup>(root, lg =>
                    {
                        lg.childAlignment  = childAlignment;
                        lg.spacing         = new Vector2(cellSpacing, cellSpacing);
                        lg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
                        lg.constraintCount = gridColumns;
                    });
                    break;

                case SlotLayoutMode.HorizontalWrap:
                    EnsureLayoutGroup<HorizontalLayoutGroup>(root, lg =>
                    {
                        lg.childAlignment         = childAlignment;
                        lg.spacing                = cellSpacing;
                        lg.childControlWidth      = false;
                        lg.childControlHeight     = false;
                        lg.childForceExpandWidth  = false;
                        lg.childForceExpandHeight = false;
                    });
                    break;

                case SlotLayoutMode.Vertical:
                    EnsureLayoutGroup<VerticalLayoutGroup>(root, lg =>
                    {
                        lg.childAlignment         = childAlignment;
                        lg.spacing                = cellSpacing;
                        lg.childControlWidth      = false;
                        lg.childControlHeight     = false;
                        lg.childForceExpandWidth  = true;
                        lg.childForceExpandHeight = false;
                    });
                    break;
            }
        }

        private static void EnsureLayoutGroup<T>(Transform root, System.Action<T> configure)
            where T : LayoutGroup
        {
            if (!root.TryGetComponent<T>(out var lg))
                lg = root.gameObject.AddComponent<T>();
            configure(lg);
        }

        private static void SetSectionVisual(GameObject section, bool active, Color color)
        {
            if (section == null) return;
            section.SetActive(active);
            if (!active) return;
            Image bg = section.GetComponent<Image>();
            if (bg != null) bg.color = color;
        }

        private void ClearSlots()
        {
            foreach (var z in _upgradeZones)    if (z != null) Destroy(z.gameObject);
            foreach (var z in _attachmentZones) if (z != null) Destroy(z.gameObject);
            _upgradeZones.Clear();
            _attachmentZones.Clear();

            if (_emptyWeaponDropZone != null)
            {
                Destroy(_emptyWeaponDropZone.gameObject);
                _emptyWeaponDropZone = null;
            }

            if (weaponCardAnchor != null)
            {
                for (int i = weaponCardAnchor.childCount - 1; i >= 0; i--)
                    Destroy(weaponCardAnchor.GetChild(i).gameObject);
            }
        }
    }
}