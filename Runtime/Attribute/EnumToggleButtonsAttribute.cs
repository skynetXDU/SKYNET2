using UnityEngine;

namespace SKYNET {
public class EnumToggleButtonsAttribute : PropertyAttribute {

    // 这里不设applyToCollection, 目的是让该注解能穿透到列表元素上
    // 也就是支持下面的写法:
    // [InspectorLabel("一个枚举列表")]
    // [EnumToggleButtons]
    // [TableList]
    // public List<SomeEnum> someEnums;
    // 列表里的每一个元素都会被展开成一排按钮
    public EnumToggleButtonsAttribute() {}
}
}
