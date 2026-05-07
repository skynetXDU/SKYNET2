### InspectorLabelAttribute

#### 描述
该特性用在MonoBehaviour或Serializable类的字段上，用来控制字段在Inspector中显示的名称。

#### 示例
```csharp
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
```

![InspectorLabel](images/inspector_label_example.png)

#### 参数
| 参数 | 含义 |
|:----:|------|
|Label|要在Inspector里显示的名字|

#### 细节
1. 因为`InspectorLabel`的构造函数中用到了[applyToCollection](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PropertyAttribute-applyToCollection.html)，而这个API直到unity6才有，所以不支持unity6以下的版本，后续会考虑如何兼容；
2. 如果不用这个参数，那么当InspectorLabel作用到列表时，标签名会出现在每一个列表项上，而不是整个字段上，感觉怪怪的；