using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KawaiiKiller.UI.Core
{
    public class UIThemeApplicator : MonoBehaviour
    {
        public enum UIElementType
        {
            BackgroundPrimary,
            BackgroundSecondary,
            ButtonPrimary,
            TextMain,
            TextSecondary,
            OutlineWeapon,
            OutlineModifier
        }

        [SerializeField] private UIThemeSO    currentTheme;
        [SerializeField] private UIElementType elementType;

        private void Awake() => ApplyTheme();

        public void ApplyTheme()
        {
            if (currentTheme == null) return;

            if (TryGetComponent<Image>(out var image))
                image.color = ResolveColor();

            if (TryGetComponent<TextMeshProUGUI>(out var tmp))
            {
                tmp.color = ResolveColor();
                if (currentTheme.MainFont != null &&
                    (elementType == UIElementType.TextMain || elementType == UIElementType.TextSecondary))
                    tmp.font = currentTheme.MainFont;
            }
        }

        private Color ResolveColor() => elementType switch
        {
            UIElementType.BackgroundPrimary   => currentTheme.PrimaryBackground,
            UIElementType.BackgroundSecondary => currentTheme.SecondaryBackground,
            UIElementType.ButtonPrimary       => currentTheme.PrimaryInteractable,
            UIElementType.TextMain            => currentTheme.TextMain,
            UIElementType.TextSecondary       => currentTheme.TextSecondary,
            UIElementType.OutlineWeapon       => currentTheme.OutlineWeapon,
            UIElementType.OutlineModifier     => currentTheme.OutlineModifier,
            _                                 => Color.white
        };
    }
}
