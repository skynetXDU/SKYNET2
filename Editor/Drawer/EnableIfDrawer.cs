using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

[CustomPropertyDrawer(typeof(EnableIfAttribute))]
public class EnableIfDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // 处理SDictionary失效
        if(!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
            return;
        
        EnableIfAttribute enableIf = (EnableIfAttribute)attribute;
        bool enabled = ConditionUtil.MatchesCondition(property, enableIf.conditionFieldName, enableIf.expectedValues);

        using (new EditorGUI.DisabledScope(!enabled)) {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // 处理SDictionary失效
        if(!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
            return -EditorGUIUtility.standardVerticalSpacing;
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
}
