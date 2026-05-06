using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

public class SerializedPropertyWindow : EditorWindow {

    private UnityEngine.Object[] targets;
    private string propertyPath;
    private string propertyLabel;

    private Vector2 scrollPosition;

    public static void Open(SerializedProperty property, GUIContent label) {
        SerializedPropertyWindow window = CreateInstance<SerializedPropertyWindow>();

        window.targets = property.serializedObject.targetObjects;
        window.propertyPath = property.propertyPath;
        window.propertyLabel = label.text;

        window.titleContent = new GUIContent(label.text);
        window.minSize = new Vector2(500f, 300f);
        window.Show();
    }

    private void OnGUI() {
        if (targets == null || targets.Length == 0) {
            EditorGUILayout.HelpBox("目标对象不存在。", MessageType.Warning);
            return;
        }

        SerializedObject serializedObject = new(targets);

        serializedObject.Update();

        SerializedProperty property = serializedObject.FindProperty(propertyPath);

        if (property == null) {
            EditorGUILayout.HelpBox($"找不到字段：{propertyPath}", MessageType.Error);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.PropertyField(
            property,
            new GUIContent(propertyLabel),
            true
        );

        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }
}
}
