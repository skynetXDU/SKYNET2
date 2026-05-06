using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SKYNET.Editor {

public static class SkynetEditorUtil {

    private static readonly Dictionary<Type, bool> SerializableTypeCache = new();

    public static bool IsUnitySerializedField(FieldInfo field) {
        if (field == null)
            return false;
        // 静态或不可变字段不序列化
        if (field.IsStatic || field.IsInitOnly || field.IsLiteral)
            return false;
        // 有不序列化注解的不序列化
        if (field.GetCustomAttribute<NonSerializedAttribute>() != null)
            return false;
        // private默认不序列化
        if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
            return false;

        return IsUnitySerializableType(field.FieldType);
    }

    public static bool IsUnitySerializableType(Type type) {
        if (type == null)
            return false;

        if (SerializableTypeCache.TryGetValue(type, out bool cached))
            return cached;

        bool res;
        // 首先排除掉二维数组
        if (type.IsArray) {
            Type elementType = type.GetElementType();
            // 排除掉二维数组
            // 一维数组且数组元素类型可序列化, 那么该类型可序列化
            res = type.GetArrayRank() == 1 && !IsContainerType(elementType) && IsUnitySerializableType(elementType);
            
            SerializableTypeCache[type] = res;
            return res;
        }
        if (type.IsGenericType) {
            Type elementType = type.GetGenericArguments()[0];
            // 一维列表且列表元素类型可序列化, 那么该类型可序列化
            res =   (type.GetGenericTypeDefinition() == typeof(List<>)
                    && !IsContainerType(elementType)
                    && IsUnitySerializableType(elementType))
                ||  (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(SDictionary<,>)
                    && IsUnitySerializableType(type.GetGenericArguments()[0])
                    && IsUnitySerializableType(type.GetGenericArguments()[1]));
            
            SerializableTypeCache[type] = res;
            return res;
        }

        res = IsBasicType(type) || IsUnityType(type)
            // 标记了Serializable注解的类型可序列化
            || (!type.IsGenericType
                && !type.IsAbstract
                && !typeof(Delegate).IsAssignableFrom(type)
                && (type.IsClass || type.IsValueType)
                && type.GetCustomAttribute<SerializableAttribute>() != null);


        SerializableTypeCache[type] = res;
        return res;
    }

    public static bool IsContainerType(Type type) {
        return type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>));
    }

    private static bool IsBasicType(Type type) {
        return type.IsEnum || type == typeof(bool) || type == typeof(char) || type == typeof(byte)
            || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int)
            || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) || type == typeof(float)
            || type == typeof(double) || type == typeof(string);
    }
    private static bool IsUnityType(Type type) {
        return typeof(UnityEngine.Object).IsAssignableFrom(type) || type == typeof(Color)
            || type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4)
            || type == typeof(Vector2Int) || type == typeof(Vector3Int) || type == typeof(Rect)
            || type == typeof(RectInt) || type == typeof(Bounds) || type == typeof(BoundsInt)
            || type == typeof(Quaternion) || type == typeof(AnimationCurve) || type == typeof(Gradient)
            || type == typeof(LayerMask) || type == typeof(Matrix4x4);
    }
}
}
