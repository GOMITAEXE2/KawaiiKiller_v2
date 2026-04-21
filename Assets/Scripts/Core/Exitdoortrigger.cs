using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using KawaiiKiller.UI;
using KawaiiKiller.UI.Shop;
using KawaiiKiller.Player;
using KawaiiKiller.Core;

namespace KawaiiKiller.Core
{
    public class ExitDoorTrigger : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ShopUIManager         shopUIManager;
        [SerializeField] private InteractPromptUI      promptUI;
        [SerializeField] private PlayerHealth          playerHealth;
        [SerializeField] private PlayerWeaponController weaponController;
        [SerializeField] private PlayerEconomy          playerEconomy;

        [Header("Detection")]
        [SerializeField] private float     interactRadius = 3f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Scene")]
        [SerializeField] private string nextSceneName  = "";
        [SerializeField] private int    nextSceneIndex = -1;
        [SerializeField] private bool   loadByName     = true;

        [Header("Settings")]
        [SerializeField] private string promptReady            = "Presioná E para continuar";
        [SerializeField] private string promptBlocked          = "Cerrá la tienda antes de salir";
        [SerializeField] private float  blockedMessageDuration = 2f;
        [SerializeField] private float  loadDelay              = 0.3f;

        private bool       _playerInRange;
        private Collider[] _overlapBuffer = new Collider[4];
        private Coroutine  _blockedRoutine;
        private Coroutine  _loadRoutine;

        private void Update()
        {
            bool inRange = CheckPlayerInRange();

            if (inRange && !_playerInRange)
                OnPlayerEnter();
            else if (!inRange && _playerInRange)
                OnPlayerExit();

            if (!_playerInRange || !Input.GetKeyDown(KeyCode.E)) return;

            if (shopUIManager != null && shopUIManager.IsOpen)
            {
                ShowBlockedMessage();
                return;
            }

            if (_loadRoutine == null)
                _loadRoutine = StartCoroutine(ExitRoutine());
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
            promptUI?.Show(promptReady);
        }

        private void OnPlayerExit()
        {
            _playerInRange = false;
            promptUI?.Hide();
            if (_blockedRoutine != null) { StopCoroutine(_blockedRoutine); _blockedRoutine = null; }
        }

        private void ShowBlockedMessage()
        {
            if (_blockedRoutine != null) StopCoroutine(_blockedRoutine);
            _blockedRoutine = StartCoroutine(BlockedRoutine());
        }

        private IEnumerator BlockedRoutine()
        {
            promptUI?.Show(promptBlocked);
            yield return new WaitForSecondsRealtime(blockedMessageDuration);
            if (_playerInRange) promptUI?.Show(promptReady);
            _blockedRoutine = null;
        }

        private IEnumerator ExitRoutine()
        {
            promptUI?.Hide();
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, loadDelay));

            playerHealth?.SetInvulnerable(false);

            // Capturar estado antes de cambiar escena
            if (PersistentPlayer.Instance != null && weaponController != null && playerEconomy != null)
                PersistentPlayer.Instance.CaptureState(weaponController, playerEconomy);

            // Persistir en disco
            if (SaveSystem.Instance != null && weaponController != null && playerEconomy != null)
                SaveSystem.Instance.Save(playerEconomy, weaponController);

            int nextRound = WaveManager.Instance != null ? WaveManager.Instance.CurrentRound + 1 : 1;

            if (loadByName && !string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneLoader.Instance.LoadScene(nextSceneName, () =>
                    WaveManager.Instance?.StartRound(nextRound));
            }
            else if (!loadByName && nextSceneIndex >= 0)
            {
                SceneLoader.Instance.LoadScene(SceneUtility.GetScenePathByBuildIndex(nextSceneIndex), () =>
                    WaveManager.Instance?.StartRound(nextRound));
            }
            else
            {
                Debug.LogWarning("[ExitDoorTrigger] No se configuró una escena destino válida.");
            }

            _loadRoutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}