using System;
using NUnit.Framework;

namespace SKYNET.Editor.Tests {

// These checks do not require a live Unity editor or native SerializedObject instances.
public sealed class OptionConfigurationTests {
    [TestCase("boolean", typeof(bool))] [TestCase("character", typeof(char))]
    [TestCase("unsignedByte", typeof(byte))] [TestCase("signedByte", typeof(sbyte))]
    [TestCase("smallInteger", typeof(short))] [TestCase("unsignedSmallInteger", typeof(ushort))]
    [TestCase("integer", typeof(int))] [TestCase("unsignedInteger", typeof(uint))]
    [TestCase("largeInteger", typeof(long))] [TestCase("unsignedLargeInteger", typeof(ulong))]
    [TestCase("single", typeof(float))] [TestCase("precision", typeof(double))]
    [TestCase("text", typeof(string))] [TestCase("array", typeof(int))]
    [TestCase("list", typeof(float))]
    public void AcceptsOnlyExactElementTypes(string name, Type type) {
        OptionConfiguration configuration = OptionUtil.GetConfiguration(typeof(OptionTestAsset).GetField(name));
        Assert.That(configuration.IsValid, Is.True, configuration.Error);
        Assert.That(configuration.ValueType, Is.EqualTo(type));
        foreach (object value in configuration.Values)
            Assert.That(value.GetType(), Is.EqualTo(type));
    }

    [TestCase("wrongNumber", "float")] [TestCase("wrongString", "int")]
    [TestCase("nullOption", "null")] [TestCase("unsupported", "Vector3")]
    [TestCase("enumValue", "TestEnum")] [TestCase("enumField", "TestEnum")]
    [TestCase("decimalField", "Decimal")]
    public void InvalidValueRejectsWholeGroup(string name, string reportedType) {
        OptionConfiguration configuration = OptionUtil.GetConfiguration(typeof(OptionTestAsset).GetField(name));
        Assert.That(configuration.IsValid, Is.False);
        Assert.That(configuration.Error, Does.Contain(name).And.Contain(reportedType).And.Contain("所有 Option 已停用"));
        Assert.That(configuration.FindSelected(configuration.Values[0]), Is.EqualTo(-1));
    }

    [Test] public void OrderingLabelsAndSelection() {
        OptionTests checks = new();
        checks.MetadataOrderIgnoresDrawerOrderAndConfigurationIsCached();
        checks.LabelsUseInvariantCultureAndEmptyLabelsFallBack();
        checks.SelectionHandlesUnknownMixedDuplicateAndExactFloatingValues();
    }
}
}
