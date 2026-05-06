using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace SKYNET.Editor {

public class SDictionaryUtil {

    // 拿到字典的泛型参数, 注意那个大坑
    public static Type[] GetSDictArgs(FieldInfo fieldInfo) {
        Type fieldType = fieldInfo.FieldType;
        // 拿到泛型参数, 这里只服务SDictionary<K, V>, 所以args.Length==2一定成立
        Type[] args = fieldType.GetGenericArguments();

        // 这里有一个大坑, 这谁想得到
        // 如果有一个List<SDictionary>, 那么此时property指的是列表里一个一个的元素
        // 但是fieldInfo却是整个List字段的内容,
        // 所以要在这个的基础上往下走一层
        if(fieldType.IsArray && fieldType.GetArrayRank() == 1)
            args = fieldType.GetElementType().GetGenericArguments();
        if(fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            args = fieldType.GetGenericArguments()[0].GetGenericArguments();
        
        return args;
    }

    // 字典能不能画, 必须两个泛型参数都能序列化
    public static bool CanDrawDictionary(SerializedProperty property, FieldInfo fieldInfo) {
        
        Type[] args = GetSDictArgs(fieldInfo);

        // 两个泛型只要有一个不能序列化, 整个Dictionary就都不可序列化
        // 如果因为泛型不能被序列化而导致不绘制, 那么整个Dictionary不为null但长度为0
        return args.Length == 2
            && SkynetEditorUtil.IsUnitySerializableType(args[0])
            && SkynetEditorUtil.IsUnitySerializableType(args[1])
            && property.FindPropertyRelative("pairs") != null;
    }
}
}
