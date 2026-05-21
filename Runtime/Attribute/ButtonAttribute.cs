using System;

namespace SKYNET {
    
[AttributeUsage(AttributeTargets.Method)]
public class ButtonAttribute : Attribute {
    public string Label{ get; }

    public ButtonAttribute(string label = "") {
        Label = label;
    }
}

}

