using System;
using System.Collections.Generic;
using SKYNET;
using UnityEngine;

/// <summary>Attach to an empty GameObject to try Option buttons and invalid configurations.</summary>
public class OptionExample : MonoBehaviour {
    [InspectorLabel("显示条件选项")]
    public bool showOptions = true;
    [InspectorLabel("允许编辑条件选项")]
    public bool enableOptions = true;

    [InspectorLabel("整数选项")]
    [Option(1, "one")]
    [Option(2, "two")]
    [Option(3)]
    public int number = 2;

    [InspectorLabel("字符串选项")]
    [Option("foo", "选项一")]
    [Option("bar")]
    [Option("")]
    public string text = "bar";

    [Option(false, "关闭")] [Option(true, "开启")]
    public bool boolean;
    [Option('A')] [Option('中')]
    public char character = 'A';
    [Option((byte)0)] [Option(byte.MaxValue)]
    public byte unsignedByte;
    [Option(sbyte.MinValue)] [Option(sbyte.MaxValue)]
    public sbyte signedByte;
    [Option(short.MinValue)] [Option(short.MaxValue)]
    public short smallInteger;
    [Option((ushort)0)] [Option(ushort.MaxValue)]
    public ushort unsignedSmallInteger;
    [Option(0u)] [Option(uint.MaxValue)]
    public uint unsignedInteger;
    [Option(long.MinValue, "最小 long")] [Option(long.MaxValue, "最大 long")]
    public long largeInteger;
    [Option(0ul)] [Option(ulong.MaxValue, "最大 ulong")]
    public ulong unsignedLargeInteger;
    [Option(0.5f)] [Option(1f)] [Option(2f)]
    public float speed = 1f;
    [Option(1.2345678901234567d, "高精度数值")] [Option(double.MaxValue, "最大 double")]
    public double precision;

    [InspectorLabel("条件控制")]
    [ShowIf(nameof(showOptions))] [EnableIf(nameof(enableOptions))]
    [Option(1)] [Option(2)]
    public int conditional;

    [InspectorLabel("数组元素")]
    [Option(1)] [Option(2)] [Option(3)]
    public int[] array = { 1, 2, 99 };

    [InspectorLabel("字符串列表")]
    [Option("foo")] [Option("bar")]
    public List<string> list = new() { "foo", "bar" };

    [InspectorLabel("布尔表格")]
    [TableList] [Option(false, "关闭")] [Option(true, "开启")]
    public List<bool> tableFlags = new() { false, true };

    [InspectorLabel("表格中的选项")]
    [TableList]
    public List<OptionExampleRow> rows = new() { new OptionExampleRow() };

    [InspectorLabel("无匹配值：不自动修改")]
    [Option(1)] [Option(2)]
    public int unmatched = 99;

    [InspectorLabel("重复值：首项高亮")]
    [Option(1, "第一个")] [Option(1, "同值别名")] [Option(2)]
    public int duplicate = 1;

    [InspectorLabel("错误示例：整体回退")]
    [Option(1, "one")] [Option(2f, "two")] [Option(3, "three")]
    public int invalidNumber = 42;

    [InspectorLabel("错误示例：null")]
    [Option("foo")] [Option(null)]
    public string invalidNull = "保持原值";

    [InspectorLabel("错误示例：不支持 Vector3")]
    [Option(1)]
    public Vector3 unsupported = Vector3.one;
}

[Serializable]
public class OptionExampleRow {
    [TableName("启用")]
    [Option(false, "关")] [Option(true, "开")]
    public bool enabled;
    [TableName("等级")]
    [Option(1, "低")] [Option(2, "中")] [Option(3, "高")]
    public int level = 1;
}
