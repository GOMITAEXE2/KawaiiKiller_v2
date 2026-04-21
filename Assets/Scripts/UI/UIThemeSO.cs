using UnityEngine;
using TMPro;

namespace KawaiiKiller.UI.Core
{
    [CreateAssetMenu(fileName = "UITheme", menuName = "KawaiiKiller/UI/Theme")]
    public class UIThemeSO : ScriptableObject
    {
        [Header("Backgrounds")]
        public Color PrimaryBackground   = new Color(0.051f, 0.039f, 0.078f, 1f);
        public Color SecondaryBackground = new Color(0.102f, 0.071f, 0.157f, 1f);

        [Header("Interactables")]
        public Color PrimaryInteractable = new Color(1f,    0.122f, 0.431f, 1f);
        public Color HoverInteractable   = new Color(1f,    0.435f, 0.659f, 1f);
        public Color ClickInteractable   = new Color(0.769f, 0f,   0.310f, 1f);

        [Header("Item Outlines")]
        public Color OutlineWeapon   = new Color(0.420f, 0.373f, 1f,    1f);
        public Color OutlineModifier = new Color(0f,     0.910f, 0.588f, 1f);

        [Header("Rarity Accents")]
        public Color RaritySimple     = new Color(0.361f, 0.353f, 0.333f, 1f);
        public Color RarityRara       = new Color(0.420f, 0.373f, 1f,    1f);
        public Color RarityExtrañа   = new Color(1f,     0.122f, 0.431f, 1f);
        public Color RarityLegendaria = new Color(1f,    0.816f, 0f,     1f);

        [Header("Text")]
        public Color TextMain      = new Color(1f,    0.722f, 0.824f, 1f);
        public Color TextSecondary = new Color(0.361f, 0.353f, 0.333f, 1f);

        [Header("Semantic")]
        public Color ColorConfirm = new Color(0.224f, 1f,    0.078f, 1f);
        public Color ColorCancel  = new Color(1f,    0.122f, 0.431f, 1f);

        [Header("Typography")]
        public TMP_FontAsset MainFont;
    }
}
