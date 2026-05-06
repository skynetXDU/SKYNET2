using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

public class PropertyUtil {

    public static void ClearProperty(SerializedProperty prop) {
        if (prop == null) {
            return;
        }

        if (prop.isArray && prop.propertyType != SerializedPropertyType.String) {
            prop.arraySize = 0;
            return;
        }

        switch (prop.propertyType) {
            case SerializedPropertyType.Generic:
                ClearChildProperties(prop);
                break;
            case SerializedPropertyType.Integer:
                prop.intValue = 0;
                break;
            case SerializedPropertyType.Boolean:
                prop.boolValue = false;
                break;
            case SerializedPropertyType.Float:
                prop.floatValue = 0f;
                break;
            case SerializedPropertyType.String:
                prop.stringValue = "";
                break;
            case SerializedPropertyType.Color:
                prop.colorValue = Color.white;
                break;
            case SerializedPropertyType.ObjectReference:
                prop.objectReferenceValue = null;
                break;
            case SerializedPropertyType.Enum:
                prop.enumValueIndex = 0;
                break;
            case SerializedPropertyType.Vector2:
                prop.vector2Value = Vector2.zero;
                break;
            case SerializedPropertyType.Vector3:
                prop.vector3Value = Vector3.zero;
                break;
            case SerializedPropertyType.Vector4:
                prop.vector4Value = Vector4.zero;
                break;
            case SerializedPropertyType.Rect:
                prop.rectValue = Rect.zero;
                break;
            case SerializedPropertyType.Bounds:
                prop.boundsValue = new Bounds();
                break;
            case SerializedPropertyType.AnimationCurve:
                prop.animationCurveValue = new AnimationCurve();
                break;
            case SerializedPropertyType.Quaternion:
                prop.quaternionValue = default;
                break;
            case SerializedPropertyType.Vector2Int:
                prop.vector2IntValue = Vector2Int.zero;
                break;
            case SerializedPropertyType.Vector3Int:
                prop.vector3IntValue = Vector3Int.zero;
                break;
            case SerializedPropertyType.RectInt:
                prop.rectIntValue = new RectInt();
                break;
            case SerializedPropertyType.BoundsInt:
                prop.boundsIntValue = new BoundsInt();
                break;
            case SerializedPropertyType.Character:
            case SerializedPropertyType.LayerMask:
                prop.intValue = 0;
                break;
            case SerializedPropertyType.ManagedReference:
                prop.managedReferenceValue = null;
                break;
            default:
                if (prop.hasVisibleChildren)
                    ClearChildProperties(prop);
                break;
        }
    }

    private static void ClearChildProperties(SerializedProperty prop) {
        SerializedProperty child = prop.Copy();
        SerializedProperty end = child.GetEndProperty();

        bool enterChildren = true;
        while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end)) {
            ClearProperty(child);
            enterChildren = false;
        }
    }

}
}
