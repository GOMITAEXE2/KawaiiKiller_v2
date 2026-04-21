using UnityEngine;
using KawaiiKiller.UI.Shop;

namespace KawaiiKiller.Core
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private ShopUIManager shopUIManager;

        private void Update()
        {
            if (shopUIManager != null && Input.GetKeyDown(KeyCode.P))
            {
                shopUIManager.ToggleShopUI();
            }
            else if (shopUIManager != null && shopUIManager.IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                shopUIManager.CloseShopUI();
            }
        }
    }
}
