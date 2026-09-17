using System;
using UnityEngine;

namespace SKYNET {

/// <summary>One value/label pair in a field's single-selection button row.</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class OptionAttribute : PropertyAttribute {
    public object Value { get; }
    public string Label { get; }

    // Keep applyToCollection false: on arrays/lists the options belong to each element.
    public OptionAttribute(object value, string label = null) {
        Value = value;
        Label = label;
    }
}
}
