using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 패널")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        HideAll();
    }

    public void ShowPause()
    {
        HideAll();
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ShowGameOver()
    {
        HideAll();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ShowVictory()
    {
        HideAll();
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void HideAll()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }
}
