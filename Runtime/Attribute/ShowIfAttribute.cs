using UnityEngine;

namespace SKYNET {
public class ShowIfAttribute : PropertyAttribute {
    
    public string conditionFieldName;
    public object[] expectedValues;

    public ShowIfAttribute(string conditionFieldName, bool expectedValue = true) {
        this.conditionFieldName = conditionFieldName;
        expectedValues = new object[] { expectedValue };
    }

    public ShowIfAttribute(string conditionFieldName, params object[] expectedValues) {
        this.conditionFieldName = conditionFieldName;
        this.expectedValues = expectedValues;
    }
}
}
