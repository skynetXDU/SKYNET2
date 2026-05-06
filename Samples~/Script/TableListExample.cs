using System;
using System.Collections.Generic;
using SKYNET;
using UnityEngine;

[Serializable]
public class Location {
    [TableName("索引")]
    public int id;

    [TableName("名称")]
    public string name;

    [TableName("位置")]
    public Vector3 posotion;
}

public class TableListExample : MonoBehaviour {

    [InspectorLabel("地点列表")]
    [TableList]
    public List<Location> locations;
}
