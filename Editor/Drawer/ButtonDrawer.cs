using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace SKYNET.Editor {

public static class ButtonDrawer {

    private const int MaxDepth = 5;

    private static readonly BindingFlags MethodFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private static readonly BindingFlags FieldFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private static readonly Dictionary<string, MethodState> MethodStates = new();

    public static void Draw(UnityEngine.Object[] targets) {
        if (targets == null || targets.Length == 0 || targets[0] == null)
            return;

        List<MethodInfo> methods = GetButtonMethods(targets[0].GetType());
        if (methods.Count == 0)
            return;

        EditorGUILayout.Space();

        foreach (MethodInfo method in methods) {
            DrawMethod(targets, method);
        }
    }

    private static void DrawMethod(UnityEngine.Object[] targets, MethodInfo method) {
        ButtonAttribute button = method.GetCustomAttribute<ButtonAttribute>();
        string label = string.IsNullOrEmpty(button.Label)
            ? ObjectNames.NicifyVariableName(method.Name)
            : button.Label;

        ParameterInfo[] parameters = method.GetParameters();
        MethodState state = GetState(method, parameters);
        bool canInvoke = state.CanInvoke && AreParametersInvokable(parameters, state.Values);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        using (new EditorGUI.DisabledScope(!canInvoke)) {
            if (GUILayout.Button(label)) {
                InvokeMethod(targets, method, label, state.Values);
            }
        }

        if (parameters.Length > 0) {
            EditorGUI.indentLevel++;
            bool parametersValid = true;

            for (int i = 0; i < parameters.Length; i++) {
                ParameterInfo parameter = parameters[i];
                string parameterName = string.IsNullOrEmpty(parameter.Name) ? $"Parameter {i + 1}" : parameter.Name;
                GUIContent parameterLabel = new(ObjectNames.NicifyVariableName(parameterName));

                if (parameter.ParameterType.IsByRef || parameter.IsOut) {
                    EditorGUILayout.HelpBox($"{parameterName}: ref/out parameters are not supported.", MessageType.Warning);
                    parametersValid = false;
                    continue;
                }

                bool valid;
                state.Values[i] = DrawValue(parameterLabel, parameter.ParameterType, state.Values[i], 0, new HashSet<object>(ReferenceEqualityComparer.Instance), out valid);
                if (!valid)
                    parametersValid = false;
            }

            state.CanInvoke = parametersValid;
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private static object DrawValue(GUIContent label, Type type, object value, int depth, HashSet<object> visited, out bool valid) {
        valid = true;

        if (type == typeof(bool))
            return EditorGUILayout.Toggle(label, value is bool b && b);

        if (type == typeof(string))
            return EditorGUILayout.TextField(label, value as string ?? string.Empty);

        if (type == typeof(char)) {
            string text = value is char c && c != '\0' ? c.ToString() : string.Empty;
            text = EditorGUILayout.TextField(label, text);
            return string.IsNullOrEmpty(text) ? '\0' : text[0];
        }

        if (type == typeof(byte))
            return (byte)Mathf.Clamp(EditorGUILayout.IntField(label, value is byte v ? v : default), byte.MinValue, byte.MaxValue);

        if (type == typeof(sbyte))
            return (sbyte)Mathf.Clamp(EditorGUILayout.IntField(label, value is sbyte v ? v : default), sbyte.MinValue, sbyte.MaxValue);

        if (type == typeof(short))
            return (short)Mathf.Clamp(EditorGUILayout.IntField(label, value is short v ? v : default), short.MinValue, short.MaxValue);

        if (type == typeof(ushort))
            return (ushort)Mathf.Clamp(EditorGUILayout.IntField(label, value is ushort v ? v : default), ushort.MinValue, ushort.MaxValue);

        if (type == typeof(int))
            return EditorGUILayout.IntField(label, value is int v ? v : default);

        if (type == typeof(uint)) {
            long current = value is uint v ? v : default;
            return ClampToUInt(EditorGUILayout.LongField(label, current));
        }

        if (type == typeof(long))
            return EditorGUILayout.LongField(label, value is long v ? v : default);

        if (type == typeof(ulong)) {
            string text = value is ulong v ? v.ToString() : "0";
            text = EditorGUILayout.TextField(label, text);
            if (ulong.TryParse(text, out ulong parsed))
                return parsed;

            valid = false;
            return value ?? default(ulong);
        }

        if (type == typeof(float))
            return EditorGUILayout.FloatField(label, value is float v ? v : default);

        if (type == typeof(double))
            return EditorGUILayout.DoubleField(label, value is double v ? v : default);

        if (type.IsEnum) {
            Enum enumValue = value as Enum ?? (Enum)Enum.ToObject(type, 0);
            return type.GetCustomAttribute<FlagsAttribute>() != null
                ? EditorGUILayout.EnumFlagsField(label, enumValue)
                : EditorGUILayout.EnumPopup(label, enumValue);
        }

        if (type == typeof(Vector2))
            return EditorGUILayout.Vector2Field(label, value is Vector2 v ? v : default);

        if (type == typeof(Vector3))
            return EditorGUILayout.Vector3Field(label, value is Vector3 v ? v : default);

        if (type == typeof(Vector4))
            return EditorGUILayout.Vector4Field(label, value is Vector4 v ? v : default);

        if (type == typeof(Vector2Int))
            return EditorGUILayout.Vector2IntField(label, value is Vector2Int v ? v : default);

        if (type == typeof(Vector3Int))
            return EditorGUILayout.Vector3IntField(label, value is Vector3Int v ? v : default);

        if (type == typeof(Color))
            return EditorGUILayout.ColorField(label, value is Color v ? v : Color.white);

        if (type == typeof(Rect))
            return EditorGUILayout.RectField(label, value is Rect v ? v : default);

        if (type == typeof(RectInt))
            return EditorGUILayout.RectIntField(label, value is RectInt v ? v : default);

        if (type == typeof(Bounds))
            return EditorGUILayout.BoundsField(label, value is Bounds v ? v : default);

        if (type == typeof(BoundsInt))
            return EditorGUILayout.BoundsIntField(label, value is BoundsInt v ? v : default);

        if (type == typeof(AnimationCurve))
            return EditorGUILayout.CurveField(label, value as AnimationCurve ?? new AnimationCurve());

        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            return EditorGUILayout.ObjectField(label, value as UnityEngine.Object, type, true);

        if (IsUnsupportedContainer(type)) {
            EditorGUILayout.HelpBox($"{label.text}: collection parameters are not supported in Button v1.", MessageType.Warning);
            valid = false;
            return value;
        }

        if (IsCustomSerializableType(type))
            return DrawCustomValue(label, type, value, depth, visited, out valid);

        EditorGUILayout.HelpBox($"{label.text}: {type.Name} is not a supported Button parameter type.", MessageType.Warning);
        valid = false;
        return value;
    }

    private static object DrawCustomValue(GUIContent label, Type type, object value, int depth, HashSet<object> visited, out bool valid) {
        valid = true;

        if (depth >= MaxDepth) {
            EditorGUILayout.HelpBox($"{label.text}: max draw depth reached.", MessageType.Info);
            return value;
        }

        if (value == null) {
            value = CreateDefaultValue(type, out bool created);
            if (!created) {
                EditorGUILayout.HelpBox($"{label.text}: {type.Name} requires a public or non-public parameterless constructor.", MessageType.Warning);
                valid = false;
                return null;
            }
        }

        if (!type.IsValueType) {
            if (visited.Contains(value)) {
                EditorGUILayout.HelpBox($"{label.text}: circular reference skipped.", MessageType.Info);
                return value;
            }

            visited.Add(value);
        }

        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        foreach (FieldInfo field in GetSerializableFields(type)) {
            object fieldValue = field.GetValue(value);
            bool fieldValid;
            fieldValue = DrawValue(new GUIContent(ObjectNames.NicifyVariableName(field.Name)), field.FieldType, fieldValue, depth + 1, visited, out fieldValid);

            if (fieldValid)
                field.SetValue(value, fieldValue);
            else
                valid = false;
        }

        EditorGUI.indentLevel--;

        if (!type.IsValueType)
            visited.Remove(value);

        return value;
    }

    private static void InvokeMethod(UnityEngine.Object[] targets, MethodInfo method, string label, object[] values) {
        UnityEngine.Object[] compatibleTargets = targets
            .Where(target => target != null && method.DeclaringType != null && method.DeclaringType.IsAssignableFrom(target.GetType()))
            .ToArray();

        if (compatibleTargets.Length == 0)
            return;

        Undo.RecordObjects(compatibleTargets, $"Invoke {label}");

        foreach (UnityEngine.Object target in compatibleTargets) {
            try {
                method.Invoke(target, values.ToArray());
                EditorUtility.SetDirty(target);
            }
            catch (TargetInvocationException e) {
                Debug.LogException(e.InnerException ?? e, target);
            }
            catch (Exception e) {
                Debug.LogException(e, target);
            }
        }
    }

    private static MethodState GetState(MethodInfo method, ParameterInfo[] parameters) {
        string key = $"{method.Module.ModuleVersionId}:{method.MetadataToken}";

        if (MethodStates.TryGetValue(key, out MethodState state) && state.Values.Length == parameters.Length)
            return state;

        object[] values = new object[parameters.Length];
        bool canInvoke = true;

        for (int i = 0; i < parameters.Length; i++) {
            values[i] = GetInitialValue(parameters[i], out bool valid);
            if (!valid)
                canInvoke = false;
        }

        state = new MethodState(values, canInvoke);
        MethodStates[key] = state;
        return state;
    }

    private static object GetInitialValue(ParameterInfo parameter, out bool valid) {
        valid = true;

        if (parameter.HasDefaultValue && parameter.DefaultValue != DBNull.Value)
            return parameter.DefaultValue;

        return CreateDefaultValue(parameter.ParameterType, out valid);
    }

    private static object CreateDefaultValue(Type type, out bool valid) {
        valid = true;

        if (type == typeof(string))
            return string.Empty;

        if (type == typeof(AnimationCurve))
            return new AnimationCurve();

        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            return null;

        if (type.IsValueType)
            return Activator.CreateInstance(type);

        if (type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null) {
            valid = false;
            return null;
        }

        try {
            return Activator.CreateInstance(type, true);
        }
        catch {
            valid = false;
            return null;
        }
    }

    private static List<MethodInfo> GetButtonMethods(Type type) {
        List<MethodInfo> methods = new();
        HashSet<string> signatures = new();

        for (Type current = type; current != null && current != typeof(object); current = current.BaseType) {
            MethodInfo[] declaredMethods = current.GetMethods(MethodFlags);
            Array.Sort(declaredMethods, CompareMetadataOrder);

            foreach (MethodInfo method in declaredMethods) {
                if (!CanDrawMethod(method))
                    continue;

                string signature = GetMethodSignature(method);
                if (signatures.Add(signature))
                    methods.Add(method);
            }
        }

        return methods;
    }

    private static bool CanDrawMethod(MethodInfo method) {
        return method.GetCustomAttribute<ButtonAttribute>() != null
            && !method.IsGenericMethod
            && !method.IsAbstract
            && !method.IsSpecialName;
    }

    private static IEnumerable<FieldInfo> GetSerializableFields(Type type) {
        List<FieldInfo> fields = new();

        for (Type current = type; current != null && current != typeof(object); current = current.BaseType) {
            FieldInfo[] declaredFields = current.GetFields(FieldFlags);
            Array.Sort(declaredFields, CompareMetadataOrder);

            foreach (FieldInfo field in declaredFields) {
                if (IsSerializableField(field))
                    fields.Add(field);
            }
        }

        return fields;
    }

    private static bool IsSerializableField(FieldInfo field) {
        if (field.IsStatic || field.IsInitOnly || field.IsLiteral)
            return false;

        if (field.GetCustomAttribute<NonSerializedAttribute>() != null)
            return false;

        return field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
    }

    private static bool IsCustomSerializableType(Type type) {
        return type.GetCustomAttribute<SerializableAttribute>() != null
            && !type.IsPrimitive
            && type != typeof(string)
            && !type.IsEnum
            && !typeof(UnityEngine.Object).IsAssignableFrom(type);
    }

    private static bool IsUnsupportedContainer(Type type) {
        return type.IsArray || (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type));
    }

    private static bool AreParametersInvokable(ParameterInfo[] parameters, object[] values) {
        if (parameters.Length != values.Length)
            return false;

        for (int i = 0; i < parameters.Length; i++) {
            ParameterInfo parameter = parameters[i];

            if (parameter.ParameterType.IsByRef || parameter.IsOut)
                return false;

            if (!IsInvokableType(parameter.ParameterType, values[i], 0, new HashSet<Type>()))
                return false;
        }

        return true;
    }

    private static bool IsInvokableType(Type type, object value, int depth, HashSet<Type> typeStack) {
        if (type == typeof(bool)
            || type == typeof(string)
            || type == typeof(char)
            || type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(ushort)
            || type == typeof(int)
            || type == typeof(uint)
            || type == typeof(long)
            || type == typeof(ulong)
            || type == typeof(float)
            || type == typeof(double)
            || type.IsEnum
            || type == typeof(Vector2)
            || type == typeof(Vector3)
            || type == typeof(Vector4)
            || type == typeof(Vector2Int)
            || type == typeof(Vector3Int)
            || type == typeof(Color)
            || type == typeof(Rect)
            || type == typeof(RectInt)
            || type == typeof(Bounds)
            || type == typeof(BoundsInt)
            || type == typeof(AnimationCurve)
            || typeof(UnityEngine.Object).IsAssignableFrom(type))
            return true;

        if (IsUnsupportedContainer(type))
            return false;

        if (!IsCustomSerializableType(type))
            return false;

        if (!type.IsValueType && value == null)
            return false;

        if (depth >= MaxDepth || typeStack.Contains(type))
            return true;

        typeStack.Add(type);

        foreach (FieldInfo field in GetSerializableFields(type)) {
            object fieldValue = value != null ? field.GetValue(value) : null;
            if (!IsInvokableType(field.FieldType, fieldValue, depth + 1, typeStack)) {
                typeStack.Remove(type);
                return false;
            }
        }

        typeStack.Remove(type);
        return true;
    }

    private static uint ClampToUInt(long value) {
        if (value < uint.MinValue)
            return uint.MinValue;

        if (value > uint.MaxValue)
            return uint.MaxValue;

        return (uint)value;
    }

    private static int CompareMetadataOrder(MemberInfo left, MemberInfo right) {
        try {
            return left.MetadataToken.CompareTo(right.MetadataToken);
        }
        catch {
            return string.CompareOrdinal(left.Name, right.Name);
        }
    }

    private static string GetMethodSignature(MethodInfo method) {
        string parameters = string.Join(",", method.GetParameters().Select(parameter => parameter.ParameterType.FullName));
        return $"{method.Name}({parameters})";
    }

    private sealed class MethodState {
        public readonly object[] Values;
        public bool CanInvoke;

        public MethodState(object[] values, bool canInvoke) {
            Values = values;
            CanInvoke = canInvoke;
        }
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object> {
        public static readonly ReferenceEqualityComparer Instance = new();

        public new bool Equals(object x, object y) {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
}
