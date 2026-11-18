using System.Collections.Generic;
using RPGFramework.Core.Memory;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    [CustomPropertyDrawer(typeof(VariableNameAttribute))]
    internal sealed class VariableNameDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VariableWidth width = ((VariableNameAttribute)attribute).Width;
            List<string>  names = new List<string>(VariableNames.InProject(width));

            names.Insert(0, ProviderNames.NONE);

            DropdownField field = new DropdownField(property.displayName, names, 0) { tooltip = property.tooltip };

            // A name the map does not hold stays as it was, for the build's check to report.
            field.SetValueWithoutNotify(string.IsNullOrEmpty(property.stringValue) ? ProviderNames.NONE : property.stringValue);
            field.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            field.RegisterValueChangedCallback(e =>
            {
                property.stringValue = e.newValue == ProviderNames.NONE ? string.Empty : e.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            return field;
        }
    }
}
