using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

/// <summary>
/// 把List<ElementType>画成表格 <br/>
/// </summary>
[CustomPropertyDrawer(typeof(TableListAttribute))]
public class TableListDrawer : PropertyDrawer {

    private const float SpacingX = 6f;
    private const float SpacingY = 4f;
    private const float IndexColumnWidth = 34f;
    private const float DeleteButtonWidth = 24f;
    private const float ButtonHeight = 24f;

    private static GUIStyle headerStyle;

    private static GUIStyle HeaderStyle {
        get {
            headerStyle ??= new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter
            };
            return headerStyle;
        }
    }
    // 需要一并处理ShowIf、EnableIf和InspectorLabel, 因为TableList不会把绘制命令继续转发下去
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // 处理SDictionary失效
        if(!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
            return;
        
        // 处理ShowIf
        ShowIfAttribute sa = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
        if(sa != null && !ConditionUtil.MatchesCondition(property, sa.conditionFieldName, sa.expectedValues)) {
            return;
        }

        // 处理EnableIf
        bool oldEnabled = GUI.enabled;
        EnableIfAttribute ea = fieldInfo.GetCustomAttribute<EnableIfAttribute>();
        if(ea != null && !ConditionUtil.MatchesCondition(property, ea.conditionFieldName, ea.expectedValues)) {
            GUI.enabled = false;
        }

        if (!CanDrawTable(property)) {
            EditorGUI.PropertyField(position, property, label, true);
            // 处理EnableIf
            if(ea != null && !ConditionUtil.MatchesCondition(property, ea.conditionFieldName, ea.expectedValues)) {
                GUI.enabled = oldEnabled;
            }
            return;
        }
        
        // 处理InspectorLabel
        InspectorLabelAttribute la = fieldInfo.GetCustomAttribute<InspectorLabelAttribute>();
        if(la != null && label.text != "")
            label.text = la.Label;

        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float y = position.y;

        Rect foldoutRect = new(position.x, y, position.width, lineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        y += lineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded) {
            // 先拿到数组或列表元素的类型
            Type elementType = GetElementType(fieldInfo.FieldType);
            // 再去这个类型里找可序列化的字段
            List<FieldInfo> fields = GetSerializableFields(elementType, out bool drawFields);

            DrawHeader(position, ref y, fields, drawFields);

            for (int i = 0; i < property.arraySize; ++i) {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                float rowHeight = GetRowHeight(element, fields, drawFields);

                bool deleted = DrawRow(position, y, rowHeight, property, element, fields, drawFields, i);
                if (deleted)
                    break;

                y += rowHeight + SpacingY;
            }

            DrawBottomButtons(position, y, property, property, label);
        }

        // 处理EnableIf
        if(ea != null && !ConditionUtil.MatchesCondition(property, ea.conditionFieldName, ea.expectedValues)) {
            GUI.enabled = oldEnabled;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // 处理SDictionary失效
        if(!SkynetEditorUtil.IsUnitySerializableType(fieldInfo.FieldType))
            return -EditorGUIUtility.standardVerticalSpacing;
        
        // 处理ShowIf
        ShowIfAttribute sa = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
        if(sa != null && !ConditionUtil.MatchesCondition(property, sa.conditionFieldName, sa.expectedValues)) {
            return -EditorGUIUtility.standardVerticalSpacing;
        }

        if (!CanDrawTable(property))
            return EditorGUI.GetPropertyHeight(property, label, true);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float height = lineHeight;

        if (!property.isExpanded)
            return height;

        Type elementType = GetElementType(fieldInfo.FieldType);
        List<FieldInfo> fields = GetSerializableFields(elementType, out bool drawFields);

        height += EditorGUIUtility.standardVerticalSpacing;
        height += lineHeight + SpacingY;

        for (int i = 0; i < property.arraySize; ++i) {
            SerializedProperty element = property.GetArrayElementAtIndex(i);
            height += GetRowHeight(element, fields, drawFields) + SpacingY;
        }

        height += ButtonHeight;
        return height;
    }

    // 画成表格的条件: 数组或List类型
    private bool CanDrawTable(SerializedProperty property) {
        return fieldInfo != null
            && property.isArray
            && property.propertyType != SerializedPropertyType.String
            && GetElementType(fieldInfo.FieldType) != null;
    }

    // 拿到数组元素的类型
    private Type GetElementType(Type propertyType) {
        // 如果是数组
        if (propertyType.IsArray)
            return propertyType.GetElementType();
        // 如果是列表泛型
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            return propertyType.GetGenericArguments()[0];
        return null;
    }

    // 从数组元素的字段里找unity序列化字段
    private List<FieldInfo> GetSerializableFields(Type elementType, out bool drawFields) {
        List<FieldInfo> fields = new();

        // 如果列表元素是基元类型或unity对象, 那么不展开字段
        if (elementType.IsEnum
            || elementType.IsPrimitive
            || elementType == typeof(decimal)
            || elementType == typeof(string)
            || typeof(UnityEngine.Object).IsAssignableFrom(elementType)){
            
            drawFields = false;
            return fields;
        }

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo[] allFields = elementType.GetFields(flags);

        foreach (FieldInfo field in allFields) {
            if (!SkynetEditorUtil.IsUnitySerializedField(field))
                continue;
            fields.Add(field);
        }
        drawFields = true;
        return fields;
    }

    private string GetColumnName(FieldInfo field) {
        TableNameAttribute tn = field.GetCustomAttribute<TableNameAttribute>();
        InspectorLabelAttribute il = field.GetCustomAttribute<InspectorLabelAttribute>();
        if(tn != null && !string.IsNullOrEmpty(tn.Name))
            return tn.Name;
        if(il != null && !string.IsNullOrEmpty(il.Label))
            return il.Label;
        return field.Name;
    }

    private void DrawHeader(Rect rect, ref float y, List<FieldInfo> fields, bool drawFields) {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        Rect indexRect = new(rect.x, y, IndexColumnWidth, lineHeight);
        EditorGUI.LabelField(indexRect, "#", HeaderStyle);

        Rect[] columnRects = GetColumnRects(rect, y, lineHeight, Math.Max(fields.Count, 1));

        if (drawFields) {
            for (int i = 0; i < fields.Count; ++i)
                EditorGUI.LabelField(columnRects[i], GetColumnName(fields[i]), HeaderStyle);
        }
        else {
            EditorGUI.LabelField(columnRects[0], "value", HeaderStyle);
        }

        y += lineHeight + SpacingY;
    }

    private bool DrawRow(Rect rect,
        float y,
        float rowHeight,
        SerializedProperty arrayProperty, // 这个是要画成表格的列表字段, 也就是TableList所在的字段
        SerializedProperty element,       // 这个是列表里的单个元素
        List<FieldInfo> fields,
        bool drawFields,
        int index
    ) {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        // 画行号
        Rect indexRect = new(rect.x, y + (rowHeight - lineHeight) * 0.5f, IndexColumnWidth, lineHeight);
        EditorGUI.LabelField(indexRect, index.ToString(), HeaderStyle);

        // 画列
        Rect[] columnRects = GetColumnRects(rect, y, rowHeight, Math.Max(fields.Count, 1));

        if (drawFields) {
            for (int i = 0; i < fields.Count; ++i) {
                SerializedProperty child = element.FindPropertyRelative(fields[i].Name);
                DrawCell(columnRects[i], child, fields[i]);
            }
        }
        else {
            DrawCell(columnRects[0], element, null);
        }

        Rect deleteRect = new(rect.xMax - DeleteButtonWidth, y + (rowHeight - lineHeight) * 0.5f, DeleteButtonWidth, lineHeight);

        if (GUI.Button(deleteRect, "X")) {
            int oldSize = arrayProperty.arraySize;
            arrayProperty.DeleteArrayElementAtIndex(index);

            if (arrayProperty.arraySize == oldSize)
                arrayProperty.DeleteArrayElementAtIndex(index);
            return true;
        }

        return false;
    }

    private void DrawBottomButtons(Rect rect, float y, SerializedProperty arrayProperty, SerializedProperty property, GUIContent label) {
        float buttonWidth = (rect.width - SpacingX) * 0.5f;

        Rect addButtonRect = new(rect.x, y, buttonWidth, ButtonHeight);

        Rect openButtonRect = new(addButtonRect.xMax + SpacingX, y, buttonWidth, ButtonHeight);

        if (GUI.Button(addButtonRect, "+ 添加一行")) {
            int newIndex = arrayProperty.arraySize;
            arrayProperty.arraySize++;

            SerializedProperty newElement = arrayProperty.GetArrayElementAtIndex(newIndex);
            PropertyUtil.ClearProperty(newElement);
        }

        if (GUI.Button(openButtonRect, "在独立窗口中打开")) {
            SerializedPropertyWindow.Open(property, label);
        }
    }

    private void DrawCell(Rect rect, SerializedProperty property, FieldInfo fi) {

        float height = EditorGUI.GetPropertyHeight(property, GUIContent.none, true);

        if (height <= 0f)
            return;

        if (property.propertyType == SerializedPropertyType.Boolean) {
            // 处理ShowIf
            ShowIfAttribute sa = fi?.GetCustomAttribute<ShowIfAttribute>();
            if(sa != null && !ConditionUtil.MatchesCondition(property, sa.conditionFieldName, sa.expectedValues)) {
                return;
            }
            // 处理EnableIf
            bool oldEnabled = GUI.enabled;
            EnableIfAttribute ea = fi?.GetCustomAttribute<EnableIfAttribute>();
            bool flag = ea != null && !ConditionUtil.MatchesCondition(property, ea.conditionFieldName, ea.expectedValues);
            if(flag) {
                GUI.enabled = false;
            }
            // 针对bool字段的单独优化, 让复选框在中间位置
            float size = EditorGUIUtility.singleLineHeight;
            Rect toggleRect = new(rect.center.x - size * 0.5f, rect.center.y - size * 0.5f, size, size);
            EditorGUI.BeginChangeCheck();
            bool value = EditorGUI.Toggle(toggleRect, property.boolValue);
            if (EditorGUI.EndChangeCheck())
                property.boolValue = value;
            if(flag)
                GUI.enabled = oldEnabled;
            return;
        }

        Rect fieldRect = new(rect.x, rect.y + (rect.height - height) * 0.5f, rect.width, height);

        // 这里会触发字段本身的ShowIf和EnableIf
        EditorGUI.PropertyField(fieldRect, property, GUIContent.none, true);
    }

    private Rect[] GetColumnRects(Rect rect, float y, float height, int columnCount) {

        float tableX = rect.x + IndexColumnWidth + SpacingX;
        float tableWidth = rect.width - IndexColumnWidth - DeleteButtonWidth - SpacingX * 2f;
        float columnWidth = (tableWidth - SpacingX * (columnCount - 1)) / columnCount;

        Rect[] columnRects = new Rect[columnCount];

        for (int i = 0; i < columnCount; ++i)
            columnRects[i] = new Rect(tableX + (columnWidth + SpacingX) * i, y, columnWidth, height);

        return columnRects;
    }

    private float GetRowHeight(SerializedProperty element, List<FieldInfo> fields, bool drawFields) {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float rowHeight = lineHeight;

        if (drawFields) {
            foreach (FieldInfo field in fields) {
                SerializedProperty child = element.FindPropertyRelative(field.Name);
                rowHeight = Mathf.Max(rowHeight, EditorGUI.GetPropertyHeight(child, GUIContent.none, true));
            }
        }
        else
            rowHeight = Mathf.Max(rowHeight, EditorGUI.GetPropertyHeight(element, GUIContent.none, true));

        return rowHeight;
    }
}
}
