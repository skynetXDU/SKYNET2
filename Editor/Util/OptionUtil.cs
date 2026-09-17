using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

internal sealed class OptionConfiguration {
    internal readonly Type ValueType;
    internal readonly object[] Values;
    internal readonly GUIContent[] Labels;
    internal readonly string Error;
    internal bool IsValid => Error == null && Values.Length > 0;

    internal OptionConfiguration(Type valueType, object[] values, GUIContent[] labels, string error) {
        ValueType = valueType;
        Values = values;
        Labels = labels;
        Error = error;
    }

    internal int FindSelected(object value, bool mixed = false) {
        if (!IsValid || mixed) return -1;
        for (int i = 0; i < Values.Length; i++)
            if (Values[i].Equals(value)) return i;
        return -1;
    }
}

internal static class OptionUtil {
    private static readonly Dictionary<(FieldInfo, Type), OptionConfiguration> Configurations = new();

    internal static OptionConfiguration GetConfiguration(FieldInfo field) {
        return GetConfiguration(field, typeof(OptionAttribute));
    }

    internal static OptionConfiguration GetKeyConfiguration(FieldInfo field) {
        return GetConfiguration(field, typeof(KeyOptionAttribute));
    }

    internal static OptionConfiguration GetValueConfiguration(FieldInfo field) {
        return GetConfiguration(field, typeof(ValueOptionAttribute));
    }

    private static OptionConfiguration GetConfiguration(FieldInfo field, Type attributeType) {
        var cacheKey = (field, attributeType);
        if (Configurations.TryGetValue(cacheKey, out OptionConfiguration cached)) return cached;

        Type type = GetValueType(field.FieldType);
        bool isDictionaryOption = attributeType != typeof(OptionAttribute);
        bool isKey = attributeType == typeof(KeyOptionAttribute);
        if (isDictionaryOption) {
            // Strip only the outer array/List wrapper, never containers inside K or V.
            // A nested dictionary gets its own FieldInfo and therefore its own options.
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(SDictionary<,>)) {
                OptionConfiguration empty = new(type, Array.Empty<object>(), Array.Empty<GUIContent>(), null);
                Configurations.Add(cacheKey, empty);
                return empty;
            }
            type = type.GetGenericArguments()[isKey ? 0 : 1];
        }
        string attributeName = isDictionaryOption ? (isKey ? "KeyOption" : "ValueOption") : "Option";
        string columnName = isDictionaryOption ? (isKey ? " 的 key 列" : " 的 value 列") : "";
        List<object> values = new();
        List<GUIContent> labels = new();
        List<string> errors = new();

        // Read the compiler-emitted attribute metadata in declaration order. Unity sorts
        // PropertyAttributes for its drawer chain; that sorted chain is NOT our option list.
        foreach (CustomAttributeData data in field.GetCustomAttributesData()) {
            if (data.AttributeType != attributeType) continue;
            CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
            object value = argument.Value;
            if (argument.ArgumentType.IsEnum && value != null)
                value = Enum.ToObject(argument.ArgumentType, value);
            string label = data.ConstructorArguments.Count > 1
                ? data.ConstructorArguments[1].Value as string : null;
            string display = string.IsNullOrEmpty(label) ? FormatValue(value) : label;
            values.Add(value);
            labels.Add(new GUIContent(display));

            if (!IsSupported(type) || value == null || value.GetType() != type)
                errors.Add($"选项 #{values.Count} “{display}” 的值类型为 {TypeName(value?.GetType())}");
        }

        string error = errors.Count == 0 ? null
            : $"{attributeName} 配置错误：字段 {field.Name}{columnName}的类型为 {TypeName(type)}"
                + (IsSupported(type) ? "。" : "（不支持）。")
                + string.Join("；", errors)
                + (isDictionaryOption ? $"。该列的所有 {attributeName} 已停用。" : "。该字段的所有 Option 已停用。");
        OptionConfiguration configuration = new(type, values.ToArray(), labels.ToArray(), error);
        Configurations.Add(cacheKey, configuration);
        return configuration;
    }

    internal static Type GetValueType(Type type) {
        if (type.IsArray && type.GetArrayRank() == 1) return type.GetElementType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return type.GetGenericArguments()[0];
        return type;
    }

    internal static bool IsSupported(Type type) {
        return type == typeof(bool) || type == typeof(char) || type == typeof(byte)
            || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
            || type == typeof(int) || type == typeof(uint) || type == typeof(long)
            || type == typeof(ulong) || type == typeof(float) || type == typeof(double)
            || type == typeof(string);
    }

    internal static string FormatValue(object value) {
        if (value == null) return "null";
        if (value is string text) return text.Length == 0 ? "\"\"" : text;
        if (value is char character && char.IsControl(character)) return $"\\u{(int)character:x4}";
        return value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture) : value.ToString();
    }

    private static string TypeName(Type type) {
        if (type == null) return "null";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(char)) return "char";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(sbyte)) return "sbyte";
        if (type == typeof(short)) return "short";
        if (type == typeof(ushort)) return "ushort";
        if (type == typeof(int)) return "int";
        if (type == typeof(uint)) return "uint";
        if (type == typeof(long)) return "long";
        if (type == typeof(ulong)) return "ulong";
        if (type == typeof(float)) return "float";
        if (type == typeof(double)) return "double";
        if (type == typeof(string)) return "string";
        return type.Name;
    }

    // boxedValue preserves Unity's numericType, including unsigned integers and doubles.
    // char is represented as an integer by some Unity versions, so normalize explicitly.
    internal static object ReadValue(SerializedProperty property, Type type) {
        return type == typeof(char) ? (object)(char)property.intValue : property.boxedValue;
    }

    internal static void WriteValue(SerializedProperty property, OptionConfiguration configuration, int index) {
        if (!configuration.IsValid || index < 0 || index >= configuration.Values.Length) return;
        if (configuration.ValueType == typeof(char)) property.intValue = (char)configuration.Values[index];
        else property.boxedValue = configuration.Values[index];
    }

    internal static SerializedProperty GetConditionOwner(SerializedProperty property, FieldInfo field) {
        if (GetValueType(field.FieldType) == field.FieldType) return property;
        string path = property.propertyPath;
        int arrayIndex = path.LastIndexOf(".Array.data[", StringComparison.Ordinal);
        return arrayIndex >= 0 && path.EndsWith("]", StringComparison.Ordinal)
            ? property.serializedObject.FindProperty(path.Substring(0, arrayIndex)) : property;
    }
}
}
