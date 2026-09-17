using System.Collections.Generic;
using SKYNET;
using UnityEngine;

namespace SKYNET {

public class DictionaryOptionExample : MonoBehaviour {
    [InspectorLabel("仅限制 value")]
    [ValueOption(1, "低")] [ValueOption(2, "中")] [ValueOption(3, "高")]
    public SDictionary<string, int> levels = Create("自定义名称", 2);

    [InspectorLabel("仅限制 key")]
    [KeyOption("hp", "生命")] [KeyOption("mp", "魔力")]
    public SDictionary<string, int> attributes = Create("hp", 123);

    [InspectorLabel("同时限制两列")]
    [SDictionaryLabel("属性", "数值")]
    [KeyOption("hp", "生命")] [KeyOption("mp", "魔力")]
    [ValueOption(100)] [ValueOption(200, "加强")]
    public SDictionary<string, int> stats = Create("hp", 100);

    [InspectorLabel("错误 key 列：value 仍显示按钮")]
    [KeyOption("hp")] [KeyOption(1)]
    [ValueOption(100)] [ValueOption(200)]
    public SDictionary<string, int> invalidKey = Create("保留", 100);

    [InspectorLabel("错误 value 列：key 仍显示按钮")]
    [KeyOption("hp")] [KeyOption("mp")]
    [ValueOption(100)] [ValueOption(200f)]
    public SDictionary<string, int> invalidValue = Create("hp", 42);

    [InspectorLabel("空字典也显示错误")]
    [KeyOption(null)] [ValueOption("类型错误")]
    public SDictionary<string, int> invalidEmpty = new();

    [InspectorLabel("未匹配值不自动修改")]
    [KeyOption("hp")] [ValueOption(100)]
    public SDictionary<string, int> unmatched = Create("unknown", 999);

    [InspectorLabel("字典列表")]
    [KeyOption("hp")] [KeyOption("mp")]
    [ValueOption(100)] [ValueOption(200)]
    public List<SDictionary<string, int>> list = new() { Create("hp", 100), Create("mp", 200) };

    [InspectorLabel("字典数组")]
    [KeyOption("hp")] [ValueOption(100)]
    public SDictionary<string, int>[] array = { Create("hp", 100) };

    public bool show = true;
    public bool editable = true;
    [InspectorLabel("条件控制")]
    [ShowIf(nameof(show))] [EnableIf(nameof(editable))]
    [KeyOption("hp")] [ValueOption(100)] [ValueOption(200)]
    public SDictionary<string, int> conditional = Create("hp", 100);

    [InspectorLabel("只约束外层 key，不传入子字典")]
    [KeyOption("角色一")] [KeyOption("角色二")]
    public SDictionary<string, SDictionary<string, int>> nested = new();

    private static SDictionary<string, int> Create(string key, int value) {
        SDictionary<string, int> result = new();
        result[key] = value;
        return result;
    }
}

}
