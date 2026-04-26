using UnityEditor;

[CustomEditor(typeof(MonsterData))]
public class MonsterDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "isFlying",
            "flyingMoveSpeedMultiplier",
            "flyingWanderRadius",
            "flyingWanderInterval",
            "flyingIdleChance",
            "flyingTraceStopDistance",
            "flyingHeightOffsetRange");

        SerializedProperty isFlyingProp = serializedObject.FindProperty("isFlying");
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(isFlyingProp);

        if (isFlyingProp.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flyingMoveSpeedMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flyingWanderRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flyingWanderInterval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flyingIdleChance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flyingTraceStopDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flyingHeightOffsetRange"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
