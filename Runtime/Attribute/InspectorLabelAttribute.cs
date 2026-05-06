using UnityEngine;

namespace SKYNET {
    public class InspectorLabelAttribute : PropertyAttribute {
        public string Label { get; private set; }

        public InspectorLabelAttribute(string label) : base(true) {
            Label = label;
        }
    }
}
