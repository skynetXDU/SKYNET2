# KeyOption 与 ValueOption

为 `SDictionary<K,V>` 的 key、value 列提供一排单选按钮。不需要枚举；两个特性都可重复标注，也可只使用其中一个。

```csharp
[KeyOption("hp", "生命")]
[KeyOption("mp", "魔力")]
[ValueOption(100)]
[ValueOption(200, "加强")]
public SDictionary<string, int> stats;
```

key 列显示“生命 / 魔力”，点击后写入 `"hp"` 或 `"mp"`；value 列显示“100 / 加强”，点击后写入 `100` 或 `200`。没有对应特性的列保持原来的编辑控件。

## 参数与类型

构造接口分别是 `KeyOptionAttribute(object value, string label = null)` 和 `ValueOptionAttribute(object value, string label = null)`。第一个参数是实际值，第二个参数是显示文字。文字省略、为 null 或空字符串时使用实际值的文字；空字符串值显示为 `""`，数字使用固定文化格式。

类型范围与 [Option](./Option.md) 一致：`bool`、`char`、`byte`、`sbyte`、`short`、`ushort`、`int`、`uint`、`long`、`ulong`、`float`、`double`、`string`。key 选项必须严格匹配 `K`，value 选项必须严格匹配 `V`，不做隐式转换。

```csharp
[ValueOption(1f)]
[ValueOption(2f)]
public SDictionary<string, float> multipliers;
```

null、枚举、decimal、自定义 struct、集合和嵌套字典都不能作为选项值。另一列仍可使用 Unity 支持序列化的复杂类型并正常编辑。如果字典本身因泛型类型无法被 Unity 序列化而不显示，选项特性不会使其重新显示。

## 每列独立校验，错误时整列回退

```csharp
[KeyOption("hp")]
[KeyOption("mp")]
[ValueOption(100)]
[ValueOption(200f)] // float 与 V=int 不匹配
public SDictionary<string, int> stats;
```

上述 value 列的所有选项失效，恢复普通整数输入框；key 列仍显示按钮。错误提示位于字典展开区域的表头上方，每个错误列提示一次，不随行数重复，空字典也能显示。提示包含字段、列、期望类型和错误选项的类型。不自动修改已有数据，也不在每次重绘时向 Console 输出错误。

## 选择、集合与嵌套

- 每列分别按特性的声明顺序显示选项。当前值不在选项中时没有按钮高亮；重复选项值只高亮第一个匹配项；浮点数使用自身的 `Equals` 比较。
- 新增行沿用原有默认值，不自动选中第一项，也不分配未使用的 key。
- 重复 key 沿用 SDictionary 现有规则：反序列化构建字典时后者覆盖前者并警告。不会禁用已使用 key 的按钮或限制添加行数。
- 标注在字典数组或 `List<SDictionary<K,V>>` 上时，应用于各字典元素的直接 key/value。
- 不向嵌套字典或复杂值的子字段传播选项；复杂值的字段可以另外使用普通 `[Option]`。外层 value 是字典或集合时，对其标注基础值的 ValueOption 属于类型错误，不会自动深入内部。
- 标注在非字典字段上的 KeyOption / ValueOption 不生效。普通字段继续使用 `[Option]`。
- 支持 InspectorLabel、SDictionaryLabel、ShowIf、EnableIf、列宽拖动和独立编辑窗口。
- 多对象值不一致时不高亮具体选项；点击后统一写入所选单元格。写入使用 SerializedProperty，支持 Undo/Redo 与 Prefab 修改记录。
- 这是 Inspector 的编辑方式，不是运行时校验：代码仍然可以写入选项之外的值。

```csharp
[KeyOption("hp")]
[ValueOption(100)]
[ValueOption(200)]
public List<SDictionary<string, int>> characters;
```

## 示例与人工检查

将测试项目的 `Assets/SKYNETexample/Script/DictionaryOptionExample.cs` 挂到空物体即可使用；包内对应示例位于 `Samples~/Script/DictionaryOptionExample.cs`。

建议检查以下场景：

1. 分别点击 key 和 value 按钮，确认只修改对应单元格；单列配置时另一列仍可自由编辑。
2. 展开错误示例和空字典，检查提示只显示一次、错误列整体回退、有效列仍显示按钮。
3. 调整列宽、展开字典数组／列表、切换条件控制，并在独立窗口内检查同样的行为。
4. 添加行时保持默认值；选择已有 key 不受阻止；删除行、Undo/Redo 和 Prefab 覆盖按原方式工作。
5. 多选具有相同行数但不同值的组件，确认混合状态不高亮，点击后对应单元格统一赋值。
6. 外层字典的选项不出现在子字典中；现有 OptionExample 的普通字段按钮行为保持不变。

Editor 测试代码包括 `DictionaryOptionConfigurationTests` 和 `DictionaryOptionTests`，沿用现有测试程序集；实际 Unity 测试由使用者运行。
