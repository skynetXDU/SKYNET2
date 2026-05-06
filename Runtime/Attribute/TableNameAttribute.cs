using System;
using UnityEngine;

namespace SKYNET {
[AttributeUsage(AttributeTargets.Field)]
public class TableNameAttribute : PropertyAttribute {

    public string Name { get; private set; }

    public TableNameAttribute(string name) {
        Name = name;
    }
}
}
