using System;

namespace SKYNET {

/// <summary>One value/label pair for the directly annotated SDictionary's value column.</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class ValueOptionAttribute : Attribute {
    public object Value { get; }
    public string Label { get; }

    public ValueOptionAttribute(object value, string label = null) {
        Value = value;
        Label = label;
    }
}
}
