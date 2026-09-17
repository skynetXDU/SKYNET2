using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SKYNET.Editor.Tests {

public sealed class DictionaryOptionTests {
    private DictionaryOptionTestAsset target;
    private SerializedObject serialized;
    private OptionTestWindow window;

    [SetUp] public void SetUp() {
        target = ScriptableObject.CreateInstance<DictionaryOptionTestAsset>();
        serialized = new SerializedObject(target);
    }

    [TearDown] public void TearDown() {
        if (window != null) window.Close();
        Undo.ClearUndo(target);
        serialized.Dispose();
        Object.DestroyImmediate(target);
    }

    [TestCase("boolean")] [TestCase("character")] [TestCase("unsignedByte")]
    [TestCase("signedByte")] [TestCase("smallInteger")] [TestCase("unsignedSmallInteger")]
    [TestCase("integer")] [TestCase("unsignedInteger")] [TestCase("largeInteger")]
    [TestCase("unsignedLargeInteger")] [TestCase("single")] [TestCase("precision")] [TestCase("text")]
    public void BothColumnsRoundTripAllPrimitiveTypesWithoutPrecisionLoss(string name) {
        SerializedProperty pairs = serialized.FindProperty(name).FindPropertyRelative("pairs");
        pairs.arraySize = 1;
        SerializedProperty pair = pairs.GetArrayElementAtIndex(0);
        var field = DictionaryOptionConfigurationTests.Field(name);
        OptionConfiguration keys = OptionUtil.GetKeyConfiguration(field);
        OptionConfiguration values = OptionUtil.GetValueConfiguration(field);
        for (int index = 0; index < keys.Values.Length; index++) {
            OptionUtil.WriteValue(pair.FindPropertyRelative("key"), keys, index);
            OptionUtil.WriteValue(pair.FindPropertyRelative("value"), values, index);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            serialized.Update();
            Assert.That(keys.Values[index].Equals(OptionUtil.ReadValue(pair.FindPropertyRelative("key"), keys.ValueType)), Is.True);
            Assert.That(values.Values[index].Equals(OptionUtil.ReadValue(pair.FindPropertyRelative("value"), values.ValueType)), Is.True);
        }
    }

    [Test] public void MultiObjectWritesUndoAndRedoOnlyChangeTheSelectedColumn() {
        AddRow("both", "hp", 7);
        DictionaryOptionTestAsset second = ScriptableObject.CreateInstance<DictionaryOptionTestAsset>();
        try {
            second.both["mp"] = 8;
            using SerializedObject multiple = new(new Object[] { target, second });
            SerializedProperty value = multiple.FindProperty("both.pairs.Array.data[0].value");
            Assert.That(value.hasMultipleDifferentValues, Is.True);
            OptionUtil.WriteValue(value, OptionUtil.GetValueConfiguration(DictionaryOptionConfigurationTests.Field("both")), 1);
            multiple.ApplyModifiedProperties();
            Undo.FlushUndoRecordObjects();
            Assert.That(target.both["hp"], Is.EqualTo(200));
            Assert.That(second.both["mp"], Is.EqualTo(200));
            Undo.PerformUndo();
            Assert.That(target.both["hp"], Is.EqualTo(7));
            Assert.That(second.both["mp"], Is.EqualTo(8));
            Undo.PerformRedo();
            Assert.That(target.both["hp"], Is.EqualTo(200));
            Assert.That(second.both["mp"], Is.EqualTo(200));
        }
        finally { Undo.ClearUndo(second); Object.DestroyImmediate(second); }
    }

    [Test] public void DictionaryValueWriteRecordsPrefabOverride() {
        string folder = "Assets/DictionaryOptionTest_" + Guid.NewGuid().ToString("N");
        AssetDatabase.CreateFolder("Assets", folder.Substring("Assets/".Length));
        GameObject source = new("Dictionary option test");
        GameObject instance = null;
        try {
            source.AddComponent<DictionaryOptionTestComponent>().stats["hp"] = 100;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, folder + "/Example.prefab");
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var component = instance.GetComponent<DictionaryOptionTestComponent>();
            using SerializedObject so = new(component);
            SerializedProperty property = so.FindProperty("stats.pairs.Array.data[0].value");
            OptionUtil.WriteValue(property,
                OptionUtil.GetValueConfiguration(typeof(DictionaryOptionTestComponent).GetField("stats")), 1);
            so.ApplyModifiedProperties();
            Assert.That(property.prefabOverride, Is.True);
            Assert.That(component.stats["hp"], Is.EqualTo(200));
        }
        finally {
            if (instance != null) Object.DestroyImmediate(instance);
            Object.DestroyImmediate(source);
            AssetDatabase.DeleteAsset(folder);
        }
    }

    [UnityTest] public IEnumerator ClickingKeyAndValueChangesOnlyThatCell() {
        AddRow("both", "hp", 99);
        Open("both");
        yield return null;
        // Test window rect: x=10, width=600; default split is 40% of 561.
        float rowY = 10 + EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing + 6
            + EditorGUIUtility.singleLineHeight / 2;
        Click(170, rowY); // second key option: mp
        Assert.That(target.both.ContainsKey("mp"), Is.True);
        Assert.That(target.both["mp"], Is.EqualTo(99));
        Click(500, rowY); // second value option: 200
        Assert.That(target.both["mp"], Is.EqualTo(200));
        Assert.That(window.Failure, Is.Null);
    }

    [UnityTest] public IEnumerator EmptyDictionaryReportsErrorOnceRegardlessOfRowCount() {
        Open("badValue");
        yield return null;
        window.SendEvent(new Event { type = EventType.Layout });
        float emptyHeight = window.PropertyHeight;
        Assert.That(emptyHeight, Is.GreaterThan(EditorGUIUtility.singleLineHeight * 2
            + EditorGUIUtility.standardVerticalSpacing + 6 + 24));
        AddRow("badValue", "hp", 99);
        window.Repaint();
        yield return null;
        window.SendEvent(new Event { type = EventType.Layout });
        Assert.That(window.PropertyHeight - emptyHeight,
            Is.EqualTo(EditorGUIUtility.singleLineHeight + 6).Within(0.1f));
        Assert.That(target.badValue["hp"], Is.EqualTo(99));
        Assert.That(window.Failure, Is.Null);
    }

    [UnityTest] public IEnumerator ConditionsHideErrorsAndDisableButtonsIncludingDictionaryLists() {
        AddRow("conditional", "hp", 100);
        target.editable = false;
        serialized.Update();
        Open("conditional");
        yield return null;
        float rowY = 10 + EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing + 6
            + EditorGUIUtility.singleLineHeight / 2;
        Click(500, rowY);
        Assert.That(target.conditional["hp"], Is.EqualTo(100));
        target.visible = false;
        serialized.Update();
        window.SendEvent(new Event { type = EventType.Layout });
        Assert.That(window.PropertyHeight, Is.LessThanOrEqualTo(0));
        window.PropertyName = "conditionalList.Array.data[0]";
        window.SendEvent(new Event { type = EventType.Layout });
        Assert.That(window.PropertyHeight, Is.LessThanOrEqualTo(0));
        Assert.That(window.Failure, Is.Null);
    }

    [UnityTest] public IEnumerator AddAndDeleteKeepExistingDefaultInitialization() {
        Open("both");
        yield return null;
        window.SendEvent(new Event { type = EventType.Layout });
        Click(100, 10 + window.PropertyHeight - 12); // add row
        SerializedProperty pairs = serialized.FindProperty("both.pairs");
        Assert.That(pairs.arraySize, Is.EqualTo(1));
        Assert.That(pairs.GetArrayElementAtIndex(0).FindPropertyRelative("key").stringValue, Is.Empty);
        Assert.That(pairs.GetArrayElementAtIndex(0).FindPropertyRelative("value").intValue, Is.Zero);
        float rowY = 10 + EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing + 6
            + EditorGUIUtility.singleLineHeight / 2;
        Click(598, rowY); // delete row
        Assert.That(serialized.FindProperty("both.pairs").arraySize, Is.Zero);
        Assert.That(window.Failure, Is.Null);
    }

    private void AddRow(string name, string key, int value) {
        SerializedProperty pairs = serialized.FindProperty(name).FindPropertyRelative("pairs");
        int index = pairs.arraySize++;
        SerializedProperty pair = pairs.GetArrayElementAtIndex(index);
        pair.FindPropertyRelative("key").stringValue = key;
        pair.FindPropertyRelative("value").intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        serialized.Update();
    }

    private void Open(string name) {
        window = ScriptableObject.CreateInstance<OptionTestWindow>();
        window.Serialized = serialized;
        window.PropertyName = name;
        window.position = new Rect(50, 50, 640, 600);
        serialized.FindProperty(name).isExpanded = true;
        window.Show();
    }

    private void Click(float x, float y) {
        Vector2 point = new(x, y);
        window.SendEvent(new Event { type = EventType.MouseDown, button = 0, mousePosition = point });
        window.SendEvent(new Event { type = EventType.MouseUp, button = 0, mousePosition = point });
    }
}
}
