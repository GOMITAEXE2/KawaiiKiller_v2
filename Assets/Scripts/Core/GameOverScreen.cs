using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button     retryButton;
    [SerializeField] private Button     mainMenuButton;

    private void Awake()
    {
        panel.SetActive(false);
        retryButton.onClick.AddListener(OnRetry);
        mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    public void Show()
    {
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void Hide()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        Hide();
        GameManager.Instance.RestartCurrentMode();
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        Hide();
        GameManager.Instance.GoToMainMenu();
    }
}