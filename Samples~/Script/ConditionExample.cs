using UnityEngine;
using SKYNET;

public enum AttackType {
    Melee,
    Projectile,
    Area
}

public class ConditionExample : MonoBehaviour {
    [InspectorLabel("启用高级设置")]
    public bool advanced;

    [InspectorLabel("攻击类型"), EnumToggleButtons]
    public AttackType attackType;

    [InspectorLabel("高级数值"), ShowIf("advanced", "aaa")]
    public float advancedValue;

    [InspectorLabel("近战范围"), ShowIf("attackType", AttackType.Melee)]
    public float meleeRange;

    [InspectorLabel("投射物"), EnableIf("attackType", AttackType.Projectile)]
    public GameObject projectilePrefab;
}