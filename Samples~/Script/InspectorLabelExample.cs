using System;
using SKYNET;
using UnityEngine;

[Serializable]
public class Character {

    [InspectorLabel("名称")]
    public string name;
    [InspectorLabel("战力")]
    public int stats;
}

public class InspectorLabelExample : MonoBehaviour {
    
    [InspectorLabel("角色名称")]
    public string characterName;

    [InspectorLabel("角色")]
    public Character character;
}
