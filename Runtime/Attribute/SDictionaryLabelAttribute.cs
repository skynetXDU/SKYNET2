using System;
using UnityEngine;

namespace SKYNET {
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SDictionaryLabelAttribute : PropertyAttribute {
    public string KeyLabel { get; private set; }
    public string ValueLabel { get; private set; }

    public SDictionaryLabelAttribute(string keyLabel, string valueLabel) {
        KeyLabel = keyLabel;
        ValueLabel = valueLabel;
    }
}
}
