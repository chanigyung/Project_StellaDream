using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }

    [Header("루트 패널")]
    public GameObject rootObject;

    [Header("로딩 텍스트")]
    public Text loadingText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Hide(); // 기본적으로 비활성화
    }

    public void Show(string message = "L o a d i n g ...")
    {
        if (rootObject != null) rootObject.SetActive(true);
        if (loadingText != null) loadingText.text = message;
    }

    public void Hide()
    {
        if (rootObject != null) rootObject.SetActive(false);
    }
}
