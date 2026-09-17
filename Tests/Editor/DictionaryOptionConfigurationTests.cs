using System;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;

namespace SKYNET.Editor.Tests {

public sealed class DictionaryOptionConfigurationTests {
    internal static FieldInfo Field(string name) => typeof(DictionaryOptionTestAsset).GetField(name);

    [TestCase("boolean", typeof(bool))] [TestCase("character", typeof(char))]
    [TestCase("unsignedByte", typeof(byte))] [TestCase("signedByte", typeof(sbyte))]
    [TestCase("smallInteger", typeof(short))] [TestCase("unsignedSmallInteger", typeof(ushort))]
    [TestCase("integer", typeof(int))] [TestCase("unsignedInteger", typeof(uint))]
    [TestCase("largeInteger", typeof(long))] [TestCase("unsignedLargeInteger", typeof(ulong))]
    [TestCase("single", typeof(float))] [TestCase("precision", typeof(double))]
    [TestCase("text", typeof(string))]
    public void SupportsExactPrimitiveTypesOnBothColumns(string name, Type type) {
        foreach (OptionConfiguration configuration in new[] {
            OptionUtil.GetKeyConfiguration(Field(name)), OptionUtil.GetValueConfiguration(Field(name)) }) {
            Assert.That(configuration.IsValid, Is.True, configuration.Error);
            Assert.That(configuration.ValueType, Is.EqualTo(type));
            foreach (object value in configuration.Values) Assert.That(value.GetType(), Is.EqualTo(type));
        }
    }

    [Test] public void ColumnCachesAndDeclarationOrdersAreIndependent() {
        FieldInfo field = Field("both");
        OptionConfiguration keys = OptionUtil.GetKeyConfiguration(field);
        OptionConfiguration values = OptionUtil.GetValueConfiguration(field);
        CollectionAssert.AreEqual(new object[] { "hp", "mp" }, keys.Values);
        CollectionAssert.AreEqual(new object[] { 100, 200 }, values.Values);
        Assert.That(keys.Labels[0].text, Is.EqualTo("生命"));
        Assert.That(values.Labels[1].text, Is.EqualTo("加强"));
        Assert.That(OptionUtil.GetKeyConfiguration(field), Is.SameAs(keys));
        Assert.That(OptionUtil.GetValueConfiguration(field), Is.SameAs(values));
        Assert.That(OptionUtil.GetConfiguration(field).Values, Is.Empty);
    }

    [TestCase("keyOnly", true)] [TestCase("valueOnly", false)]
    public void MissingColumnHasNeitherButtonsNorError(string name, bool keyOnly) {
        OptionConfiguration present = keyOnly ? OptionUtil.GetKeyConfiguration(Field(name)) : OptionUtil.GetValueConfiguration(Field(name));
        OptionConfiguration absent = keyOnly ? OptionUtil.GetValueConfiguration(Field(name)) : OptionUtil.GetKeyConfiguration(Field(name));
        Assert.That(present.IsValid, Is.True);
        Assert.That(absent.IsValid, Is.False);
        Assert.That(absent.Error, Is.Null);
    }

    [TestCase("badKey", true, "int")] [TestCase("badValue", false, "float")]
    public void BadOptionDisablesOnlyItsWholeColumn(string name, bool keyIsBad, string wrongType) {
        OptionConfiguration bad = keyIsBad ? OptionUtil.GetKeyConfiguration(Field(name)) : OptionUtil.GetValueConfiguration(Field(name));
        OptionConfiguration good = keyIsBad ? OptionUtil.GetValueConfiguration(Field(name)) : OptionUtil.GetKeyConfiguration(Field(name));
        Assert.That(bad.IsValid, Is.False);
        Assert.That(bad.Error, Does.Contain(name).And.Contain(wrongType).And.Contain(keyIsBad ? "key 列" : "value 列"));
        Assert.That(bad.FindSelected(bad.Values[0]), Is.EqualTo(-1));
        Assert.That(good.IsValid, Is.True);
    }

    [TestCase("nulls")] [TestCase("unsupported")] [TestCase("containerValue")] [TestCase("nested")]
    public void RejectsNullAndComplexValuesWithoutUnwrappingThem(string name) {
        Assert.That(OptionUtil.GetValueConfiguration(Field(name)).IsValid, Is.False);
        Assert.That(OptionUtil.GetValueConfiguration(Field(name)).Error, Is.Not.Null);
        if (name == "nulls") Assert.That(OptionUtil.GetKeyConfiguration(Field(name)).Error, Is.Not.Null);
        if (name == "nested") Assert.That(OptionUtil.GetKeyConfiguration(Field(name)).IsValid, Is.True);
    }

    [TestCase("array")] [TestCase("list")]
    public void DictionaryCollectionsResolveKAndV(string name) {
        Assert.That(OptionUtil.GetKeyConfiguration(Field(name)).ValueType, Is.EqualTo(typeof(string)));
        Assert.That(OptionUtil.GetValueConfiguration(Field(name)).ValueType, Is.EqualTo(typeof(int)));
        Assert.That(OptionUtil.GetKeyConfiguration(Field(name)).IsValid, Is.True);
        Assert.That(OptionUtil.GetValueConfiguration(Field(name)).IsValid, Is.True);
    }

    [Test] public void NonDictionaryAndNestedDictionaryDoNotInheritColumnOptions() {
        Assert.That(OptionUtil.GetKeyConfiguration(Field("ordinary")).Values, Is.Empty);
        Assert.That(OptionUtil.GetValueConfiguration(Field("ordinary")).Values, Is.Empty);
        Assert.That(OptionUtil.GetConfiguration(Field("ordinary")).IsValid, Is.True);
        FieldInfo inner = typeof(SPair<string, SDictionary<string, int>>).GetField("value");
        Assert.That(OptionUtil.GetKeyConfiguration(inner).Values, Is.Empty);
        Assert.That(OptionUtil.GetValueConfiguration(inner).Values, Is.Empty);
    }

    [Test] public void LabelsAndSelectionMatchOrdinaryOptionRules() {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            OptionConfiguration keys = OptionUtil.GetKeyConfiguration(Field("labels"));
            OptionConfiguration values = OptionUtil.GetValueConfiguration(Field("labels"));
            Assert.That(keys.Labels[0].text, Is.EqualTo("hp"));
            Assert.That(keys.FindSelected("hp"), Is.EqualTo(0));
            Assert.That(keys.FindSelected("unknown"), Is.EqualTo(-1));
            Assert.That(keys.FindSelected("hp", true), Is.EqualTo(-1));
            Assert.That(values.Labels[0].text, Is.EqualTo("1.5"));
            Assert.That(values.Labels[1].text, Is.EqualTo("2"));
            Assert.That(OptionUtil.GetValueConfiguration(Field("text")).Labels[0].text, Is.EqualTo("\"\""));
            Assert.That(OptionUtil.GetValueConfiguration(Field("single")).FindSelected(float.NaN), Is.EqualTo(1));
            Assert.That(OptionUtil.GetValueConfiguration(Field("precision")).FindSelected(1.234567890123456d), Is.EqualTo(-1));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
}
}
