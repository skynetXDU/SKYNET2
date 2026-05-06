using System;
using UnityEditor;

namespace SKYNET.Editor {

public class ConditionUtil {

    public static bool MatchesCondition(SerializedProperty property, string conditionFieldName, object[] expectedValues) {
        SerializedProperty condProperty = FindConditionProperty(property, conditionFieldName);

        if (condProperty == null)
            return true;

        return condProperty.propertyType switch {
            SerializedPropertyType.Boolean => MatchesBoolean(condProperty, expectedValues),
            SerializedPropertyType.Enum => MatchesEnum(condProperty, expectedValues),
            _ => true
        };
    }

    private static bool MatchesBoolean(SerializedProperty condProperty, object[] expectedValues) {
        if (expectedValues == null || expectedValues.Length == 0 || expectedValues[0] is not bool expectedBool)
            return true;

        return condProperty.boolValue == expectedBool;
    }

    private static bool MatchesEnum(SerializedProperty condProperty, object[] expectedValues) {
        if (expectedValues == null || expectedValues.Length == 0)
            return true;

        bool hasExpectedEnum = false;

        foreach (object expectedValue in expectedValues) {
            if (expectedValue is not Enum expectedEnum)
                continue;

            hasExpectedEnum = true;

            Type enumType = expectedEnum.GetType();

            if (enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0) {
                int currentValue = condProperty.enumValueFlag;
                int expectedFlag = Convert.ToInt32(expectedEnum);

                if (expectedFlag == 0) {
                    if (currentValue == 0)
                        return true;
                }
                else if ((currentValue & expectedFlag) == expectedFlag) {
                    return true;
                }

                continue;
            }

            int enumValueIndex = condProperty.enumValueIndex;

            if (enumValueIndex < 0 || enumValueIndex >= condProperty.enumNames.Length)
                continue;

            string currentEnumName = condProperty.enumNames[enumValueIndex];

            if (currentEnumName == expectedEnum.ToString())
                return true;
        }

        return !hasExpectedEnum;
    }


    private static SerializedProperty FindConditionProperty(SerializedProperty property, string conditionFieldName) {
        if (string.IsNullOrEmpty(conditionFieldName))
            return null;

        string propertyPath = property.propertyPath;
        int index = propertyPath.LastIndexOf(".");

        string condPropPath = index >= 0 ? $"{propertyPath[..index]}.{conditionFieldName}" : conditionFieldName;

        return property.serializedObject.FindProperty(condPropPath);
    }
}
}
