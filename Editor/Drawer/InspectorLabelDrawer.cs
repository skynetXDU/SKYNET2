using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

[CustomPropertyDrawer(typeof(InspectorLabelAttribute))]
public class InspectorLabelDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // 处理SDictionary失效
        if(!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
            return;
        
        // 如果CustomPropertyDrawer的参数是某一个Attribute, 那么这个attribute就会是这个类型的Attribute
        InspectorLabelAttribute la = (InspectorLabelAttribute)attribute;

        if(label.text == "") {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }
        
        GUIContent newLabel = new(la.Label);
        
        EditorGUI.PropertyField(position, property, newLabel, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 处理SDictionary失效
        if(!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
            return -EditorGUIUtility.standardVerticalSpacing;
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
}
