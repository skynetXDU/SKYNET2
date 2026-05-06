using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // 处理SDictionary失效
        if(!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
            return;
        
        if(ShouldShow(property))
            EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // 处理SDictionary失效
        if(!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
            return -EditorGUIUtility.standardVerticalSpacing;
        
        if (ShouldShow(property))
            return EditorGUI.GetPropertyHeight(property, label, true);

        return -EditorGUIUtility.standardVerticalSpacing;
    }

    private bool ShouldShow(SerializedProperty property) {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        return ConditionUtil.MatchesCondition(property, showIf.conditionFieldName, showIf.expectedValues);
    }
}
}
