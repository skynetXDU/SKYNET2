# Option

让基本类型字段显示为一排单选按钮，不需要预先声明枚举。一个 `Option` 表示一个选项，可在同一字段上重复标注。

```csharp
using SKYNET;
using UnityEngine;

public class Example : MonoBehaviour {
    [Option(1, "one")]
    [Option(2, "two")]
    [Option(3)]
    public int number;

    [Option("foo", "选项一")]
    [Option("bar")]
    public string text;
}
```

## 参数与类型

接口为 `OptionAttribute(object value, string label = null)`。`value` 是点击后写入字段的值；`label` 是按钮文字。文字省略、为 `null` 或空字符串时，显示实际值；空字符串值显示为 `""`。数字使用固定文化格式，不随系统小数点格式变化。

支持 `bool`、`char`、`byte`、`sbyte`、`short`、`ushort`、`int`、`uint`、`long`、`ulong`、`float`、`double`、`string`。不支持枚举、decimal、自定义 struct、null 选项值或动态选项来源。Unity 不序列化的字段（例如 decimal）本身不会显示，因而也无法显示 Inspector 提示。

选项类型必须与字段类型完全相同，不执行数值转换：

```csharp
[Option(1L)] public long count;
[Option(1f)] public float speed;
[Option(1d)] public double factor;
[Option((byte)1)] public byte mode;
[Option((short)1)] public short level;
```

## 任意错误使整组失效

```csharp
[Option(1, "one")]
[Option(2f, "two")] // float 与字段 int 不匹配
[Option(3)]
public int number = 42;
```

此时不会绘制任何 Option 按钮，而是恢复普通字段控件，并显示字段名、字段类型和错误选项类型。当前值 42 保持不变；不会只保留其中有效的按钮，也不会每次重绘向 Console 输出错误。

校验发生在 Inspector，不是编译期诊断。

## 选择与组合使用

- 按代码声明顺序显示按钮，不受各特性的 `order` 影响。
- 只有点击才写入。当前值无匹配项时，没有按钮高亮，原值保持。
- 重复值允许存在，只有第一个匹配项高亮。浮点数使用对应类型的 `Equals`，不使用近似比较。
- 支持 `InspectorLabel`、`ShowIf`、`EnableIf`。隐藏字段时同时隐藏错误提示；禁用状态下不能修改。
- 标注在数组或 `List<T>` 上时，对每个元素显示按钮，并按元素类型校验。
- 支持 `TableList` 单元格，包括 bool 单元格，也支持表格的独立编辑窗口。
- 多选对象的当前值不一致时不选中任何按钮；点击后统一赋值。修改使用 Unity 序列化机制，支持 Undo/Redo 和 Prefab 覆盖记录。

```csharp
[InspectorLabel("等级列表")]
[TableList]
[Option(1, "低")]
[Option(2, "中")]
[Option(3, "高")]
public int[] levels;
```

参见 `Samples~/Script/OptionExample.cs`。在测试项目中，将 `Assets/SKYNETexample/Script/OptionExample.cs` 挂到空物体即可检查正常按钮、集合、条件控制、表格和错误回退。

## 测试

包内 `Tests/Editor` 包含配置校验、序列化读写、长整数与浮点精度、多对象、Undo/Redo、Prefab 以及 IMGUI 交互测试。消费项目需安装 Unity Test Framework，并在 `Packages/manifest.json` 的顶层 `testables` 数组中加入 `"com.skynets.skynet"`，才能在 Test Runner 的 EditMode 中运行。IMGUI 交互测试需要可创建 EditorWindow 的编辑器环境。
