using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public partial class GameController
{
    // 로딩 UI 포함 비동기 씬 전환 처리
    public void LoadSceneWithLoading(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        ChangeState(GameState.Loading);

        // 로딩 UI 표시
        if (LoadingUI.Instance != null)
            LoadingUI.Instance.Show("L o a d i n g ...");

        // 씬 비동기 로딩
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; //씬 전환 금지

        while (op.progress < 0.9f) //씬 로딩 90%까지 대기
            yield return null;

        yield return new WaitForSeconds(1.3f); // 로딩창 보여주기용 짧은 대기 연출

        op.allowSceneActivation = true; //씬 전환 허용

        while (!op.isDone) //씬 완전 전환될때까지 대기
            yield return null;

        // 로딩 UI 끄기
        if (LoadingUI.Instance != null)
            LoadingUI.Instance.Hide();
    }
}
