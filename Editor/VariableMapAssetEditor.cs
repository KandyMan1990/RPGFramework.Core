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
    public sealed class VariableMapAssetEditor : UnityEditor.Editor
    {
        private VariableMapAsset m_Map;

        private Label     m_GlobalSizeLabel;
        private Label     m_SessionSizeLabel;
        private Label     m_NextOffsetLabel;
        private TextField m_NameField;
        private EnumField m_BankField;
        private EnumField m_WidthField;
        private TextField m_DescriptionField;
        private HelpBox   m_DeclarationProblem;
        private Button    m_AllocateButton;

        private VisualElement m_ValidationResults;

        public override VisualElement CreateInspectorGUI()
        {
            m_Map = (VariableMapAsset)target;

            VisualElement root = new VisualElement();

            root.Add(BuildBankSummary());
            root.Add(BuildDeclareNew());
            root.Add(BuildValidation());
            root.Add(BuildDeclaredVariables());

            // Keep the derived labels honest when the list is edited directly, or undone.
            root.TrackSerializedObjectValue(serializedObject, _ => RefreshDerivedLabels());

            RefreshDerivedLabels();

            return root;
        }

        private VisualElement BuildBankSummary()
        {
            VisualElement section = MakeSection("Bank sizes");

            m_GlobalSizeLabel  = new Label();
            m_SessionSizeLabel = new Label();

            section.Add(m_GlobalSizeLabel);
            section.Add(m_SessionSizeLabel);

            return section;
        }

        private VisualElement BuildDeclareNew()
        {
            VisualElement section = MakeSection("Declare a variable");

            m_NameField        = new TextField("Name");
            m_BankField        = new EnumField("Bank",  MemoryBank.Global);
            m_WidthField       = new EnumField("Width", VariableWidth.Byte);
            m_DescriptionField = new TextField("Description") { multiline = true };

            m_NextOffsetLabel                    = new Label();
            m_NextOffsetLabel.style.marginTop    = 4;
            m_NextOffsetLabel.style.marginBottom = 4;

            m_DeclarationProblem               = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            m_DeclarationProblem.style.display = DisplayStyle.None;

            m_AllocateButton = new Button(OnAllocateClicked) { text = "Allocate" };

            m_NameField.RegisterValueChangedCallback(_ => RefreshDerivedLabels());
            m_BankField.RegisterValueChangedCallback(_ => RefreshDerivedLabels());
            m_WidthField.RegisterValueChangedCallback(_ => RefreshDerivedLabels());

            section.Add(m_NameField);
            section.Add(m_BankField);
            section.Add(m_WidthField);
            section.Add(m_DescriptionField);
            section.Add(m_NextOffsetLabel);
            section.Add(m_DeclarationProblem);
            section.Add(m_AllocateButton);

            return section;
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

            m_GlobalSizeLabel.text  = $"Global (saved):  {m_Map.GetRequiredBytes(MemoryBank.Global)} bytes";
            m_SessionSizeLabel.text = $"Session (not saved):  {m_Map.GetRequiredBytes(MemoryBank.Session)} bytes";

            MemoryBank    bank  = (MemoryBank)m_BankField.value;
            VariableWidth width = (VariableWidth)m_WidthField.value;

            m_NextOffsetLabel.text = $"Will be allocated at offset {m_Map.GetNextOffset(bank, width)}";

            string name      = m_NameField.value;
            bool   hasName   = !string.IsNullOrWhiteSpace(name);
            bool   nameTaken = hasName && m_Map.TryGetVariable(name, out VariableDefinition _);

            string problem = null;

            if (nameTaken)
            {
                problem = $"'{name}' is already declared in this map.";
            }
            else if (bank == MemoryBank.Temp)
            {
                problem = "Temp is script scratch and is cleared on every field and module load. Declare a variable here only if scripts need a named scratch slot; anything that must survive belongs in Session or Global.";
            }

            SetProblem(problem, nameTaken ? HelpBoxMessageType.Warning : HelpBoxMessageType.Info);

            m_AllocateButton.SetEnabled(hasName && !nameTaken);
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

            m_Map.Allocate(m_NameField.value,
                           (MemoryBank)m_BankField.value,
                           (VariableWidth)m_WidthField.value,
                           m_DescriptionField.value);

            EditorUtility.SetDirty(m_Map);
            AssetDatabase.SaveAssets();

            serializedObject.Update();

            m_NameField.value        = string.Empty;
            m_DescriptionField.value = string.Empty;

            m_ValidationResults.Clear();

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