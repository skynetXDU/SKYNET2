using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

[CustomPropertyDrawer(typeof(OptionAttribute))]
public sealed class OptionDrawer : PropertyDrawer {
    // Fallback walks Unity's remaining drawer chain. Suppress ALL remaining Option
    // instances for this property, while allowing independent child fields to work.
    private static readonly HashSet<(SerializedObject, string)> FallbackProperties = new();
    private readonly Dictionary<string, float> errorWidths = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var key = (property.serializedObject, property.propertyPath);
        if (FallbackProperties.Contains(key) || fieldInfo == null) {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }
        if (!ShouldShow(property)) return;

        OptionConfiguration configuration = OptionUtil.GetConfiguration(fieldInfo);
        GUIContent displayLabel = GetLabel(label);
        EnableIfAttribute enable = fieldInfo.GetCustomAttribute<EnableIfAttribute>();
        bool enabled = enable == null || ConditionUtil.MatchesCondition(
            OptionUtil.GetConditionOwner(property, fieldInfo), enable.conditionFieldName, enable.expectedValues);

        using (new EditorGUI.DisabledScope(!enabled)) {
            if (!configuration.IsValid) {
                EditorGUI.BeginProperty(position, displayLabel, property);
                try {
                    DrawFallback(position, property, displayLabel, configuration, key);
                }
                finally { EditorGUI.EndProperty(); }
                return;
            }

            OptionGUI.Draw(position, property, displayLabel, configuration);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        var key = (property.serializedObject, property.propertyPath);
        if (FallbackProperties.Contains(key) || fieldInfo == null)
            return EditorGUI.GetPropertyHeight(property, label, true);
        if (!ShouldShow(property)) return -EditorGUIUtility.standardVerticalSpacing;

        OptionConfiguration configuration = OptionUtil.GetConfiguration(fieldInfo);
        if (configuration.IsValid) return EditorGUIUtility.singleLineHeight;

        FallbackProperties.Add(key);
        try {
            return EditorGUI.GetPropertyHeight(property, GetLabel(label), true)
                + EditorGUIUtility.standardVerticalSpacing + ErrorHeight(property, configuration.Error);
        }
        finally { FallbackProperties.Remove(key); }
    }

    private void DrawFallback(Rect position, SerializedProperty property, GUIContent label,
        OptionConfiguration configuration, (SerializedObject, string) key) {
        errorWidths[property.propertyPath] = Mathf.Max(40f, position.width);
        FallbackProperties.Add(key);
        try {
            float height = EditorGUI.GetPropertyHeight(property, label, true);
            Rect fieldRect = new(position.x, position.y, position.width, height);
            EditorGUI.PropertyField(fieldRect, property, label, true);
            Rect errorRect = new(position.x, fieldRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width, ErrorHeight(property, configuration.Error));
            EditorGUI.HelpBox(errorRect, configuration.Error, MessageType.Error);
        }
        finally { FallbackProperties.Remove(key); }
    }

    private float ErrorHeight(SerializedProperty property, string message) {
        float width = errorWidths.TryGetValue(property.propertyPath, out float lastWidth)
            ? lastWidth : Mathf.Max(40f, EditorGUIUtility.currentViewWidth - 60f);
        return Mathf.Max(EditorGUIUtility.singleLineHeight * 2f,
            EditorStyles.helpBox.CalcHeight(new GUIContent(message), Mathf.Max(20f, width - 32f)));
    }

    private bool ShouldShow(SerializedProperty property) {
        ShowIfAttribute show = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
        return show == null || ConditionUtil.MatchesCondition(
            OptionUtil.GetConditionOwner(property, fieldInfo), show.conditionFieldName, show.expectedValues);
    }

    private GUIContent GetLabel(GUIContent label) {
        InspectorLabelAttribute custom = fieldInfo.GetCustomAttribute<InspectorLabelAttribute>();
        // A collection's label belongs to its header, never to each element.
        return custom != null && !string.IsNullOrEmpty(label.text)
            && OptionUtil.GetValueType(fieldInfo.FieldType) == fieldInfo.FieldType
            ? new GUIContent(label) { text = custom.Label } : label;
    }
}
}
