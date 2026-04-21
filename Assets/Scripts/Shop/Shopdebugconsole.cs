using TMPro;
using UnityEngine;
using KawaiiKiller.UI.Shop;

namespace KawaiiKiller.Debugging
{
    public class ShopDebugConsole : MonoBehaviour
    {
        [SerializeField] private ShopUIManager shopUIManager;

        [Header("Panels")]
        [SerializeField] private TMP_Text lastTransactionText;
        [SerializeField] private TMP_Text economyText;
        [SerializeField] private TMP_Text rerollText;
        [SerializeField] private TMP_Text slotsText;
        [SerializeField] private TMP_Text feedbackText;

        private void OnEnable()
        {
            if (shopUIManager == null) return;
            shopUIManager.OnDebugTransaction  += HandleTransaction;
            shopUIManager.OnDebugEconomy      += HandleEconomy;
            shopUIManager.OnDebugReroll       += HandleReroll;
            shopUIManager.OnDebugSlots        += HandleSlots;
            shopUIManager.OnDebugFeedback     += HandleFeedback;
        }

        private void OnDisable()
        {
            if (shopUIManager == null) return;
            shopUIManager.OnDebugTransaction  -= HandleTransaction;
            shopUIManager.OnDebugEconomy      -= HandleEconomy;
            shopUIManager.OnDebugReroll       -= HandleReroll;
            shopUIManager.OnDebugSlots        -= HandleSlots;
            shopUIManager.OnDebugFeedback     -= HandleFeedback;
        }

        private void HandleTransaction(string msg)
        {
            if (lastTransactionText != null) lastTransactionText.text = $"TRANSACCIÓN: {msg}";
        }

        private void HandleEconomy(int current, int delta)
        {
            if (economyText != null) economyText.text = $"DINERO: {current} (Δ {delta:+#;-#;0})";
        }

        private void HandleReroll(int cost, int count)
        {
            if (rerollText != null) rerollText.text = $"REROLL #{count} | Costo: {cost}";
        }

        private void HandleSlots(int visible)
        {
            if (slotsText != null) slotsText.text = $"SLOTS TIENDA VISIBLES: {visible}";
        }

        private void HandleFeedback(string msg)
        {
            if (feedbackText != null) feedbackText.text = $"FEEDBACK: {msg}";
        }
    }
}