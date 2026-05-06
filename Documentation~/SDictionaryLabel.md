### SDictionaryLabel

#### 描述
该属性与[SDictionary](./SDictionary.md)配合使用，指定表格键列和值列的列名；

#### 示例
见[SDictionary](./SDictionary.md)

#### 参数
| 参数 | 含义 |
|:----:|------|
|KeyLabel|键名称|
|ValueLabel|值名称|

#### 细节
1. 如果SDictionary不标这个属性，那么列名会显示成“键(\<K>)”、“值\<V>”的形式，例如`SDictionary<string, List<string>> dict`，画出来的列名是“键(string)”、“值(List\<string>)”，如果标了，那么把“键”、“值”替换成`KeyLabel`和`ValueLabel`；
2. 当字典套字典时，该属性仅作用在最外层SDictionary，不会穿透到内部。例如下面这种情形，只有最外层的字典会显示标记的键值名，内层的字典依然显示默认的名称；
```csharp
[SDictionaryLabel("字符串", "子字典")]
public SDictionary<string, SDictionary<int, int>> dictList;
```