using System;
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
    /// stored as. An array's default is for every element, and particular elements can start at their own. A
    /// record shows its fields instead, each with the default every record starts with, and particular records' own.
    /// </summary>
    [CustomPropertyDrawer(typeof(VariableDefinition))]
    public sealed class VariableDefinitionDrawer : PropertyDrawer
    {
        public const string DEFAULT_VALUE = "m_DefaultValue";

        private const string NAME             = "m_Name";
        private const string BANK             = "m_Bank";
        private const string WIDTH            = "m_Width";
        private const string COUNT            = "m_Count";
        private const string FIELDS           = "m_Fields";
        private const string OFFSET           = "m_Offset";
        private const string DESCRIPTION      = "m_Description";
        private const string ELEMENT_DEFAULTS = "m_ElementDefaults";
        private const string ELEMENT_INDEX    = "m_Index";
        private const string ELEMENT_FIELD    = "m_Field";
        private const string FIELD_INDEX      = "m_FieldIndex";
        private const string ELEMENT_VALUE    = "m_Value";
        private const string LABEL            = "Default";
        private const string ARRAY_LABEL      = "Default (every element)";
        private const string RECORDS_LABEL    = "Default (every record)";
        private const float  FIELD_WIDTH      = 90;
        private const float  FIELD_COUNT      = 50;

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

            PropertyField widthField  = new PropertyField(width);
            VisualElement defaultSlot = new VisualElement();

            foldout.Add(new PropertyField(name));
            foldout.Add(new PropertyField(bank));
            foldout.Add(widthField);
            foldout.Add(new PropertyField(count));
            foldout.Add(offset);
            foldout.Add(new PropertyField(property.FindPropertyRelative(DESCRIPTION)));
            foldout.Add(defaultSlot);

            BuildDefault(defaultSlot, property, widthField);

            defaultSlot.TrackPropertyValue(width, _ => BuildDefault(defaultSlot, property, widthField));
            defaultSlot.TrackPropertyValue(bank,  _ => BuildDefault(defaultSlot, property, widthField));
            defaultSlot.TrackPropertyValue(count, _ => BuildDefault(defaultSlot, property, widthField));

            return foldout;
        }

        private static void BuildDefault(VisualElement slot, SerializedProperty variable, VisualElement widthField)
        {
            slot.Clear();

            string             name         = variable.FindPropertyRelative(NAME).stringValue;
            MemoryBank         bank         = (MemoryBank)variable.FindPropertyRelative(BANK).enumValueFlag;
            VariableWidth      width        = (VariableWidth)variable.FindPropertyRelative(WIDTH).enumValueFlag;
            int                count        = variable.FindPropertyRelative(COUNT).intValue;
            bool               isRecord     = variable.FindPropertyRelative(FIELDS).arraySize > 0;
            SerializedProperty defaultValue = variable.FindPropertyRelative(DEFAULT_VALUE);

            Action rebuild = () => BuildDefault(slot, variable, widthField);

            widthField.style.display = isRecord ? DisplayStyle.None : DisplayStyle.Flex;

            if (isRecord)
            {
                slot.Add(BuildFields(variable, count, bank != MemoryBank.Temp, rebuild));
            }

            if (bank == MemoryBank.Temp)
            {
                slot.Add(new HelpBox("Temp variables have no default: each script's temp memory starts at zero.", HelpBoxMessageType.None));
                return;
            }

            if (isRecord)
            {
                // A single record's values are its fields' defaults, so only an array of them has records to set apart.
                if (count > 1)
                {
                    slot.Add(BuildRecordDefaults(variable, count, rebuild));
                }

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
            slot.Add(BuildElementDefaults(variable, width, count, rebuild));
        }

        private static VisualElement BuildElementDefaults(SerializedProperty variable, VariableWidth width, int count, Action rebuild)
        {
            SerializedProperty elementDefaults = variable.FindPropertyRelative(ELEMENT_DEFAULTS);

            VisualElement list = new VisualElement();

            for (int i = 0; i < elementDefaults.arraySize; i++)
            {
                int                position = i;
                SerializedProperty entry    = elementDefaults.GetArrayElementAtIndex(i);

                VisualElement row = MakeRow();

                IntegerField indexField = CreateIndexField("Element", entry.FindPropertyRelative(ELEMENT_INDEX), count);
                indexField.style.width = Length.Percent(50);

                VisualElement value = CreateTyped(entry.FindPropertyRelative(ELEMENT_VALUE), width, string.Empty);
                value.style.flexGrow = 1;

                row.Add(indexField);
                row.Add(value);
                row.Add(CreateRemoveButton(elementDefaults, position, "Start this element at the default for every element", rebuild));

                list.Add(row);
            }

            Button add = new Button(() => AddElementDefault(variable, count, rebuild)) { text = "Add Element Default" };
            add.SetEnabled(FirstElementWithoutDefault(elementDefaults, count) < count);

            list.Add(add);

            return list;
        }

        private static void AddElementDefault(SerializedProperty variable, int count, Action rebuild)
        {
            SerializedProperty elementDefaults = variable.FindPropertyRelative(ELEMENT_DEFAULTS);

            int index = FirstElementWithoutDefault(elementDefaults, count);

            if (index >= count)
            {
                return;
            }

            AddElementDefault(elementDefaults, index, string.Empty, 0, variable.FindPropertyRelative(DEFAULT_VALUE).ulongValue);

            rebuild();
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

        /// <summary>
        /// A record's fields in order — name, width, count and, outside Temp, the default every record starts with.
        /// Changing them changes the record's size, which the map's validation checks against what follows it.
        /// </summary>
        private static VisualElement BuildFields(SerializedProperty variable, int count, bool hasDefaults, Action rebuild)
        {
            SerializedProperty fields = variable.FindPropertyRelative(FIELDS);

            VisualElement section = new VisualElement();
            section.style.marginTop = 4;

            VisualElement header = MakeRow();
            header.Add(new Label("Fields") { style = { flexGrow = 1 } });
            header.Add(new Label("Width") { style  = { width = FIELD_WIDTH } });
            header.Add(new Label("Count") { style  = { width = FIELD_COUNT + 24 } });

            section.Add(header);

            for (int i = 0; i < fields.arraySize; i++)
            {
                int                position   = i;
                SerializedProperty field      = fields.GetArrayElementAtIndex(i);
                SerializedProperty fieldName  = field.FindPropertyRelative(NAME);
                SerializedProperty fieldWidth = field.FindPropertyRelative(WIDTH);
                SerializedProperty fieldCount = field.FindPropertyRelative(COUNT);

                VisualElement box = MakeBox();
                VisualElement row = MakeRow();

                TextField nameField = new TextField { value = fieldName.stringValue, isDelayed = true, tooltip = "The name scripts use after the record, as in $characters[0].hp" };
                nameField.style.flexGrow = 1;
                nameField.RegisterValueChangedCallback(e =>
                                                       {
                                                           RenameField(variable, fieldName, e.newValue);
                                                           rebuild();
                                                       });

                EnumField widthField = new EnumField((VariableWidth)fieldWidth.enumValueFlag) { tooltip = "Storage width of each value" };
                widthField.style.width = FIELD_WIDTH;
                widthField.RegisterValueChangedCallback(e =>
                                                        {
                                                            fieldWidth.enumValueFlag = (int)(VariableWidth)e.newValue;
                                                            fieldWidth.serializedObject.ApplyModifiedProperties();
                                                            rebuild();
                                                        });

                IntegerField countField = new IntegerField { value = fieldCount.intValue, isDelayed = true, tooltip = "More than 1 makes an array inside the record" };
                countField.style.width = FIELD_COUNT;
                countField.RegisterValueChangedCallback(e =>
                                                        {
                                                            fieldCount.intValue = e.newValue < 1 ? 1 : e.newValue;
                                                            RemoveElementDefaultsPast(variable, fieldName.stringValue, fieldCount.intValue);
                                                            rebuild();
                                                        });

                Button remove = new Button(() => RemoveField(variable, position, fieldName.stringValue, rebuild))
                                {
                                    text    = "✕",
                                    tooltip = "Remove this field, and every record's own default for it"
                                };

                remove.SetEnabled(fields.arraySize > 1);

                row.Add(nameField);
                row.Add(widthField);
                row.Add(countField);
                row.Add(remove);

                box.Add(row);

                if (hasDefaults)
                {
                    string label = count > 1 ? RECORDS_LABEL : fieldCount.intValue > 1 ? ARRAY_LABEL : LABEL;

                    box.Add(CreateTyped(field.FindPropertyRelative(DEFAULT_VALUE), (VariableWidth)fieldWidth.enumValueFlag, label));
                }

                section.Add(box);
            }

            section.Add(new Button(() => AddField(fields, rebuild)) { text = "Add Field" });

            return section;
        }

        private static void AddField(SerializedProperty fields, Action rebuild)
        {
            HashSet<string> names = new HashSet<string>();

            for (int i = 0; i < fields.arraySize; i++)
            {
                names.Add(fields.GetArrayElementAtIndex(i).FindPropertyRelative(NAME).stringValue);
            }

            int suffix = fields.arraySize;

            while (names.Contains($"field{suffix}"))
            {
                suffix++;
            }

            fields.arraySize++;

            SerializedProperty added = fields.GetArrayElementAtIndex(fields.arraySize - 1);
            added.FindPropertyRelative(NAME).stringValue         = $"field{suffix}";
            added.FindPropertyRelative(WIDTH).enumValueFlag      = (int)VariableWidth.Byte;
            added.FindPropertyRelative(COUNT).intValue           = 1;
            added.FindPropertyRelative(DEFAULT_VALUE).ulongValue = 0;

            fields.serializedObject.ApplyModifiedProperties();

            rebuild();
        }

        /// <summary>
        /// Records' own defaults name their field, so they follow it to its new name.
        /// </summary>
        private static void RenameField(SerializedProperty variable, SerializedProperty fieldName, string newName)
        {
            SerializedProperty elementDefaults = variable.FindPropertyRelative(ELEMENT_DEFAULTS);

            for (int i = 0; i < elementDefaults.arraySize; i++)
            {
                SerializedProperty field = elementDefaults.GetArrayElementAtIndex(i).FindPropertyRelative(ELEMENT_FIELD);

                if (field.stringValue == fieldName.stringValue)
                {
                    field.stringValue = newName;
                }
            }

            fieldName.stringValue = newName;
            fieldName.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// A field made shorter takes records' own values for its lost elements with it.
        /// </summary>
        private static void RemoveElementDefaultsPast(SerializedProperty variable, string field, int fieldCount)
        {
            SerializedProperty elementDefaults = variable.FindPropertyRelative(ELEMENT_DEFAULTS);

            for (int i = elementDefaults.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty entry = elementDefaults.GetArrayElementAtIndex(i);

                if (entry.FindPropertyRelative(ELEMENT_FIELD).stringValue == field && entry.FindPropertyRelative(FIELD_INDEX).intValue >= fieldCount)
                {
                    elementDefaults.DeleteArrayElementAtIndex(i);
                }
            }

            variable.serializedObject.ApplyModifiedProperties();
        }

        private static void RemoveField(SerializedProperty variable, int position, string name, Action rebuild)
        {
            SerializedProperty elementDefaults = variable.FindPropertyRelative(ELEMENT_DEFAULTS);

            for (int i = elementDefaults.arraySize - 1; i >= 0; i--)
            {
                if (elementDefaults.GetArrayElementAtIndex(i).FindPropertyRelative(ELEMENT_FIELD).stringValue == name)
                {
                    elementDefaults.DeleteArrayElementAtIndex(i);
                }
            }

            variable.FindPropertyRelative(FIELDS).DeleteArrayElementAtIndex(position);
            variable.serializedObject.ApplyModifiedProperties();

            rebuild();
        }

        /// <summary>
        /// Records that start at values of their own rather than the fields' defaults, one box per record listing every
        /// field. A record added here copies the defaults as they are then; a field added later starts at its default
        /// until it is given a value.
        /// </summary>
        private static VisualElement BuildRecordDefaults(SerializedProperty variable, int count, Action rebuild)
        {
            SerializedProperty fields          = variable.FindPropertyRelative(FIELDS);
            SerializedProperty elementDefaults = variable.FindPropertyRelative(ELEMENT_DEFAULTS);

            SortedSet<int> records = new SortedSet<int>();

            for (int i = 0; i < elementDefaults.arraySize; i++)
            {
                records.Add(elementDefaults.GetArrayElementAtIndex(i).FindPropertyRelative(ELEMENT_INDEX).intValue);
            }

            VisualElement section = new VisualElement();
            section.style.marginTop = 4;

            if (records.Count > 0)
            {
                section.Add(new Label("Records that start at their own values"));
            }

            foreach (int record in records)
            {
                section.Add(BuildRecord(fields, elementDefaults, record, count, records, rebuild));
            }

            int next = 0;

            while (next < count && records.Contains(next))
            {
                next++;
            }

            Button add = new Button(() => AddRecord(fields, elementDefaults, next, rebuild))
                         {
                             text    = "Add Record",
                             tooltip = "Give one record values of its own — a character's starting HP, say. It starts as a copy of the fields' defaults"
                         };

            add.SetEnabled(next < count);

            section.Add(add);

            return section;
        }

        private static VisualElement BuildRecord(SerializedProperty fields, SerializedProperty elementDefaults, int record, int count, SortedSet<int> records, Action rebuild)
        {
            VisualElement box    = MakeBox();
            VisualElement header = MakeRow();

            IntegerField recordField = new IntegerField("Record") { value = record, isDelayed = true, tooltip = "Which record starts at these values" };
            recordField.style.flexGrow = 1;
            recordField.RegisterValueChangedCallback(e =>
                                                     {
                                                         if (e.newValue < 0 || e.newValue >= count || records.Contains(e.newValue))
                                                         {
                                                             recordField.SetValueWithoutNotify(record);
                                                             return;
                                                         }

                                                         MoveRecord(elementDefaults, record, e.newValue);
                                                         rebuild();
                                                     });

            Button remove = new Button(() =>
                                       {
                                           RemoveRecord(elementDefaults, record);
                                           rebuild();
                                       })
                            {
                                text    = "✕",
                                tooltip = "Start this record at the fields' defaults"
                            };

            header.Add(recordField);
            header.Add(remove);

            box.Add(header);

            for (int i = 0; i < fields.arraySize; i++)
            {
                SerializedProperty field      = fields.GetArrayElementAtIndex(i);
                string             fieldName  = field.FindPropertyRelative(NAME).stringValue;
                VariableWidth      fieldWidth = (VariableWidth)field.FindPropertyRelative(WIDTH).enumValueFlag;
                int                fieldCount = field.FindPropertyRelative(COUNT).intValue;
                ulong              fallback   = field.FindPropertyRelative(DEFAULT_VALUE).ulongValue;

                for (int fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
                {
                    int                element = fieldIndex;
                    string             label   = fieldCount > 1 ? $"{fieldName}[{fieldIndex}]" : fieldName;
                    SerializedProperty entry   = FindElementDefault(elementDefaults, record, fieldName, fieldIndex);

                    VisualElement value = entry != null
                                              ? CreateTyped(entry.FindPropertyRelative(ELEMENT_VALUE), fieldWidth, label)
                                              : CreateTyped(fallback, fieldWidth, label, bits => SetElementDefault(elementDefaults, record, fieldName, element, bits));

                    box.Add(value);
                }
            }

            return box;
        }

        private static void AddRecord(SerializedProperty fields, SerializedProperty elementDefaults, int record, Action rebuild)
        {
            for (int i = 0; i < fields.arraySize; i++)
            {
                SerializedProperty field = fields.GetArrayElementAtIndex(i);

                string name         = field.FindPropertyRelative(NAME).stringValue;
                int    fieldCount   = field.FindPropertyRelative(COUNT).intValue;
                ulong  defaultValue = field.FindPropertyRelative(DEFAULT_VALUE).ulongValue;

                for (int fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
                {
                    AddElementDefault(elementDefaults, record, name, fieldIndex, defaultValue);
                }
            }

            rebuild();
        }

        private static void MoveRecord(SerializedProperty elementDefaults, int from, int to)
        {
            for (int i = 0; i < elementDefaults.arraySize; i++)
            {
                SerializedProperty index = elementDefaults.GetArrayElementAtIndex(i).FindPropertyRelative(ELEMENT_INDEX);

                if (index.intValue == from)
                {
                    index.intValue = to;
                }
            }

            elementDefaults.serializedObject.ApplyModifiedProperties();
        }

        private static void RemoveRecord(SerializedProperty elementDefaults, int record)
        {
            for (int i = elementDefaults.arraySize - 1; i >= 0; i--)
            {
                if (elementDefaults.GetArrayElementAtIndex(i).FindPropertyRelative(ELEMENT_INDEX).intValue == record)
                {
                    elementDefaults.DeleteArrayElementAtIndex(i);
                }
            }

            elementDefaults.serializedObject.ApplyModifiedProperties();
        }

        private static SerializedProperty FindElementDefault(SerializedProperty elementDefaults, int index, string field, int fieldIndex)
        {
            SerializedProperty found = null;

            for (int i = 0; i < elementDefaults.arraySize && found == null; i++)
            {
                SerializedProperty entry = elementDefaults.GetArrayElementAtIndex(i);

                if (entry.FindPropertyRelative(ELEMENT_INDEX).intValue    == index &&
                    entry.FindPropertyRelative(ELEMENT_FIELD).stringValue == field &&
                    entry.FindPropertyRelative(FIELD_INDEX).intValue      == fieldIndex)
                {
                    found = entry;
                }
            }

            return found;
        }

        private static void SetElementDefault(SerializedProperty elementDefaults, int index, string field, int fieldIndex, ulong value)
        {
            SerializedProperty entry = FindElementDefault(elementDefaults, index, field, fieldIndex);

            if (entry == null)
            {
                AddElementDefault(elementDefaults, index, field, fieldIndex, value);
                return;
            }

            Write(entry.FindPropertyRelative(ELEMENT_VALUE), value);
        }

        private static void AddElementDefault(SerializedProperty elementDefaults, int index, string field, int fieldIndex, ulong value)
        {
            elementDefaults.arraySize++;

            // A new array element copies the one before it, so every part is set.
            SerializedProperty added = elementDefaults.GetArrayElementAtIndex(elementDefaults.arraySize - 1);
            added.FindPropertyRelative(ELEMENT_INDEX).intValue    = index;
            added.FindPropertyRelative(ELEMENT_FIELD).stringValue = field;
            added.FindPropertyRelative(FIELD_INDEX).intValue      = fieldIndex;
            added.FindPropertyRelative(ELEMENT_VALUE).ulongValue  = value;

            elementDefaults.serializedObject.ApplyModifiedProperties();
        }

        private static IntegerField CreateIndexField(string label, SerializedProperty index, int count)
        {
            IntegerField indexField = new IntegerField(label) { value = index.intValue };
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

            return indexField;
        }

        private static Button CreateRemoveButton(SerializedProperty list, int position, string tooltip, Action rebuild)
        {
            Button remove = new Button(() =>
                                       {
                                           list.DeleteArrayElementAtIndex(position);
                                           list.serializedObject.ApplyModifiedProperties();

                                           rebuild();
                                       })
                            {
                                text    = "✕",
                                tooltip = tooltip
                            };

            return remove;
        }

        private static VisualElement MakeRow()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            return row;
        }

        private static VisualElement MakeBox()
        {
            VisualElement box = new VisualElement();
            box.style.marginTop       = 2;
            box.style.marginBottom    = 2;
            box.style.paddingLeft     = 4;
            box.style.borderLeftWidth = 2;
            box.style.borderLeftColor = new UnityEngine.Color(0.5f, 0.5f, 0.5f, 0.5f);

            return box;
        }

        private static VisualElement CreateTyped(SerializedProperty defaultValue, VariableWidth width, string label)
        {
            VisualElement typed = CreateTyped(defaultValue.ulongValue, width, label, bits => Write(defaultValue, bits));

            return typed;
        }

        private static VisualElement CreateTyped(ulong bits, VariableWidth width, string label, Action<ulong> write)
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
                    (long min, long max) = GetRange(width);

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
