using UnityEngine;
using KawaiiKiller.UI;
using KawaiiKiller.UI.Shop;
using KawaiiKiller.Player;

namespace KawaiiKiller.Core
{
    public class ShopAccessTrigger : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ShopUIManager    shopUIManager;
        [SerializeField] private InteractPromptUI promptUI;
        [SerializeField] private PlayerHealth     playerHealth;

        [Header("Detection")]
        [SerializeField] private float     interactRadius = 3f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Settings")]
        [SerializeField] private string promptMessage = "Presioná E para abrir la tienda";

        private bool       _playerInRange;
        private Collider[] _overlapBuffer = new Collider[4];

        private void Awake()
        {
            ResolveDependencies();
        }

        private void ResolveDependencies()
        {
            if (shopUIManager == null) shopUIManager = FindObjectOfType<ShopUIManager>();
            if (promptUI == null)      promptUI      = FindObjectOfType<InteractPromptUI>();
            if (playerHealth == null)  playerHealth  = FindObjectOfType<PlayerHealth>();

            if (playerHealth == null)
            {
                Debug.LogError("[ShopAccessTrigger] CRITICAL ERROR: PlayerHealth dependency not found in scene!");
            }
        }

        private void Update()
        {
            bool inRange = CheckPlayerInRange();

            if (inRange && !_playerInRange)
                OnPlayerEnter();
            else if (!inRange && _playerInRange)
                OnPlayerExit();

            if (_playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                if (shopUIManager == null) return;
                shopUIManager.NotifyShopEntered();
                shopUIManager.OpenShopUI();
            }
        }

        private bool CheckPlayerInRange()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, interactRadius, _overlapBuffer, playerLayer);
            return count > 0;
        }

        private void OnPlayerEnter()
        {
            _playerInRange = true;
            promptUI?.Show(promptMessage);
            playerHealth?.SetInvulnerable(true);
        }

        private void OnPlayerExit()
        {
            _playerInRange = false;
            promptUI?.Hide();

            if (shopUIManager != null && shopUIManager.IsOpen)
                shopUIManager.CloseShopUI();

            playerHealth?.SetInvulnerable(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}