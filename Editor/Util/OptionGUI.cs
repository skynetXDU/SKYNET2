using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

/// <summary>Shared serialized single-selection controls for fields and dictionary cells.</summary>
internal static class OptionGUI {
    internal static void Draw(Rect position, SerializedProperty property, GUIContent label,
        OptionConfiguration configuration) {
        if (!configuration.IsValid) return;
        EditorGUI.BeginProperty(position, label, property);
        bool oldMixed = EditorGUI.showMixedValue;
        try {
            bool mixed = property.hasMultipleDifferentValues;
            EditorGUI.showMixedValue = mixed;
            int selected = configuration.FindSelected(
                OptionUtil.ReadValue(property, configuration.ValueType), mixed);
            Rect buttons = EditorGUI.PrefixLabel(position, label);
            EditorGUI.BeginChangeCheck();
            int chosen = GUI.Toolbar(buttons, selected, configuration.Labels, EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck() && chosen >= 0)
                OptionUtil.WriteValue(property, configuration, chosen);
        }
        finally {
            EditorGUI.showMixedValue = oldMixed;
            EditorGUI.EndProperty();
        }
    }
}
}
