using System.Collections.Generic;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// Inspector for <see cref="VariableMapAsset" />.<br /><br />
    /// The point of this inspector is that <b>offsets are never typed</b>. A variable is declared by name,
    /// bank and width, and the map decides where it goes. Hand-assigned offsets are how a memory map
    /// ends up with variables silently sharing bytes.
    /// </summary>
    [CustomEditor(typeof(VariableMapAsset))]
    internal sealed class VariableMapAssetEditor : UnityEditor.Editor
    {
        private VariableMapAsset m_Map;

        private Label        m_PersistentSizeLabel;
        private Label        m_SessionSizeLabel;
        private Label        m_NextOffsetLabel;
        private TextField    m_NameField;
        private EnumField    m_BankField;
        private EnumField    m_WidthField;
        private IntegerField m_CountField;
        private TextField    m_DescriptionField;
        private HelpBox      m_DeclarationProblem;
        private Button       m_AllocateButton;
        private Toggle       m_RecordToggle;

        private VisualElement m_PendingFieldsContainer;

        private readonly List<PendingField> m_PendingFields = new List<PendingField>();

        private VisualElement m_ValidationResults;
        private HelpBox       m_MissingRequired;
        private Button        m_AddMissingButton;

        public override VisualElement CreateInspectorGUI()
        {
            m_Map = (VariableMapAsset)target;

            AssignMissingIds();

            VisualElement root = new VisualElement();

            root.Add(BuildBankSummary());
            root.Add(BuildRequired());
            root.Add(BuildDeclareNew());
            root.Add(BuildValidation());
            root.Add(BuildDeclaredVariables());

            // Keep the derived labels honest when the list is edited directly, or undone.
            root.TrackSerializedObjectValue(serializedObject, _ =>
                                                              {
                                                                  AssignMissingIds();
                                                                  RefreshDerivedLabels();
                                                              });

            RefreshDerivedLabels();

            return root;
        }

        /// <summary>
        /// A map from before ids, or a variable or field copied in the list, gets its ids as soon as it is seen. Not
        /// recorded for undo: undoing it would only bring the missing or shared id back.
        /// </summary>
        private void AssignMissingIds()
        {
            if (m_Map.AssignMissingIds() == 0)
            {
                return;
            }

            EditorUtility.SetDirty(m_Map);
            AssetDatabase.SaveAssetIfDirty(m_Map);

            serializedObject.Update();
        }

        private VisualElement BuildBankSummary()
        {
            VisualElement section = MakeSection("Bank sizes");

            m_PersistentSizeLabel = new Label();
            m_SessionSizeLabel    = new Label();

            section.Add(m_PersistentSizeLabel);
            section.Add(m_SessionSizeLabel);

            return section;
        }

        private VisualElement BuildRequired()
        {
            VisualElement section = MakeSection("Required by the framework");

            m_MissingRequired  = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            m_AddMissingButton = new Button(OnAddMissingClicked) { text = "Add Missing Required Variables" };

            section.Add(new HelpBox("A new game starts every variable at its default. The framework reads some variables itself — the module to start in, and where in it — so every map must declare them; export refuses a map that does not.", HelpBoxMessageType.Info));
            section.Add(m_MissingRequired);
            section.Add(m_AddMissingButton);

            return section;
        }

        private VisualElement BuildDeclareNew()
        {
            VisualElement section = MakeSection("Declare a variable");

            m_NameField        = new TextField("Name");
            m_BankField        = new EnumField("Bank",  MemoryBank.Persistent);
            m_RecordToggle     = new Toggle("Record") { tooltip = "A record is fields of their own widths packed together, as a struct is — a character, an item slot" };
            m_WidthField       = new EnumField("Width", VariableWidth.Byte);
            m_CountField       = new IntegerField("Count") { value = 1, tooltip = "1 is a single value or record; more makes an array. An array's count is fixed once saves exist, since growing it would move what follows" };
            m_DescriptionField = new TextField("Description") { multiline = true };

            m_PendingFieldsContainer               = new VisualElement();
            m_PendingFieldsContainer.style.display = DisplayStyle.None;

            m_NextOffsetLabel                    = new Label();
            m_NextOffsetLabel.style.marginTop    = 4;
            m_NextOffsetLabel.style.marginBottom = 4;

            m_DeclarationProblem               = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            m_DeclarationProblem.style.display = DisplayStyle.None;

            m_AllocateButton = new Button(OnAllocateClicked) { text = "Allocate" };

            m_NameField.RegisterValueChangedCallback(_ => RefreshDerivedLabels());
            m_BankField.RegisterValueChangedCallback(_ => RefreshDerivedLabels());
            m_WidthField.RegisterValueChangedCallback(_ => RefreshDerivedLabels());
            m_CountField.RegisterValueChangedCallback(_ => RefreshDerivedLabels());
            m_RecordToggle.RegisterValueChangedCallback(e => OnRecordToggled(e.newValue));

            section.Add(m_NameField);
            section.Add(m_BankField);
            section.Add(m_RecordToggle);
            section.Add(m_WidthField);
            section.Add(m_PendingFieldsContainer);
            section.Add(m_CountField);
            section.Add(m_DescriptionField);
            section.Add(m_NextOffsetLabel);
            section.Add(m_DeclarationProblem);
            section.Add(m_AllocateButton);

            return section;
        }

        private void OnRecordToggled(bool record)
        {
            if (record && m_PendingFields.Count == 0)
            {
                m_PendingFields.Add(new PendingField());
            }

            m_WidthField.style.display             = record ? DisplayStyle.None : DisplayStyle.Flex;
            m_PendingFieldsContainer.style.display = record ? DisplayStyle.Flex : DisplayStyle.None;

            RebuildPendingFields();
        }

        private void RebuildPendingFields()
        {
            m_PendingFieldsContainer.Clear();

            foreach (PendingField pending in m_PendingFields)
            {
                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;

                TextField    name  = new TextField { value  = pending.Name, tooltip  = "The field's name, as scripts write it after the record" };
                EnumField    width = new EnumField(pending.Width) { tooltip = "Storage width of each value" };
                IntegerField count = new IntegerField { value = pending.Count, tooltip = "More than 1 makes an array inside the record" };

                name.style.flexGrow = 1;
                width.style.width   = 90;
                count.style.width   = 50;

                name.RegisterValueChangedCallback(e =>
                                                  {
                                                      pending.Name = e.newValue;
                                                      RefreshDerivedLabels();
                                                  });
                width.RegisterValueChangedCallback(e =>
                                                   {
                                                       pending.Width = (VariableWidth)e.newValue;
                                                       RefreshDerivedLabels();
                                                   });
                count.RegisterValueChangedCallback(e =>
                                                   {
                                                       pending.Count = e.newValue;
                                                       RefreshDerivedLabels();
                                                   });

                Button remove = new Button(() =>
                                           {
                                               m_PendingFields.Remove(pending);
                                               RebuildPendingFields();
                                           })
                                {
                                    text = "✕"
                                };

                remove.SetEnabled(m_PendingFields.Count > 1);

                row.Add(name);
                row.Add(width);
                row.Add(count);
                row.Add(remove);

                m_PendingFieldsContainer.Add(row);
            }

            m_PendingFieldsContainer.Add(new Button(() =>
                                                    {
                                                        m_PendingFields.Add(new PendingField { Name = $"field{m_PendingFields.Count}" });
                                                        RebuildPendingFields();
                                                    })
                                         {
                                             text = "Add Field"
                                         });

            RefreshDerivedLabels();
        }

        private List<VariableRecordField> CreatePendingFields()
        {
            List<VariableRecordField> fields = new List<VariableRecordField>(m_PendingFields.Count);

            foreach (PendingField pending in m_PendingFields)
            {
                fields.Add(new VariableRecordField(pending.Name, pending.Width, pending.Count, 0));
            }

            return fields;
        }

        /// <summary>
        /// A field of the record being declared, before it is allocated.
        /// </summary>
        private sealed class PendingField
        {
            internal string        Name  = "field0";
            internal VariableWidth Width = VariableWidth.Byte;
            internal int           Count = 1;
        }

        private VisualElement BuildValidation()
        {
            VisualElement section = MakeSection("Validation");

            m_ValidationResults = new VisualElement();

            section.Add(new Button(OnValidateClicked) { text = "Validate" });
            section.Add(m_ValidationResults);

            return section;
        }

        private VisualElement BuildDeclaredVariables()
        {
            VisualElement section = MakeSection("Declared variables");

            section.Add(new HelpBox("Offsets are assigned by the map and are never reused, so deleting a variable leaves a permanent gap. That is deliberate: reassigning a freed offset would make a new variable read an old one's data out of every existing save file.", HelpBoxMessageType.Info));

            VisualElement defaultInspector = new VisualElement();
            InspectorElement.FillDefaultInspector(defaultInspector, serializedObject, this);

            section.Add(defaultInspector);

            return section;
        }

        private static VisualElement MakeSection(string title)
        {
            VisualElement section = new VisualElement();
            section.style.marginBottom = 12;

            Label heading = new Label(title);
            heading.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            heading.style.marginBottom            = 4;

            section.Add(heading);

            return section;
        }

        private void RefreshDerivedLabels()
        {
            if (m_Map == null)
            {
                return;
            }

            RefreshRequired();

            m_PersistentSizeLabel.text = $"Persistent (saved):  {m_Map.GetRequiredBytes(MemoryBank.Persistent)} bytes";
            m_SessionSizeLabel.text    = $"Session (not saved):  {m_Map.GetRequiredBytes(MemoryBank.Session)} bytes";

            MemoryBank    bank   = (MemoryBank)m_BankField.value;
            VariableWidth width  = (VariableWidth)m_WidthField.value;
            int           count  = m_CountField.value;
            bool          record = m_RecordToggle.value;

            List<VariableRecordField> fields = CreatePendingFields();

            int alignment   = record ? VariableMapAsset.GetAlignment(fields) : width.GetByteCount();
            int elementSize = record ? 0 : width.GetByteCount();

            if (record)
            {
                foreach (VariableRecordField field in fields)
                {
                    elementSize += field.ByteCount;
                }
            }

            m_NextOffsetLabel.text = $"Will be allocated at offset {m_Map.GetNextOffset(bank, alignment)}, taking {elementSize * count} bytes";

            string name      = m_NameField.value;
            bool   hasName   = !string.IsNullOrWhiteSpace(name);
            bool   nameTaken = hasName && m_Map.TryGetVariable(name, out VariableDefinition _);

            string fieldProblem = record ? FindPendingFieldProblem(fields) : null;
            bool   blocked      = nameTaken || count < 1 || (hasName && !VariableMapAsset.IsScriptName(name)) || fieldProblem != null;

            string problem = null;

            if (nameTaken)
            {
                problem = $"'{name}' is already declared in this map.";
            }
            else if (hasName && !VariableMapAsset.IsScriptName(name))
            {
                problem = "Scripts name variables in text, so use letters, digits and underscores, not starting with a digit.";
            }
            else if (count < 1)
            {
                problem = "A variable holds at least one value.";
            }
            else if (fieldProblem != null)
            {
                problem = fieldProblem;
            }
            else if (bank == MemoryBank.Temp)
            {
                problem = "Temp is script scratch: each running script has its own, zeroed when it starts. Declare a variable here only if scripts need a named scratch slot; anything that must outlast the script belongs in Session or Persistent.";
            }

            SetProblem(problem, blocked ? HelpBoxMessageType.Warning : HelpBoxMessageType.Info);

            m_AllocateButton.SetEnabled(hasName && !blocked);
        }

        private static string FindPendingFieldProblem(List<VariableRecordField> fields)
        {
            HashSet<string> names   = new HashSet<string>();
            string          problem = null;

            foreach (VariableRecordField field in fields)
            {
                if (!VariableMapAsset.IsScriptName(field.Name))
                {
                    problem = $"The field '{field.Name}' needs a name scripts can use: letters, digits and underscores, not starting with a digit.";
                }
                else if (!names.Add(field.Name))
                {
                    problem = $"Two fields are called '{field.Name}'.";
                }
                else if (field.Count < 1)
                {
                    problem = $"The field '{field.Name}' holds at least one value.";
                }

                if (problem != null)
                {
                    break;
                }
            }

            return problem;
        }

        private void SetProblem(string message, HelpBoxMessageType messageType)
        {
            if (string.IsNullOrEmpty(message))
            {
                m_DeclarationProblem.style.display = DisplayStyle.None;
                return;
            }

            m_DeclarationProblem.text          = message;
            m_DeclarationProblem.messageType   = messageType;
            m_DeclarationProblem.style.display = DisplayStyle.Flex;
        }

        private void OnAllocateClicked()
        {
            Undo.RecordObject(m_Map, "Allocate variable");

            if (m_RecordToggle.value)
            {
                m_Map.AllocateRecord(m_NameField.value,
                                     (MemoryBank)m_BankField.value,
                                     CreatePendingFields(),
                                     m_CountField.value,
                                     m_DescriptionField.value);
            }
            else
            {
                m_Map.Allocate(m_NameField.value,
                               (MemoryBank)m_BankField.value,
                               (VariableWidth)m_WidthField.value,
                               m_CountField.value,
                               m_DescriptionField.value,
                               0);
            }

            EditorUtility.SetDirty(m_Map);
            AssetDatabase.SaveAssets();

            serializedObject.Update();

            m_PendingFields.Clear();

            m_NameField.value        = string.Empty;
            m_CountField.value       = 1;
            m_DescriptionField.value = string.Empty;
            m_RecordToggle.value     = false;

            m_ValidationResults.Clear();

            RefreshDerivedLabels();
        }

        private void RefreshRequired()
        {
            List<string> missing = new List<string>();

            foreach (RequiredVariable required in RequiredVariables.FindAll())
            {
                if (!m_Map.TryGetVariable(required.Name, out VariableDefinition _))
                {
                    missing.Add(required.Name);
                }
            }

            m_MissingRequired.text          = $"Missing: {string.Join(", ", missing)}";
            m_MissingRequired.style.display = missing.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            m_AddMissingButton.SetEnabled(missing.Count > 0);
        }

        private void OnAddMissingClicked()
        {
            Undo.RecordObject(m_Map, "Add missing required variables");

            m_Map.AddMissingRequiredVariables();

            EditorUtility.SetDirty(m_Map);
            AssetDatabase.SaveAssets();

            serializedObject.Update();

            RefreshDerivedLabels();
        }

        private void OnValidateClicked()
        {
            m_ValidationResults.Clear();

            List<string> problems = m_Map.Validate();

            if (problems.Count == 0)
            {
                m_ValidationResults.Add(new HelpBox("No problems found.", HelpBoxMessageType.Info));
                return;
            }

            foreach (string problem in problems)
            {
                m_ValidationResults.Add(new HelpBox(problem, HelpBoxMessageType.Error));
            }
        }
    }
}