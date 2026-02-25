#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeStartSceneSetter
{
    private const string BootScenePath = "Assets/Scenes/BootScene.unity";

    static PlayModeStartSceneSetter()
    {
        var bootScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath);

        if (bootScene == null)
        {
            // BootScene 경로가 틀리면 여기서 바로 로그로 알려줌
            UnityEngine.Debug.LogError($"[PlayModeStartSceneSetter] BootScene을 찾을 수 없음: {BootScenePath}");
            return;
        }

        EditorSceneManager.playModeStartScene = bootScene;
    }
}
#endif