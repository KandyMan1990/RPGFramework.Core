using System.Collections.Generic;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// A variable in the map, with its default shown as the variable's own type — a toggle, a number in the
    /// width's range, or a list when a package knows what the value names — rather than as the bytes it is
    /// stored as. An array's default is for every element, and particular elements can start at their own.
    /// </summary>
    [CustomPropertyDrawer(typeof(VariableDefinition))]
    public sealed class VariableDefinitionDrawer : PropertyDrawer
    {
        public const string DEFAULT_VALUE = "m_DefaultValue";

        private const string NAME             = "m_Name";
        private const string BANK             = "m_Bank";
        private const string WIDTH            = "m_Width";
        private const string COUNT            = "m_Count";
        private const string OFFSET           = "m_Offset";
        private const string DESCRIPTION      = "m_Description";
        private const string ELEMENT_DEFAULTS = "m_ElementDefaults";
        private const string ELEMENT_INDEX    = "m_Index";
        private const string ELEMENT_VALUE    = "m_Value";
        private const string LABEL            = "Default";
        private const string ARRAY_LABEL      = "Default (every element)";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty name  = property.FindPropertyRelative(NAME);
            SerializedProperty bank  = property.FindPropertyRelative(BANK);
            SerializedProperty width = property.FindPropertyRelative(WIDTH);
            SerializedProperty count = property.FindPropertyRelative(COUNT);

            Foldout foldout = new Foldout { text = name.stringValue, value = false };
            foldout.TrackPropertyValue(name, changed => foldout.text = changed.stringValue);

            PropertyField offset = new PropertyField(property.FindPropertyRelative(OFFSET));
            offset.SetEnabled(false);

            VisualElement defaultSlot = new VisualElement();

            foldout.Add(new PropertyField(name));
            foldout.Add(new PropertyField(bank));
            foldout.Add(new PropertyField(width));
            foldout.Add(new PropertyField(count));
            foldout.Add(offset);
            foldout.Add(new PropertyField(property.FindPropertyRelative(DESCRIPTION)));
            foldout.Add(defaultSlot);

            BuildDefault(defaultSlot, property);

            defaultSlot.TrackPropertyValue(width, _ => BuildDefault(defaultSlot, property));
            defaultSlot.TrackPropertyValue(bank,  _ => BuildDefault(defaultSlot, property));
            defaultSlot.TrackPropertyValue(count, _ => BuildDefault(defaultSlot, property));

            return foldout;
        }

        private static void BuildDefault(VisualElement slot, SerializedProperty variable)
        {
            slot.Clear();

            string             name         = variable.FindPropertyRelative(NAME).stringValue;
            MemoryBank         bank         = (MemoryBank)variable.FindPropertyRelative(BANK).enumValueFlag;
            VariableWidth      width        = (VariableWidth)variable.FindPropertyRelative(WIDTH).enumValueFlag;
            int                count        = variable.FindPropertyRelative(COUNT).intValue;
            SerializedProperty defaultValue = variable.FindPropertyRelative(DEFAULT_VALUE);

            if (bank == MemoryBank.Temp)
            {
                slot.Add(new HelpBox("Temp variables have no default: each script's temp memory starts at zero.", HelpBoxMessageType.None));
                return;
            }

            if (VariableDefaultFields.TryGet(name, out IVariableDefaultField field))
            {
                slot.Add(field.Create(variable));
                return;
            }

            if (count <= 1)
            {
                slot.Add(CreateTyped(defaultValue, width, LABEL));
                return;
            }

            slot.Add(CreateTyped(defaultValue, width, ARRAY_LABEL));
            slot.Add(BuildElementDefaults(slot, variable, width, count));
        }

        private static VisualElement BuildElementDefaults(VisualElement slot, SerializedProperty variable, VariableWidth width, int count)
        {
            SerializedProperty elementDefaults = variable.FindPropertyRelative(ELEMENT_DEFAULTS);

            VisualElement list = new VisualElement();

            for (int i = 0; i < elementDefaults.arraySize; i++)
            {
                int                position = i;
                SerializedProperty entry    = elementDefaults.GetArrayElementAtIndex(i);
                SerializedProperty index    = entry.FindPropertyRelative(ELEMENT_INDEX);

                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;

                IntegerField indexField = new IntegerField("Element") { value = index.intValue };
                indexField.style.width = Length.Percent(50);
                indexField.RegisterValueChangedCallback(e =>
                                                        {
                                                            int clamped = e.newValue < 0 ? 0 : e.newValue >= count ? count - 1 : e.newValue;

                                                            if (clamped != e.newValue)
                                                            {
                                                                indexField.SetValueWithoutNotify(clamped);
                                                            }

                                                            index.intValue = clamped;
                                                            index.serializedObject.ApplyModifiedProperties();
                                                        });

                VisualElement value = CreateTyped(entry.FindPropertyRelative(ELEMENT_VALUE), width, string.Empty);
                value.style.flexGrow = 1;

                Button remove = new Button(() =>
                                           {
                                               elementDefaults.DeleteArrayElementAtIndex(position);
                                               elementDefaults.serializedObject.ApplyModifiedProperties();

                                               BuildDefault(slot, variable);
                                           })
                                {
                                    text    = "✕",
                                    tooltip = "Start this element at the default for every element"
                                };

                row.Add(indexField);
                row.Add(value);
                row.Add(remove);

                list.Add(row);
            }

            Button add = new Button(() => AddElementDefault(slot, variable, count)) { text = "Add Element Default" };
            add.SetEnabled(FirstElementWithoutDefault(elementDefaults, count) < count);

            list.Add(add);

            return list;
        }

        private static void AddElementDefault(VisualElement slot, SerializedProperty variable, int count)
        {
            SerializedProperty elementDefaults = variable.FindPropertyRelative(ELEMENT_DEFAULTS);

            int index = FirstElementWithoutDefault(elementDefaults, count);

            if (index >= count)
            {
                return;
            }

            elementDefaults.arraySize++;

            SerializedProperty added = elementDefaults.GetArrayElementAtIndex(elementDefaults.arraySize - 1);
            added.FindPropertyRelative(ELEMENT_INDEX).intValue   = index;
            added.FindPropertyRelative(ELEMENT_VALUE).ulongValue = variable.FindPropertyRelative(DEFAULT_VALUE).ulongValue;

            elementDefaults.serializedObject.ApplyModifiedProperties();

            BuildDefault(slot, variable);
        }

        private static int FirstElementWithoutDefault(SerializedProperty elementDefaults, int count)
        {
            HashSet<int> used = new HashSet<int>();

            for (int i = 0; i < elementDefaults.arraySize; i++)
            {
                used.Add(elementDefaults.GetArrayElementAtIndex(i).FindPropertyRelative(ELEMENT_INDEX).intValue);
            }

            int index = 0;

            while (index < count && used.Contains(index))
            {
                index++;
            }

            return index;
        }

        private static VisualElement CreateTyped(SerializedProperty defaultValue, VariableWidth width, string label)
        {
            ulong bits = defaultValue.ulongValue;

            switch (width)
            {
                case VariableWidth.Bool:
                {
                    Toggle toggle = new Toggle(label) { value = VariableDefaults.ToBool(bits) };
                    toggle.RegisterValueChangedCallback(e => Write(defaultValue, VariableDefaults.FromBool(e.newValue)));

                    return toggle;
                }

                case VariableWidth.Float:
                {
                    FloatField floatField = new FloatField(label) { value = VariableDefaults.ToFloat(bits) };
                    floatField.RegisterValueChangedCallback(e => Write(defaultValue, VariableDefaults.FromFloat(e.newValue)));

                    return floatField;
                }

                case VariableWidth.ULong:
                {
                    UnsignedLongField ulongField = new UnsignedLongField(label) { value = bits };
                    ulongField.RegisterValueChangedCallback(e => Write(defaultValue, e.newValue));

                    return ulongField;
                }

                default:
                {
                    (long min, long max) = GetRange(width);

                    LongField longField = new LongField(label) { value = VariableDefaults.ToInteger(bits, width) };
                    longField.RegisterValueChangedCallback(e =>
                                                           {
                                                               long clamped = e.newValue < min ? min : e.newValue > max ? max : e.newValue;

                                                               if (clamped != e.newValue)
                                                               {
                                                                   longField.SetValueWithoutNotify(clamped);
                                                               }

                                                               Write(defaultValue, VariableDefaults.FromInteger(clamped, width));
                                                           });

                    return longField;
                }
            }
        }

        private static (long min, long max) GetRange(VariableWidth width)
        {
            (long min, long max) range = width switch
                                         {
                                             VariableWidth.SByte  => (sbyte.MinValue, sbyte.MaxValue),
                                             VariableWidth.Byte   => (byte.MinValue, byte.MaxValue),
                                             VariableWidth.Short  => (short.MinValue, short.MaxValue),
                                             VariableWidth.UShort => (ushort.MinValue, ushort.MaxValue),
                                             VariableWidth.Int    => (int.MinValue, int.MaxValue),
                                             VariableWidth.UInt   => (uint.MinValue, uint.MaxValue),
                                             _                    => (long.MinValue, long.MaxValue)
                                         };

            return range;
        }

        public static void Write(SerializedProperty defaultValue, ulong bits)
        {
            defaultValue.ulongValue = bits;
            defaultValue.serializedObject.ApplyModifiedProperties();
        }
    }
}
