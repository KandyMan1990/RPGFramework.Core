using System.Collections.Generic;
using RPGFramework.Core.Dialogue;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor.Dialogue
{
    /// <summary>
    /// Paste a line from a sheet and see it as a player will: typed out, with its styles, pauses and flashing, and
    /// with any markup mistakes listed. The same parser and renderer the game uses, so nothing here can disagree
    /// with play.
    /// </summary>
    public sealed class DialoguePreviewWindow : EditorWindow
    {
        [SerializeField] private string             m_Source    = "You found a {KeyItem}sword{/KeyItem}!{Wait 1} You have {Var 0} gold.";
        [SerializeField] private int[]              m_Variables = new int[DialogueMarkup.MESSAGE_VARIABLE_COUNT];
        [SerializeField] private float              m_Speed     = 40f;
        [SerializeField] private DialogueTextStyles m_Styles;

        private DialoguePreviewElement      m_Preview;
        private HelpBox                     m_Problems;
        private Label                       m_PageLabel;
        private IReadOnlyList<DialoguePage> m_Pages;
        private int                         m_Page;

        [MenuItem("RPG Framework/Dialogue/Dialogue Preview")]
        public static void Open()
        {
            GetWindow<DialoguePreviewWindow>("Dialogue Preview");
        }

        private void CreateGUI()
        {
            if (m_Variables == null || m_Variables.Length != DialogueMarkup.MESSAGE_VARIABLE_COUNT)
            {
                m_Variables = new int[DialogueMarkup.MESSAGE_VARIABLE_COUNT];
            }

            if (m_Styles == null)
            {
                m_Styles = DialogueEditorUtility.FindProjectTextStyles();
            }

            VisualElement root = rootVisualElement;
            root.style.paddingLeft   = 8;
            root.style.paddingRight  = 8;
            root.style.paddingTop    = 8;
            root.style.paddingBottom = 8;

            root.Add(new HelpBox("Paste a line from a sheet to see it as players will. " +
                                 "{Name}…{/Name} uses a style, {Colour #2CE2DD}…{/Colour} and {Blink}…{/Blink} style it directly, " +
                                 "{Wait 1} pauses the typing, {Var 0} shows a message variable, {NewPage} starts a page and [Name] at the start names the speaker.",
                                 HelpBoxMessageType.Info));

            ObjectField styles = new ObjectField("Styles") { objectType = typeof(DialogueTextStyles), value = m_Styles };
            styles.RegisterValueChangedCallback(e =>
                                                {
                                                    m_Styles = (DialogueTextStyles)e.newValue;
                                                    Refresh(true);
                                                });
            root.Add(styles);

            TextField source = new TextField("Text") { multiline = true, value = m_Source };
            source.style.minHeight = 60;
            source.style.whiteSpace = WhiteSpace.Normal;
            source.RegisterValueChangedCallback(e =>
                                                {
                                                    m_Source = e.newValue;
                                                    Refresh(true);
                                                });
            root.Add(source);

            Foldout variables = new Foldout { text = "Message variables", value = false };

            for (int i = 0; i < m_Variables.Length; i++)
            {
                int          slot  = i;
                IntegerField field = new IntegerField($"{{Var {slot}}}") { value = m_Variables[slot] };
                field.RegisterValueChangedCallback(e =>
                                                   {
                                                       m_Variables[slot] = e.newValue;
                                                       Refresh(false);
                                                   });
                variables.Add(field);
            }

            root.Add(variables);

            FloatField speed = new FloatField("Characters per second") { value = m_Speed };
            speed.RegisterValueChangedCallback(e =>
                                               {
                                                   m_Speed                       = Mathf.Max(1f, e.newValue);
                                                   m_Preview.CharactersPerSecond = m_Speed;
                                               });
            root.Add(speed);

            m_Problems = new HelpBox(string.Empty, HelpBoxMessageType.Error) { style = { marginTop = 6 } };
            root.Add(m_Problems);

            m_Preview = new DialoguePreviewElement { CharactersPerSecond = m_Speed, style = { marginTop = 6 } };
            root.Add(m_Preview);

            VisualElement controls = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4 } };

            m_PageLabel = new Label { style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };

            controls.Add(new Button(() => ShowPage(m_Page - 1)) { text = "◀" });
            controls.Add(m_PageLabel);
            controls.Add(new Button(() => ShowPage(m_Page + 1)) { text = "▶" });
            controls.Add(new Button(() => ShowPage(m_Page)) { text = "Replay" });
            controls.Add(new Button(() => m_Preview.Skip()) { text = "Skip" });

            root.Add(controls);

            Refresh(true);
        }

        /// <param name="fromStart">Go back to the first page, as when the text itself changed.</param>
        private void Refresh(bool fromStart)
        {
            List<string> problems = DialogueMarkup.Validate(m_Source ?? string.Empty, m_Styles);

            m_Problems.text          = string.Join("\n", problems);
            m_Problems.style.display = problems.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            m_Pages = DialogueMarkup.ToPages(m_Source ?? string.Empty);

            ShowPage(fromStart ? 0 : m_Page);
        }

        private void ShowPage(int page)
        {
            m_Page = Mathf.Clamp(page, 0, m_Pages.Count - 1);

            m_PageLabel.text = $"  Page {m_Page + 1} of {m_Pages.Count}";

            m_Preview.Show(m_Pages[m_Page], m_Variables, m_Styles, true);
        }
    }
}
