using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKYNET.Editor.Tests {

public sealed class OptionTestAsset : ScriptableObject {
    [Option(false)] [Option(true)] public bool boolean;
    [Option('A')] [Option('\uffff')] public char character;
    [Option((byte)0)] [Option(byte.MaxValue)] public byte unsignedByte;
    [Option(sbyte.MinValue)] [Option(sbyte.MaxValue)] public sbyte signedByte;
    [Option(short.MinValue)] [Option(short.MaxValue)] public short smallInteger;
    [Option((ushort)0)] [Option(ushort.MaxValue)] public ushort unsignedSmallInteger;
    [Option(int.MinValue)] [Option(int.MaxValue)] public int integer;
    [Option(0u)] [Option(uint.MaxValue)] public uint unsignedInteger;
    [Option(long.MinValue)] [Option(long.MaxValue)] public long largeInteger;
    [Option(0ul)] [Option(ulong.MaxValue)] public ulong unsignedLargeInteger;
    [Option(float.MinValue)] [Option(float.MaxValue)] [Option(float.NaN)] public float single;
    [Option(double.MinValue)] [Option(double.MaxValue)] [Option(1.2345678901234567d)] public double precision;
    [Option("")] [Option("foo")] public string text;

    [Option(3, "third", order = 10)]
    [Option(1, "first", order = -10)]
    [Option(2, "second")]
    public int ordered;
    [Option(1, "first")] [Option(1, "alias")] public int duplicates;
    [Option(1)] [Option(2, "")] [Option(3, null)] public int defaultLabels;
    [Option(1)] [Option(2f, "two")] [Option(3)] public int wrongNumber = 42;
    [Option("foo")] [Option(2)] public string wrongString = "keep";
    [Option(null)] public string nullOption = "keep";
    [Option(1)] public Vector3 unsupported = Vector3.one;
    [Option(TestEnum.First)] public int enumValue;
    [Option(1)] public TestEnum enumField;
    [Option(1)] public decimal decimalField;

    public bool visible = true;
    public bool editable = true;
    [Option(1)] [Option(2)] [InspectorLabel("中文字段")]
    [ShowIf(nameof(visible))] [EnableIf(nameof(editable))]
    public int conditional;
    [InspectorLabel("错误字段")] [ShowIf(nameof(visible))] [EnableIf(nameof(editable))]
    [Option(1)] [Option("wrong")]
    public int conditionalError;
    [Option(1)] [Option(2)] [ShowIf(nameof(visible))] [EnableIf(nameof(editable))]
    public int[] array = { 0, 1 };
    [Option(1f)] [Option(2f)] public List<float> list = new() { 0f, 1f };
    [TableList] [Option(false, "关")] [Option(true, "开")]
    public List<bool> tableFlags = new() { false };
    [TableList] public List<OptionTestRow> rows = new() { new OptionTestRow() };

    public enum TestEnum { First }
}

[Serializable]
public sealed class OptionTestRow {
    [Option(false, "关")] [Option(true, "开")] public bool flag;
    [Option(1)] [Option(2)] public int number;
    [Option(1)] [Option(2f)] public int error = 8;
}
}
