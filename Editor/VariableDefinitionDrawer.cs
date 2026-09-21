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
    /// stored as.
    /// </summary>
    [CustomPropertyDrawer(typeof(VariableDefinition))]
    public sealed class VariableDefinitionDrawer : PropertyDrawer
    {
        public const string DEFAULT_VALUE = "m_DefaultValue";

        private const string NAME        = "m_Name";
        private const string BANK        = "m_Bank";
        private const string WIDTH       = "m_Width";
        private const string OFFSET      = "m_Offset";
        private const string DESCRIPTION = "m_Description";
        private const string LABEL       = "Default";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty name  = property.FindPropertyRelative(NAME);
            SerializedProperty bank  = property.FindPropertyRelative(BANK);
            SerializedProperty width = property.FindPropertyRelative(WIDTH);

            Foldout foldout = new Foldout { text = name.stringValue, value = false };
            foldout.TrackPropertyValue(name, changed => foldout.text = changed.stringValue);

            PropertyField offset = new PropertyField(property.FindPropertyRelative(OFFSET));
            offset.SetEnabled(false);

            VisualElement defaultSlot = new VisualElement();

            foldout.Add(new PropertyField(name));
            foldout.Add(new PropertyField(bank));
            foldout.Add(new PropertyField(width));
            foldout.Add(offset);
            foldout.Add(new PropertyField(property.FindPropertyRelative(DESCRIPTION)));
            foldout.Add(defaultSlot);

            BuildDefault(defaultSlot, property);

            defaultSlot.TrackPropertyValue(width, _ => BuildDefault(defaultSlot, property));
            defaultSlot.TrackPropertyValue(bank,  _ => BuildDefault(defaultSlot, property));

            return foldout;
        }

        private static void BuildDefault(VisualElement slot, SerializedProperty variable)
        {
            slot.Clear();

            string             name         = variable.FindPropertyRelative(NAME).stringValue;
            MemoryBank         bank         = (MemoryBank)variable.FindPropertyRelative(BANK).enumValueFlag;
            VariableWidth      width        = (VariableWidth)variable.FindPropertyRelative(WIDTH).enumValueFlag;
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

            slot.Add(CreateTyped(defaultValue, width));
        }

        private static VisualElement CreateTyped(SerializedProperty defaultValue, VariableWidth width)
        {
            ulong bits = defaultValue.ulongValue;

            switch (width)
            {
                case VariableWidth.Bool:
                {
                    Toggle toggle = new Toggle(LABEL) { value = VariableDefaults.ToBool(bits) };
                    toggle.RegisterValueChangedCallback(e => Write(defaultValue, VariableDefaults.FromBool(e.newValue)));

                    return toggle;
                }

                case VariableWidth.Float:
                {
                    FloatField floatField = new FloatField(LABEL) { value = VariableDefaults.ToFloat(bits) };
                    floatField.RegisterValueChangedCallback(e => Write(defaultValue, VariableDefaults.FromFloat(e.newValue)));

                    return floatField;
                }

                case VariableWidth.ULong:
                {
                    UnsignedLongField ulongField = new UnsignedLongField(LABEL) { value = bits };
                    ulongField.RegisterValueChangedCallback(e => Write(defaultValue, e.newValue));

                    return ulongField;
                }

                default:
                {
                    (long min, long max) = GetRange(width);

                    LongField longField = new LongField(LABEL) { value = VariableDefaults.ToInteger(bits, width) };
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
