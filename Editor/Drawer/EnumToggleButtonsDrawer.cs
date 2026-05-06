using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {


    [CustomPropertyDrawer(typeof(EnumToggleButtonsAttribute))]
    public class EnumToggleButtonsDrawer : PropertyDrawer {

        private readonly struct FlagButton {

            public readonly string Label;
            public readonly int Value;

            public FlagButton(string label, int value) {
                Label = label;
                Value = value;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // 处理SDictionary失效
            if (!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
                return;
            // 处理ShowIf
            ShowIfAttribute sa = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
            if (sa != null && !ConditionUtil.MatchesCondition(property, sa.conditionFieldName, sa.expectedValues)) {
                return;
            }
            // 处理EnableIf
            bool oldEnabled = GUI.enabled;
            EnableIfAttribute ea = fieldInfo.GetCustomAttribute<EnableIfAttribute>();
            if (ea != null && !ConditionUtil.MatchesCondition(property, ea.conditionFieldName, ea.expectedValues)) {
                GUI.enabled = false;
            }

            // 不是Enum
            if (property.propertyType != SerializedPropertyType.Enum) {
                EditorGUI.PropertyField(position, property, label, true);
                // 处理EnableIf
                if (ea != null && !ConditionUtil.MatchesCondition(property, ea.conditionFieldName, ea.expectedValues)) {
                    GUI.enabled = oldEnabled;
                }
                return;
            }

            // 处理InspectorLabel
            InspectorLabelAttribute la = fieldInfo.GetCustomAttribute<InspectorLabelAttribute>();
            // 这里还是那个大坑, property和fieldInfo不对应
            // 防止把InspectorLabel带到列表每个元素上
            if (fieldInfo.FieldType.IsEnum && la != null && label.text != "")
                label.text = la.Label;

            EditorGUI.BeginProperty(position, label, property);

            Rect buttonRect = EditorGUI.PrefixLabel(position, label);

            Type enumType = GetEnumType();

            if (enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                DrawFlagsEnum(buttonRect, property);
            else
                DrawNormalEnum(buttonRect, property);

            // 处理EnableIf
            if (ea != null && !ConditionUtil.MatchesCondition(property, ea.conditionFieldName, ea.expectedValues)) {
                GUI.enabled = oldEnabled;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // 处理SDictionary失效
            if (!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
                return -EditorGUIUtility.standardVerticalSpacing;
            // 处理ShowIf
            ShowIfAttribute sa = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
            if (sa != null && !ConditionUtil.MatchesCondition(property, sa.conditionFieldName, sa.expectedValues)) {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            return property.propertyType == SerializedPropertyType.Enum
                ? EditorGUIUtility.singleLineHeight
                : EditorGUI.GetPropertyHeight(property, label, true);
        }

        private Type GetEnumType() {
            // 这里还是同样的大坑, fieldInfo和property不对应
            Type enumType = fieldInfo.FieldType;
            if (enumType.IsArray && enumType.GetArrayRank() == 1)
                enumType = enumType.GetElementType();
            else if (enumType.IsGenericType && enumType.GetGenericTypeDefinition() == typeof(List<>))
                enumType = enumType.GetGenericArguments()[0];

            return enumType;
        }

        // 画普通枚举
        private void DrawNormalEnum(Rect rect, SerializedProperty property) {
            string[] names = property.enumDisplayNames.Length > 0
                ? property.enumDisplayNames
                : property.enumNames;

            if (names.Length == 0) {
                EditorGUI.PropertyField(rect, property, GUIContent.none, true);
                return;
            }

            int selectedIndex = Mathf.Clamp(property.enumValueIndex, 0, names.Length - 1);
            int newIndex = GUI.Toolbar(rect, selectedIndex, names);

            if (newIndex != selectedIndex)
                property.enumValueIndex = newIndex;
        }

        // 能多选的枚举
        private void DrawFlagsEnum(Rect rect, SerializedProperty property) {

            List<FlagButton> buttons = new();

            Type enumType = GetEnumType();
            Array values = Enum.GetValues(enumType);

            foreach (object value in values) {
                int numericValue = Convert.ToInt32(value);

                string stringValue = value.ToString();
                string name = ObjectNames.NicifyVariableName(stringValue);
                FieldInfo field = enumType.GetField(stringValue);
                InspectorNameAttribute na = field?.GetCustomAttribute<InspectorNameAttribute>();
                if (na != null)
                    name = na.displayName;

                if (numericValue == 0 || IsSingleBit(numericValue))
                    buttons.Add(new FlagButton(name, numericValue));
            }

            if (buttons.Count == 0) {
                EditorGUI.PropertyField(rect, property, GUIContent.none, true);
                return;
            }

            int currentValue = property.enumValueFlag;
            float buttonWidth = rect.width / buttons.Count;

            for (int i = 0; i < buttons.Count; ++i) {
                FlagButton button = buttons[i];
                Rect buttonRect = new(rect.x + buttonWidth * i, rect.y, buttonWidth, rect.height);
                bool isSelected = button.Value == 0
                    ? currentValue == 0
                    : (currentValue & button.Value) == button.Value;

                EditorGUI.BeginChangeCheck();
                bool newSelected = GUI.Toggle(buttonRect, isSelected, button.Label, EditorStyles.miniButton);
                if (!EditorGUI.EndChangeCheck())
                    continue;

                if (button.Value == 0) {
                    if (newSelected)
                        currentValue = 0;
                }
                else if (newSelected) {
                    currentValue |= button.Value;
                }
                else {
                    currentValue &= ~button.Value;
                }
            }

            property.enumValueFlag = currentValue;
        }

        private bool IsSingleBit(int value) {
            return value > 0 && (value & (value - 1)) == 0;
        }
    }
}
