using System;
using RPGFramework.Core.Memory;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// A variable's value as its own type wherever a value is set outside a playthrough: a package's picker when it
    /// offers one for the variable, a module or a field by name, otherwise a toggle or a number held to its width's range,
    /// rather than the bytes it is stored as.
    /// </summary>
    public sealed class VariableValueField : VisualElement
    {
        /// <summary>A package's picker shows the value, labelled as the picker labels it.</summary>
        public bool IsPicker { get; }

        public VariableValueField(string label, VariableWidth width, ulong value, Action<ulong> write) : this(label, width, value, write, null, null)
        {
        }

        /// <param name="variableName">The variable whose picker to use, if a package offers one; null for a number.</param>
        /// <param name="context">The other values where this one is set, which a picker may read.</param>
        public VariableValueField(string label, VariableWidth width, ulong value, Action<ulong> write, string variableName, IVariableValueContext context)
        {
            if (variableName != null && VariableDefaultFields.TryGet(variableName, out IVariableDefaultField picker))
            {
                IsPicker = true;
                Add(picker.Create(value, write, context));
                return;
            }

            IsPicker = false;
            Add(CreateTyped(label, width, value, write));
        }

        private static VisualElement CreateTyped(string label, VariableWidth width, ulong bits, Action<ulong> write)
        {
            switch (width)
            {
                case VariableWidth.Bool:
                {
                    Toggle toggle = new Toggle(label) { value = VariableDefaults.ToBool(bits) };
                    toggle.RegisterValueChangedCallback(e => write(VariableDefaults.FromBool(e.newValue)));

                    return toggle;
                }

                case VariableWidth.Float:
                {
                    FloatField floatField = new FloatField(label) { value = VariableDefaults.ToFloat(bits) };
                    floatField.RegisterValueChangedCallback(e => write(VariableDefaults.FromFloat(e.newValue)));

                    return floatField;
                }

                case VariableWidth.ULong:
                {
                    UnsignedLongField ulongField = new UnsignedLongField(label) { value = bits };
                    ulongField.RegisterValueChangedCallback(e => write(e.newValue));

                    return ulongField;
                }

                default:
                {
                    long      min       = Minimum(width);
                    long      max       = Maximum(width);
                    LongField longField = new LongField(label) { value = VariableDefaults.ToInteger(bits, width) };

                    longField.RegisterValueChangedCallback(e =>
                                                           {
                                                               long clamped = e.newValue < min ? min : e.newValue > max ? max : e.newValue;

                                                               if (clamped != e.newValue)
                                                               {
                                                                   longField.SetValueWithoutNotify(clamped);
                                                               }

                                                               write(VariableDefaults.FromInteger(clamped, width));
                                                           });

                    return longField;
                }
            }
        }

        private static long Minimum(VariableWidth width)
        {
            return width switch
                   {
                       VariableWidth.SByte => sbyte.MinValue,
                       VariableWidth.Short => short.MinValue,
                       VariableWidth.Int   => int.MinValue,
                       VariableWidth.Long  => long.MinValue,
                       _                   => 0L
                   };
        }

        private static long Maximum(VariableWidth width)
        {
            return width switch
                   {
                       VariableWidth.SByte  => sbyte.MaxValue,
                       VariableWidth.Byte   => byte.MaxValue,
                       VariableWidth.Short  => short.MaxValue,
                       VariableWidth.UShort => ushort.MaxValue,
                       VariableWidth.Int    => int.MaxValue,
                       VariableWidth.UInt   => uint.MaxValue,
                       _                    => long.MaxValue
                   };
        }
    }
}
