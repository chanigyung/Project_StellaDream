using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneLoader : MonoBehaviour
{
    public static GameSceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName, System.Action onComplete = null)
    {
        GameController.Instance.ChangeState(GameState.Loading);
        StartCoroutine(LoadSceneRoutine(sceneName, onComplete));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, System.Action onComplete)
    {
        // 로딩 UI 띄우기
        if (LoadingUI.Instance != null)
            LoadingUI.Instance.Show("로딩 중...");

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        yield return new WaitForSeconds(1.3f);

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        if (LoadingUI.Instance != null)
            LoadingUI.Instance.Hide();

        onComplete?.Invoke();
    }
}
