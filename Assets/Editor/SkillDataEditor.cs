using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SkillData), true)]
public class SkillDataEditor : Editor
{
    private SerializedProperty statusEffectsProp;
    private static Dictionary<string, Type> effectTypes;
    private int selectedIndex = -1;

    private void OnEnable()
    {
        statusEffectsProp = serializedObject.FindProperty("statusEffects");

        if (effectTypes == null)
        {
            var baseType = typeof(StatusEffectInfo);
            effectTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))
                .ToDictionary(t => t.Name, t => t);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 기본 스킬 필드들 먼저 출력
        DrawPropertiesExcluding(serializedObject, "statusEffects");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Status Effects (Custom Editor)", EditorStyles.boldLabel);

        if (statusEffectsProp != null)
        {
            for (int i = 0; i < statusEffectsProp.arraySize; i++)
            {
                var element = statusEffectsProp.GetArrayElementAtIndex(i);

                if (element.managedReferenceValue == null)
                {
                    EditorGUILayout.LabelField($"Effect {i}: <null>");
                    continue;
                }

                var effectObj = element.managedReferenceValue as StatusEffectInfo;

                EditorGUILayout.BeginVertical("box");

                // 타입만 읽기 전용으로 표시
                GUI.enabled = false;
                EditorGUILayout.EnumPopup("Type", effectObj.type);
                GUI.enabled = true;

                // 변수 전체 출력
                element.isExpanded = true;
                EditorGUILayout.PropertyField(element, new GUIContent($"Effect {i} Fields"), true);

                if (GUILayout.Button($"Remove Effect {i}"))
                {
                    statusEffectsProp.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Add New Effect", EditorStyles.boldLabel);

        string[] typeNames = effectTypes.Keys.ToArray();
        selectedIndex = EditorGUILayout.Popup("Effect Type", selectedIndex, typeNames);

        if (selectedIndex >= 0 && selectedIndex < typeNames.Length)
        {
            Type selectedType = effectTypes[typeNames[selectedIndex]];
            var instance = Activator.CreateInstance(selectedType) as StatusEffectInfo;

            if (instance != null)
            {
                statusEffectsProp.arraySize++;
                var newElement = statusEffectsProp.GetArrayElementAtIndex(statusEffectsProp.arraySize - 1);
                newElement.managedReferenceValue = instance;
            }

            selectedIndex = -1;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
