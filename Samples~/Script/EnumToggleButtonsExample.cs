using SKYNET;
using UnityEngine;


public class EnumToggleButtonsExample : MonoBehaviour {
    
    [InspectorLabel("攻击类型")]
    [EnumToggleButtons]
    public AttackType attackType;

    [InspectorLabel("Flags枚举")]
    [EnumToggleButtons]
    public SomeEnumF someEnumF;
}