using System.Collections.Generic;
using RPGFramework.Core.Audio;
using RPGFramework.Hashing;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// A sound effect's name hash, chosen by name from the project's sound effect providers; None is zero, and a hash
    /// no sound has any more shows blank.
    /// </summary>
    [CustomPropertyDrawer(typeof(SoundNameAttribute))]
    internal sealed class SoundNameDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            List<string> names = ProviderNames.Sounds();

            names.Insert(0, ProviderNames.NONE);

            int chosen = property.ulongValue == 0 ? 0 : ProviderNames.IndexOfHash(names, property.ulongValue);

            DropdownField field = new DropdownField(property.displayName, names, Mathf.Max(0, chosen)) { tooltip = property.tooltip };

            if (chosen < 0)
            {
                field.SetValueWithoutNotify(string.Empty);
            }

            field.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            field.RegisterValueChangedCallback(e =>
            {
                property.ulongValue = names.IndexOf(e.newValue) == 0 ? 0UL : Fnv1a64.Hash(e.newValue);
                property.serializedObject.ApplyModifiedProperties();
            });

            return field;
        }
    }
}