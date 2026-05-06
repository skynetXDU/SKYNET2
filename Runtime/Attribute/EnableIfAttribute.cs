using UnityEngine;

namespace SKYNET {
    public class EnableIfAttribute : PropertyAttribute {

        public string conditionFieldName;
        public object[] expectedValues;

        public EnableIfAttribute(string conditionFieldName, bool expectedValue = true) {
            this.conditionFieldName = conditionFieldName;
            expectedValues = new object[] { expectedValue };
        }

        public EnableIfAttribute(string conditionFieldName, params object[] expectedValues) {
            this.conditionFieldName = conditionFieldName;
            this.expectedValues = expectedValues;
        }
    }
}