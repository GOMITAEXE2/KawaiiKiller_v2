using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KawaiiKiller.Items;
using KawaiiKiller.Weapons;
using KawaiiKiller.Modifiers;

namespace KawaiiKiller.UI.Shop
{
    public class ItemDetailPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup    canvasGroup;
        [SerializeField] private RectTransform  panelRect;
        [SerializeField] private ScrollRect     scrollRect;
        [SerializeField] private Image          backgroundImage;
        [SerializeField] private Image          rarityBadgeImage;

        [Header("Texts")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text rarityText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text descriptionText;

        [Header("Icon")]
        [SerializeField] private Image iconImage;

        [Header("Animation")]
        [SerializeField] private float popDuration = 0.18f;

        [Header("Rarity Colors")]
        [SerializeField] private Color colorSimple     = new Color(0.65f, 0.65f, 0.65f, 1f);
        [SerializeField] private Color colorRara       = new Color(0.25f, 0.50f, 1.00f, 1f);
        [SerializeField] private Color colorExtraña    = new Color(0.85f, 0.20f, 0.20f, 1f);
        [SerializeField] private Color colorLegendaria = new Color(0.60f, 0.15f, 0.85f, 1f);

        [Header("Layout")]
        [SerializeField] private Vector2 offset = new Vector2(10f, 0f);
        [SerializeField] private float   screenPadding = 20f;

        private Coroutine _animRoutine;
        private Canvas    _rootCanvas;

        private void Awake()
        {
            _rootCanvas = GetComponentInParent<Canvas>();
            Hide(true);
        }

        public void Show(ScriptableObject data, Vector2 screenPosition)
        {
            if (data == null) { Hide(true); return; }

            Populate(data);
            PositionNear(screenPosition);

            gameObject.SetActive(true);

            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _animRoutine = StartCoroutine(PopIn());
        }

        public void Hide(bool instant = false)
        {
            if (_animRoutine != null) { StopCoroutine(_animRoutine); _animRoutine = null; }

            if (instant || !gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                 if (canvasGroup != null) canvasGroup.alpha       = 0f;
                 if (panelRect   != null) panelRect.localScale    = Vector3.zero;
                 return;
            }

             _animRoutine = StartCoroutine(PopOut());
        }

        // ── Populate ─────────────────────────────────────────────────────────────
        private void Populate(ScriptableObject data)
        {
            ItemRarity rarity  = ResolveRarity(data);
            Color      color   = RarityToColor(rarity);
            string     rarityLabel = rarity.ToString();

            if (rarityBadgeImage != null) rarityBadgeImage.color = color;
            if (backgroundImage  != null)
            {
                Color bg = color;
                bg.a = 0.15f;
                backgroundImage.color = bg;
            }

            if (nameText   != null) nameText.text   = ResolveName(data);
            if (rarityText != null)
            {
                rarityText.text  = rarityLabel;
                rarityText.color = color;
            }
            if (priceText  != null) priceText.text  = ResolvePrice(data).ToString();
            if (descriptionText != null) descriptionText.text = ResolveDescription(data);

            if (iconImage != null)
            {
                Sprite icon = data is IStorable s ? s.Icon : null;
                iconImage.sprite  = icon;
                iconImage.enabled = icon != null;
            }

            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;
        }

        // ── Positioning ───────────────────────────────────────────────────────────
        private void PositionNear(Vector2 screenPosition)
        {
            if (panelRect == null) return;

            Canvas canvas = _rootCanvas;
            if (canvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint
            );

            Vector2 panelSize = panelRect.sizeDelta;
            Vector2 candidate = localPoint + offset;

            RectTransform canvasRect = canvas.transform as RectTransform;
            float halfW = canvasRect.rect.width  * 0.5f;
            float halfH = canvasRect.rect.height * 0.5f;

            if (candidate.x + panelSize.x * 0.5f > halfW - screenPadding)
                candidate.x = localPoint.x - panelSize.x - offset.x;

            if (candidate.y - panelSize.y * 0.5f < -halfH + screenPadding)
                candidate.y = -halfH + screenPadding + panelSize.y * 0.5f;

            if (candidate.y + panelSize.y * 0.5f > halfH - screenPadding)
                candidate.y = halfH - screenPadding - panelSize.y * 0.5f;

            panelRect.anchoredPosition = candidate;
        }

        // ── Animation ─────────────────────────────────────────────────────────────
        private IEnumerator PopIn()
        {
            float elapsed  = 0f;
            float duration = Mathf.Max(0.01f, popDuration);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (panelRect   != null) panelRect.localScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t  = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                if (panelRect   != null) panelRect.localScale = Vector3.one * ease;
                if (canvasGroup != null) canvasGroup.alpha    = ease;
                yield return null;
            }

            if (panelRect   != null) panelRect.localScale = Vector3.one;
            if (canvasGroup != null) canvasGroup.alpha    = 1f;
            _animRoutine = null;
        }

        private IEnumerator PopOut()
        {
            float elapsed  = 0f;
            float duration = Mathf.Max(0.01f, popDuration * 0.6f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t  = Mathf.Clamp01(elapsed / duration);
                if (panelRect   != null) panelRect.localScale = Vector3.one * (1f - t);
                if (canvasGroup != null) canvasGroup.alpha    = 1f - t;
                yield return null;
            }

            gameObject.SetActive(false);
            _animRoutine = null;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private Color RarityToColor(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Simple     => colorSimple,
            ItemRarity.Rara       => colorRara,
            ItemRarity.Extraña    => colorExtraña,
            ItemRarity.Legendaria => colorLegendaria,
            _                     => colorSimple
        };

        private ItemRarity ResolveRarity(ScriptableObject data) => data switch
        {
            WeaponDataSO w   => w.Rareza,
            ModifierDataSO m => m.Rareza,
            _                => ItemRarity.Simple
        };

        private string ResolveName(ScriptableObject data) => data switch
        {
            WeaponDataSO w   => w.Nombre,
            ModifierDataSO m => m.Nombre,
            _                => data.name
        };

        private int ResolvePrice(ScriptableObject data) => data switch
        {
            WeaponDataSO w   => w.Precio,
            ModifierDataSO m => m.Precio,
            _                => 0
        };

        private string ResolveDescription(ScriptableObject data)
        {
            if (data is WeaponDataSO w)
            {
                WeaponStats s = w.WeaponStats;
                return $"Daño base: {s.BaseDamage:0.##}\n" +
                       $"Cadencia: {s.FireRate:0.##} disp/s\n" +
                       $"Probabilidad crítico: {s.CriticalChance * 100f:0.##}%\n" +
                       $"Proyectiles por disparo: {s.ProjectilesPerShot}\n" +
                       $"Automática: {(s.IsAutomatic ? "Sí" : "No")}\n" +
                       $"Dispersión base: {s.BaseSpreadAngle:0.##}°\n" +
                       $"Dispersión apuntando: {s.AimingSpreadAngle:0.##}°\n" +
                       $"Cargador: {s.MagazineSize}\n" +
                       $"Tiempo de recarga: {s.ReloadTime:0.##}s";
            }

            if (data is ModifierDataSO m)
            {
                int count = m.Efectos != null ? m.Efectos.Count : 0;
                string tipo = m.TipoDeSlot == SlotType.Upgrade ? "Mejora" : "Accesorio";
                return $"Tipo: {tipo}\nEfectos activos: {count}";
            }

            return string.Empty;
        }
    }
}