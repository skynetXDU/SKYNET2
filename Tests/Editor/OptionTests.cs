using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SKYNET.Editor.Tests {

public sealed class OptionTests {
    private OptionTestAsset target;
    private SerializedObject serialized;

    [SetUp] public void SetUp() {
        target = ScriptableObject.CreateInstance<OptionTestAsset>();
        serialized = new SerializedObject(target);
    }

    [TearDown] public void TearDown() {
        foreach (OptionTestWindow window in Resources.FindObjectsOfTypeAll<OptionTestWindow>()) window.Close();
        serialized.Dispose();
        Object.DestroyImmediate(target);
    }

    private static OptionConfiguration Configuration(string name) {
        return OptionUtil.GetConfiguration(typeof(OptionTestAsset).GetField(name));
    }

    [TestCase("boolean")] [TestCase("character")] [TestCase("unsignedByte")]
    [TestCase("signedByte")] [TestCase("smallInteger")] [TestCase("unsignedSmallInteger")]
    [TestCase("integer")] [TestCase("unsignedInteger")] [TestCase("largeInteger")]
    [TestCase("unsignedLargeInteger")] [TestCase("single")] [TestCase("precision")]
    [TestCase("text")]
    public void AllSupportedTypesRoundTripWithoutPrecisionLoss(string name) {
        OptionConfiguration configuration = Configuration(name);
        Assert.That(configuration.IsValid, Is.True, configuration.Error);
        SerializedProperty property = serialized.FindProperty(name);
        for (int index = 0; index < configuration.Values.Length; index++) {
            OptionUtil.WriteValue(property, configuration, index);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            serialized.Update();
            object actual = OptionUtil.ReadValue(property, configuration.ValueType);
            Assert.That(actual.GetType(), Is.EqualTo(configuration.ValueType));
            Assert.That(configuration.Values[index].Equals(actual), Is.True, $"{name}[{index}]");
            Assert.That(configuration.FindSelected(actual), Is.EqualTo(index));
        }
    }

    [TestCase("wrongNumber")] [TestCase("wrongString")] [TestCase("nullOption")]
    [TestCase("unsupported")] [TestCase("enumValue")] [TestCase("enumField")]
    [TestCase("decimalField")]
    public void OneInvalidOptionDisablesEntireConfiguration(string name) {
        OptionConfiguration configuration = Configuration(name);
        Assert.That(configuration.IsValid, Is.False);
        Assert.That(configuration.Error, Does.Contain(name).And.Contain("所有 Option 已停用"));
        Assert.That(configuration.FindSelected(configuration.Values[0]), Is.EqualTo(-1));
        SerializedProperty property = serialized.FindProperty(name);
        if (property == null) return; // Unity does not serialize decimal.
        object before = property.boxedValue;
        OptionUtil.WriteValue(property, configuration, 0);
        Assert.That(property.boxedValue, Is.EqualTo(before));
        Assert.That(serialized.hasModifiedProperties, Is.False);
    }

    [Test] public void MetadataOrderIgnoresDrawerOrderAndConfigurationIsCached() {
        OptionConfiguration configuration = Configuration("ordered");
        CollectionAssert.AreEqual(new object[] { 3, 1, 2 }, configuration.Values);
        Assert.That(configuration.Labels[0].text, Is.EqualTo("third"));
        Assert.That(Configuration("ordered"), Is.SameAs(configuration));
    }

    [Test] public void LabelsUseInvariantCultureAndEmptyLabelsFallBack() {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.That(OptionUtil.FormatValue(1.5f), Is.EqualTo("1.5"));
            Assert.That(OptionUtil.FormatValue(""), Is.EqualTo("\"\""));
            Assert.That(OptionUtil.FormatValue('\0'), Is.EqualTo("\\u0000"));
            OptionConfiguration configuration = Configuration("defaultLabels");
            for (int i = 0; i < 3; i++) Assert.That(configuration.Labels[i].text, Is.EqualTo((i + 1).ToString()));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Test] public void SelectionHandlesUnknownMixedDuplicateAndExactFloatingValues() {
        Assert.That(Configuration("duplicates").FindSelected(1), Is.EqualTo(0));
        Assert.That(Configuration("duplicates").FindSelected(99), Is.EqualTo(-1));
        Assert.That(Configuration("duplicates").FindSelected(1, true), Is.EqualTo(-1));
        Assert.That(Configuration("precision").FindSelected(1.234567890123456d), Is.EqualTo(-1));
        Assert.That(Configuration("single").FindSelected(float.NaN), Is.EqualTo(2));
    }

    [TestCase("array", typeof(int))] [TestCase("list", typeof(float))]
    public void CollectionsValidateAndWriteElementType(string name, Type expectedType) {
        OptionConfiguration configuration = Configuration(name);
        Assert.That(configuration.ValueType, Is.EqualTo(expectedType));
        SerializedProperty element = serialized.FindProperty(name).GetArrayElementAtIndex(0);
        OptionUtil.WriteValue(element, configuration, 1);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        serialized.Update();
        Assert.That(OptionUtil.ReadValue(element, expectedType), Is.EqualTo(configuration.Values[1]));
        Assert.That(OptionUtil.GetConditionOwner(element, typeof(OptionTestAsset).GetField(name)).propertyPath, Is.EqualTo(name));
    }

    [Test] public void MultiObjectWriteAndUndoRedoUseSerializedProperties() {
        OptionTestAsset second = ScriptableObject.CreateInstance<OptionTestAsset>();
        try {
            target.ordered = 3;
            second.ordered = 1;
            using SerializedObject multiple = new(new Object[] { target, second });
            SerializedProperty property = multiple.FindProperty("ordered");
            Assert.That(property.hasMultipleDifferentValues, Is.True);
            OptionUtil.WriteValue(property, Configuration("ordered"), 2);
            multiple.ApplyModifiedProperties();
            Undo.FlushUndoRecordObjects();
            Assert.That(target.ordered, Is.EqualTo(2));
            Assert.That(second.ordered, Is.EqualTo(2));
            Undo.PerformUndo();
            Assert.That(target.ordered, Is.EqualTo(3));
            Assert.That(second.ordered, Is.EqualTo(1));
            Undo.PerformRedo();
            Assert.That(target.ordered, Is.EqualTo(2));
            Assert.That(second.ordered, Is.EqualTo(2));
        }
        finally { Undo.ClearUndo(target); Undo.ClearUndo(second); Object.DestroyImmediate(second); }
    }

    [Test] public void PrefabWriteRecordsOverrideAndPersists() {
        string folder = "Assets/OptionTest_" + Guid.NewGuid().ToString("N");
        AssetDatabase.CreateFolder("Assets", folder.Substring("Assets/".Length));
        GameObject source = new("Option test");
        GameObject instance = null;
        try {
            source.AddComponent<OptionTestComponent>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, folder + "/Example.prefab");
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            OptionTestComponent component = instance.GetComponent<OptionTestComponent>();
            using SerializedObject so = new(component);
            SerializedProperty property = so.FindProperty("value");
            OptionConfiguration configuration = OptionUtil.GetConfiguration(typeof(OptionTestComponent).GetField("value"));
            OptionUtil.WriteValue(property, configuration, 1);
            so.ApplyModifiedProperties();
            Assert.That(property.prefabOverride, Is.True);
            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
            AssetDatabase.SaveAssets();
            Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/Example.prefab")
                .GetComponent<OptionTestComponent>().value, Is.EqualTo(2));
        }
        finally {
            if (instance != null) Object.DestroyImmediate(instance);
            Object.DestroyImmediate(source);
            AssetDatabase.DeleteAsset(folder);
        }
    }

    [UnityTest] public IEnumerator DrawerChainDrawsOnceAndInvalidFallbackDoesNotWrite() {
        OptionTestWindow window = Open("ordered");
        yield return null;
        window.SendEvent(new Event { type = EventType.Layout });
        Assert.That(window.Failure, Is.Null);
        Assert.That(window.PropertyHeight, Is.EqualTo(EditorGUIUtility.singleLineHeight));
        Assert.That(target.ordered, Is.EqualTo(0));
        window.PropertyName = "wrongNumber";
        window.Repaint();
        yield return null;
        window.SendEvent(new Event { type = EventType.Layout });
        Assert.That(window.Failure, Is.Null);
        Assert.That(window.PropertyHeight, Is.GreaterThan(EditorGUIUtility.singleLineHeight));
        Assert.That(window.PropertyHeight, Is.LessThan(EditorGUIUtility.singleLineHeight * 12));
        Assert.That(target.wrongNumber, Is.EqualTo(42));
        Assert.That(serialized.hasModifiedProperties, Is.False);
    }

    [UnityTest] public IEnumerator ButtonsClickConditionsAndCollectionElementsWork() {
        OptionTestWindow window = Open("conditional");
        yield return null;
        Click(window, 0.75f);
        Assert.That(target.conditional, Is.EqualTo(2));
        target.editable = false;
        serialized.Update();
        Click(window, 0.25f);
        Assert.That(target.conditional, Is.EqualTo(2));
        target.visible = false;
        serialized.Update();
        window.SendEvent(new Event { type = EventType.Layout });
        Assert.That(window.PropertyHeight, Is.LessThanOrEqualTo(0));
        window.PropertyName = "conditionalError";
        window.SendEvent(new Event { type = EventType.Layout });
        Assert.That(window.PropertyHeight, Is.LessThanOrEqualTo(0));
        window.PropertyName = "array.Array.data[0]";
        window.SendEvent(new Event { type = EventType.Layout });
        Assert.That(window.PropertyHeight, Is.LessThanOrEqualTo(0));
        target.visible = target.editable = true;
        serialized.Update();
        Click(window, 0.75f);
        Assert.That(target.array[0], Is.EqualTo(2));
        Assert.That(window.Failure, Is.Null);
    }

    [UnityTest] public IEnumerator TableBoolOptionsAndErrorCellsDrawWithoutExceptions() {
        OptionTestWindow window = Open("tableFlags");
        serialized.FindProperty("tableFlags").isExpanded = true;
        yield return null;
        // Foldout, header, then first row; the right half of its value cell is '开'.
        float y = 10 + EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing + 4
            + EditorGUIUtility.singleLineHeight / 2;
        window.SendEvent(new Event { type = EventType.MouseDown, button = 0, mousePosition = new Vector2(440, y) });
        window.SendEvent(new Event { type = EventType.MouseUp, button = 0, mousePosition = new Vector2(440, y) });
        Assert.That(target.tableFlags[0], Is.True);
        window.PropertyName = "rows";
        serialized.FindProperty("rows").isExpanded = true;
        window.Repaint();
        yield return null;
        Assert.That(window.Failure, Is.Null);
        Assert.That(target.rows[0].error, Is.EqualTo(8));
    }

    private OptionTestWindow Open(string property) {
        OptionTestWindow window = ScriptableObject.CreateInstance<OptionTestWindow>();
        window.Serialized = serialized;
        window.PropertyName = property;
        window.position = new Rect(50, 50, 640, 400);
        window.Show();
        return window;
    }

    private static void Click(OptionTestWindow window, float fraction) {
        float x = 10 + 150 + (600 - 150) * fraction;
        Vector2 point = new(x, 10 + EditorGUIUtility.singleLineHeight / 2);
        window.SendEvent(new Event { type = EventType.MouseDown, button = 0, mousePosition = point });
        window.SendEvent(new Event { type = EventType.MouseUp, button = 0, mousePosition = point });
    }
}

public sealed class OptionTestWindow : EditorWindow {
    internal SerializedObject Serialized;
    internal string PropertyName;
    internal float PropertyHeight;
    internal Exception Failure;
    private void OnGUI() {
        if (Serialized == null) return;
        try {
            Serialized.Update();
            EditorGUIUtility.labelWidth = 150;
            SerializedProperty property = Serialized.FindProperty(PropertyName);
            PropertyHeight = EditorGUI.GetPropertyHeight(property, true);
            if (PropertyHeight > 0)
                EditorGUI.PropertyField(new Rect(10, 10, 600, PropertyHeight), property, true);
            Serialized.ApplyModifiedProperties();
        }
        catch (Exception e) { Failure = e; }
    }
}
}
