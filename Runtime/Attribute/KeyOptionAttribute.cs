using System;

namespace SKYNET {

/// <summary>One key value/label pair for the directly annotated SDictionary.</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class KeyOptionAttribute : Attribute {
    public object Value { get; }
    public string Label { get; }

    public KeyOptionAttribute(object value, string label = null) {
        Value = value;
        Label = label;
    }
}
}
