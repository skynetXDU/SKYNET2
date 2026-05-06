using System;
using System.Collections.Generic;
using UnityEngine;
using SKYNET;

[Serializable]
public class WeirdRow {

    [InspectorLabel("坏字典字段")]
    public SDictionary<string, SomeClass[][]> badDict;
}

public class Enemy : MonoBehaviour {
    [InspectorLabel("List套List")]
    [TableList]
    public List<List<int>> listList;

    [InspectorLabel("数组套数组")]
    [TableList]
    public int[][] intJagged;

    [InspectorLabel("List套数组")]
    [TableList]
    public List<int[]> listArray;

    [InspectorLabel("数组套List")]
    [TableList]
    public List<int>[] arrayList;

    [InspectorLabel("字典到二维数组")]
    [TableList]
    public SDictionary<string, SomeClass[][]> dictArrayArray;

    [InspectorLabel("字典到List套List")]
    [TableList]
    public SDictionary<string, List<List<int>>> dictListList;

    [InspectorLabel("字典到数组套List")]
    [TableList]
    public SDictionary<string, List<int>[]> dictArrayList;

    [InspectorLabel("字典到List套数组")]
    [TableList]
    [EnumToggleButtons]
    public SDictionary<string, List<int[]>> dictListArray;

    [InspectorLabel("坏字典列表")]
    public List<SDictionary<string, SomeClass[][]>> badDictList;

    [InspectorLabel("坏字典数组")]
    public SDictionary<string, SomeClass[][]>[] badDictArray;

    [InspectorLabel("字典套坏字典")]
    public SDictionary<string, SDictionary<int, SomeClass[][]>> nestedBadDict;

    [InspectorLabel("List套字典套坏字典")]
    public List<SDictionary<string, SDictionary<int, SomeClass[][]>>> listNestedBadDict;

    [InspectorLabel("表格行里藏坏字典")]
    [TableList]
    public List<WeirdRow> weirdRows;

    [InspectorLabel("隐藏坏字典")]
    [ShowIf("field0")]
    public SDictionary<string, SomeClass[][]> hiddenBadDict;

    [InspectorLabel("禁用坏字典")]
    [EnableIf("field1")]
    public SDictionary<string, SomeClass[][]> disabledBadDict;

    [ShowIf("field0")]
    [EnableIf("field1")]
    [InspectorLabel("又隐藏又禁用的坏字典")]
    public SDictionary<string, SomeClass[][]> mixedBadDict;

}
