using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

[CustomPropertyDrawer(typeof(SDictionary<,>))]
public class SDictionaryDrawer : PropertyDrawer {

    private static GUIStyle headerStyle;

    private static GUIStyle HeaderStyle {
        get {
            headerStyle ??= new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter
            };
            return headerStyle;
        }
    }

    private const float SpacingX = 15f;
    private const float SpacingY = 6f;
    private const float DeleteButtonWidth = 24f;
    private const float ButtonHeight = 24f;
    private const float ButtonSpacing = 4f;

    private const float DefaultSplitCenterRatio = 0.4f;
    private const float SplitterHitWidth = 6f;
    private const float SplitterWidth = 1f;
    private const float MinColumnWidth = 50f;

    private static readonly Dictionary<string, float> SplitCenterLocalXRatio = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (!SDictionaryUtil.CanDrawDictionary(property, fieldInfo))
            return;

        EditorGUI.BeginProperty(position, label, property);

        Rect rect = position;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float y = rect.y;

        Rect foldoutRect = new(rect.x, y, rect.width, lineHeight);

        property.isExpanded = EditorGUI.Foldout(
            foldoutRect,
            property.isExpanded,
            label,
            true
        );

        y += lineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded) {
            SerializedProperty pairsProp = property.FindPropertyRelative("pairs");

            string splitKey = $"{property.serializedObject.targetObject.GetEntityId()}:{property.propertyPath}";

            //===========处理分隔线================
            float tableWidth = rect.width - DeleteButtonWidth - SpacingX;

            float splitCenterLocalX = tableWidth * (SplitCenterLocalXRatio.TryGetValue(splitKey, out float savedSplitCenterRatio)
                                    ? savedSplitCenterRatio
                                    : DefaultSplitCenterRatio);
            // Debug.Log(
            //     $"event={Event.current.type}, raw={Event.current.rawType}, " +
            //     $"rect.width={rect.width}, tableWidth={tableWidth}, path={property.propertyPath}"
            // );
            splitCenterLocalX = ClampSplitCenterLocalX(splitCenterLocalX, tableWidth);
            // 这里必须删除, 原理大致是EventType为Layout和Repaint时, 传进来的posotion不一样, 如果这里写SplitCenterLocalXs, 就会出现一些奇怪的问题
            // SplitCenterLocalXs[splitKey] = splitCenterLocalX;

            HandleSplitter(rect, splitKey, tableWidth, y, ref splitCenterLocalX);

            //==========处理完分隔线, 开始绘制=========

            DrawHeader(rect, ref y, splitCenterLocalX);

            for (int i = 0; i < pairsProp.arraySize; i++) {
                SerializedProperty pairProp = pairsProp.GetArrayElementAtIndex(i);

                bool deleted = DrawPairRow(rect, ref y, pairProp, pairsProp, i, splitCenterLocalX);

                if (deleted) break;
            }

            DrawAddButton(rect, y, pairsProp, property, label);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (!SDictionaryUtil.CanDrawDictionary(property, fieldInfo))
            return -EditorGUIUtility.standardVerticalSpacing;

        float lineHeight = EditorGUIUtility.singleLineHeight;

        float height = lineHeight; // 字段名占一行

        if (!property.isExpanded) { // 不展开
            return height;
        }

        // 字段名和表头之间的间隔
        height += SpacingY;

        SerializedProperty pairsProp = property.FindPropertyRelative("pairs");
        // 表头以及表头和第一行之间的间隔
        height += lineHeight + SpacingY;
        // 表格的所有列
        for (int i = 0; i < pairsProp.arraySize; i++) {
            SerializedProperty pairProp = pairsProp.GetArrayElementAtIndex(i);

            SerializedProperty keyProp = pairProp.FindPropertyRelative("key");
            SerializedProperty valueProp = pairProp.FindPropertyRelative("value");

            float keyHeight = EditorGUI.GetPropertyHeight(keyProp, true);
            float valueHeight = EditorGUI.GetPropertyHeight(valueProp, true);

            height += Mathf.Max(lineHeight, keyHeight, valueHeight) + SpacingY;
        }
        // 最后是添加按钮本身的高度
        height += ButtonHeight;

        return height;
    }

    // 把泛型从List`1这种写成List<ElementType>这种更易读的形式
    private string GetFriendlyTypeName(Type type) {
        if (type == typeof(int)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(string)) return "string";
        if (type == typeof(bool)) return "bool";
        if (!type.IsGenericType) return type.Name;

        string typeName = type.Name;
        int index = typeName.IndexOf("`");
        if (index >= 0)
            typeName = typeName[..index];

        Type[] args = type.GetGenericArguments();
        string[] argNames = new string[args.Length];

        for (int k = 0; k < args.Length; ++k)
            argNames[k] = GetFriendlyTypeName(args[k]);

        return $"{typeName}<{string.Join(", ", argNames)}>";
    }

    //============================
    //          绘制相关         ||
    //============================
    private void DrawHeader(Rect rect, ref float y, float splitCenterLocalX) {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        float tableWidth = rect.width - DeleteButtonWidth - SpacingX;
        float keyWidth = splitCenterLocalX - SpacingX * 0.5f;
        float valueWidth = tableWidth - keyWidth - SpacingX;

        Rect keyHeaderRect = new(rect.x, y, keyWidth, lineHeight);

        Rect valueHeaderRect = new(keyHeaderRect.xMax + SpacingX, y, valueWidth, lineHeight);

        string keyLabel = "键", valueLabel = "值";

        SDictionaryLabelAttribute sl = fieldInfo.GetCustomAttribute<SDictionaryLabelAttribute>();
        if(sl != null) {
            if(sl.KeyLabel != "") keyLabel = sl.KeyLabel;
            if(sl.ValueLabel != "") valueLabel = sl.ValueLabel;
        }
        
        Type[] args = SDictionaryUtil.GetSDictArgs(fieldInfo);
        string keyType = GetFriendlyTypeName(args[0]);
        string valueType = GetFriendlyTypeName(args[1]);

        EditorGUI.LabelField(keyHeaderRect, $"{keyLabel}({keyType})", HeaderStyle);
        EditorGUI.LabelField(valueHeaderRect, $"{valueLabel}({valueType})", HeaderStyle);

        y += lineHeight + SpacingY;
    }

    private bool DrawPairRow(
        Rect rect,
        ref float y,
        SerializedProperty pairProp,
        SerializedProperty pairsProp,
        int index,
        float splitCenterLocalX
    ) {
        SerializedProperty keyProp = pairProp.FindPropertyRelative("key");
        SerializedProperty valueProp = pairProp.FindPropertyRelative("value");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float keyHeight = EditorGUI.GetPropertyHeight(keyProp, true);
        float valueHeight = EditorGUI.GetPropertyHeight(valueProp, true);

        float rowHeight = Mathf.Max(lineHeight, keyHeight, valueHeight);

        float tableWidth = rect.width - DeleteButtonWidth - SpacingX;
        // key占40%, value占60%
        float keyWidth = splitCenterLocalX - SpacingX * 0.5f;
        float valueWidth = tableWidth - keyWidth - SpacingX;

        Rect keyRect = new(rect.x, y + (rowHeight - keyHeight) / 2f, keyWidth, keyHeight);

        Rect valueRect = new(keyRect.xMax + SpacingX, y + (rowHeight - valueHeight) / 2f, valueWidth, valueHeight);

        Rect deleteRect = new(valueRect.xMax + SpacingX, y + (rowHeight - lineHeight) / 2f, DeleteButtonWidth, lineHeight);

        Rect splitRect = new(rect.x, y - SpacingY * 0.5f - SplitterWidth * 0.5f, tableWidth, SplitterWidth);

        float keyLabelWidth = Math.Min(80f, keyRect.width * 0.4f);
        float valueLabelWidth = Math.Min(80f, valueRect.width * 0.4f);

        DrawPropertyWithLabelWidth(keyRect, keyProp, GUIContent.none, keyLabelWidth);
        DrawPropertyWithLabelWidth(valueRect, valueProp, GUIContent.none, valueLabelWidth);
        EditorGUI.DrawRect(splitRect, new Color(0.45f, 0.45f, 0.45f, 1f));

        if (GUI.Button(deleteRect, "X")) {
            pairsProp.DeleteArrayElementAtIndex(index);
            return true;
        }

        y += rowHeight + SpacingY;

        return false;
    }

    private void DrawPropertyWithLabelWidth(
        Rect rect,
        SerializedProperty prop,
        GUIContent label,
        float labelWidth
    ) {
        float oldLabelWidth = EditorGUIUtility.labelWidth;
        // 这里让小三角老老实实待在rect里, 不要跑出来与分隔线重叠
        bool oldHierarchyMode = EditorGUIUtility.hierarchyMode;

        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUIUtility.hierarchyMode = false;
        EditorGUI.PropertyField(rect, prop, label, true);

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUIUtility.hierarchyMode = oldHierarchyMode;
    }

    private void DrawAddButton(Rect rect, float y, SerializedProperty pairsProp, SerializedProperty property, GUIContent label) {
        float buttonWidth = (rect.width - ButtonSpacing) * 0.5f;

        Rect addButtonRect = new(rect.x, y, buttonWidth, ButtonHeight);

        Rect openButtonRect = new(addButtonRect.xMax + ButtonSpacing, y, buttonWidth, ButtonHeight);

        if (GUI.Button(addButtonRect, "+ 添加一行")) {
            int newIndex = pairsProp.arraySize;
            pairsProp.arraySize++;

            SerializedProperty newPair = pairsProp.GetArrayElementAtIndex(newIndex);
            ClearPair(newPair);
        }
        if (GUI.Button(openButtonRect, "在独立窗口中打开")) {
            SerializedPropertyWindow.Open(property, label);
        }
    }

    //============================
    //       分隔线增强功能       ||
    //============================
    private float ClampSplitCenterLocalX(float splitCenterLocalX, float tableWidth) {
        float minSplitCenterLocalX = MinColumnWidth + SpacingX * 0.5f;
        float maxSplitCenterLocalX = tableWidth - MinColumnWidth - SpacingX * 0.5f;

        if (maxSplitCenterLocalX < minSplitCenterLocalX)
            return tableWidth * 0.5f;


        return Mathf.Clamp(splitCenterLocalX, minSplitCenterLocalX, maxSplitCenterLocalX);
    }

    private void HandleSplitter(Rect rect, string splitKey, float tableWidth, float contentTop, ref float splitCenterLocalX) {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        float splitterX = rect.x + splitCenterLocalX;

        Rect splitterRect = new(
            splitterX - SplitterHitWidth * 0.5f,
            contentTop,
            SplitterHitWidth,
            Mathf.Max(0f, rect.yMax - contentTop - lineHeight - EditorGUIUtility.standardVerticalSpacing * 2f)
        );

        EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

        int controlId = GUIUtility.GetControlID(FocusType.Passive, splitterRect);
        Event evt = Event.current;

        switch (evt.GetTypeForControl(controlId)) {
            case EventType.MouseDown:
                if (evt.button == 0 && splitterRect.Contains(evt.mousePosition)) {
                    GUIUtility.hotControl = controlId;
                    evt.Use();
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlId) {
                    splitCenterLocalX = evt.mousePosition.x - rect.x;
                    splitCenterLocalX = ClampSplitCenterLocalX(splitCenterLocalX, tableWidth);
                    SplitCenterLocalXRatio[splitKey] = splitCenterLocalX / tableWidth;

                    GUI.changed = true;
                    evt.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlId) {
                    GUIUtility.hotControl = 0;
                    evt.Use();
                }
                break;

            case EventType.Repaint:
                Rect lineRect = new(
                    splitterX - 0.5f,
                    contentTop,
                    SplitterWidth,
                    splitterRect.height
                );

                Color color = GUIUtility.hotControl == controlId
                    ? new Color(0.35f, 0.55f, 0.9f, 1f)
                    : new Color(0.45f, 0.45f, 0.45f, 1f);

                EditorGUI.DrawRect(lineRect, color);
                break;
        }
    }

    private void ClearPair(SerializedProperty pairProp) {
        SerializedProperty keyProp = pairProp.FindPropertyRelative("key");
        SerializedProperty valueProp = pairProp.FindPropertyRelative("value");

        PropertyUtil.ClearProperty(keyProp);
        PropertyUtil.ClearProperty(valueProp);
    }
}
}
