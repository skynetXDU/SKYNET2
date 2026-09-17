using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKYNET.Editor.Tests {

public sealed class DictionaryOptionTestAsset : ScriptableObject {
    [KeyOption("hp", "生命")] [ValueOption(100)]
    [KeyOption("mp", "魔力")] [ValueOption(200, "加强")]
    public SDictionary<string, int> both = new();
    [KeyOption("hp")] public SDictionary<string, int> keyOnly = new();
    [ValueOption(100)] public SDictionary<string, int> valueOnly = new();
    [KeyOption("hp")] [KeyOption(1)] [ValueOption(100)]
    public SDictionary<string, int> badKey = new();
    [KeyOption("hp")] [ValueOption(100)] [ValueOption(200f)]
    public SDictionary<string, int> badValue = new();
    [KeyOption(null)] [ValueOption(null)] public SDictionary<string, string> nulls = new();
    [ValueOption(1)] public SDictionary<string, Vector3> unsupported = new();
    [ValueOption(1)] public SDictionary<string, int[]> containerValue = new();
    [KeyOption("outer")] [ValueOption(1)]
    public SDictionary<string, SDictionary<string, int>> nested = new();
    [KeyOption("hp")] [ValueOption(100)]
    public SDictionary<string, int>[] array = { new() };
    [KeyOption("hp")] [ValueOption(100)]
    public List<SDictionary<string, int>> list = new() { new() };
    [KeyOption(1)] [ValueOption(2)] [Option(3)] public int ordinary;
    [KeyOption("hp", "")] [KeyOption("hp", "别名")]
    [ValueOption(1.5f)] [ValueOption(2f, null)]
    public SDictionary<string, float> labels = new();

    [KeyOption(false)] [KeyOption(true)] [ValueOption(false)] [ValueOption(true)]
    public SDictionary<bool, bool> boolean = new();
    [KeyOption('A')] [KeyOption('\uffff')] [ValueOption('A')] [ValueOption('\uffff')]
    public SDictionary<char, char> character = new();
    [KeyOption((byte)0)] [KeyOption(byte.MaxValue)] [ValueOption((byte)0)] [ValueOption(byte.MaxValue)]
    public SDictionary<byte, byte> unsignedByte = new();
    [KeyOption(sbyte.MinValue)] [KeyOption(sbyte.MaxValue)] [ValueOption(sbyte.MinValue)] [ValueOption(sbyte.MaxValue)]
    public SDictionary<sbyte, sbyte> signedByte = new();
    [KeyOption(short.MinValue)] [KeyOption(short.MaxValue)] [ValueOption(short.MinValue)] [ValueOption(short.MaxValue)]
    public SDictionary<short, short> smallInteger = new();
    [KeyOption((ushort)0)] [KeyOption(ushort.MaxValue)] [ValueOption((ushort)0)] [ValueOption(ushort.MaxValue)]
    public SDictionary<ushort, ushort> unsignedSmallInteger = new();
    [KeyOption(int.MinValue)] [KeyOption(int.MaxValue)] [ValueOption(int.MinValue)] [ValueOption(int.MaxValue)]
    public SDictionary<int, int> integer = new();
    [KeyOption(0u)] [KeyOption(uint.MaxValue)] [ValueOption(0u)] [ValueOption(uint.MaxValue)]
    public SDictionary<uint, uint> unsignedInteger = new();
    [KeyOption(long.MinValue)] [KeyOption(long.MaxValue)] [ValueOption(long.MinValue)] [ValueOption(long.MaxValue)]
    public SDictionary<long, long> largeInteger = new();
    [KeyOption(0ul)] [KeyOption(ulong.MaxValue)] [ValueOption(0ul)] [ValueOption(ulong.MaxValue)]
    public SDictionary<ulong, ulong> unsignedLargeInteger = new();
    [KeyOption(float.MinValue)] [KeyOption(float.NaN)] [ValueOption(float.MinValue)] [ValueOption(float.NaN)]
    public SDictionary<float, float> single = new();
    [KeyOption(1.2345678901234567d)] [KeyOption(double.MaxValue)]
    [ValueOption(1.2345678901234567d)] [ValueOption(double.MaxValue)]
    public SDictionary<double, double> precision = new();
    [KeyOption("")] [KeyOption("foo")] [ValueOption("")] [ValueOption("foo")]
    public SDictionary<string, string> text = new();

    public bool visible = true;
    public bool editable = true;
    [InspectorLabel("条件字典")] [SDictionaryLabel("属性", "数值")]
    [ShowIf(nameof(visible))] [EnableIf(nameof(editable))]
    [KeyOption("hp")] [ValueOption(100)] [ValueOption(200)]
    public SDictionary<string, int> conditional = new();
    [ShowIf(nameof(visible))] [KeyOption("hp")] [ValueOption(1f)]
    public List<SDictionary<string, int>> conditionalList = new() { new() };
}
}
