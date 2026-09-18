using RPGFramework.Core.Dialogue;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor.Dialogue
{
    /// <summary>
    /// The styles, with each one drawn as dialogue will draw it, and the markup a writer types to use it.
    /// </summary>
    [CustomEditor(typeof(DialogueTextStyles))]
    internal sealed class DialogueTextStylesEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            Label heading = new Label("How each style looks")
                            {
                                style =
                                {
                                    unityFontStyleAndWeight = FontStyle.Bold,
                                    marginTop               = 12,
                                    marginBottom            = 4
                                }
                            };

            VisualElement previews = new VisualElement();

            root.Add(heading);
            root.Add(previews);

            BuildPreviews(previews);

            root.TrackSerializedObjectValue(serializedObject, _ => BuildPreviews(previews));

            return root;
        }

        private void BuildPreviews(VisualElement previews)
        {
            previews.Clear();

            DialogueTextStyles styles = (DialogueTextStyles)target;

            foreach (DialogueTextStyle style in styles.Styles)
            {
                if (string.IsNullOrWhiteSpace(style.Name))
                {
                    continue;
                }

                // "{{" is a literal brace, so the second half shows the markup itself.
                string sample = "{" + style.Name + "}" + style.Name + "{/" + style.Name + "}   written as {{" + style.Name + "}…{{/" + style.Name + "}";

                DialoguePreviewElement preview = new DialoguePreviewElement { style = { marginBottom = 2 } };
                preview.Show(new DialoguePage(string.Empty, sample), null, styles, false);

                previews.Add(preview);
            }

            if (previews.childCount == 0)
            {
                previews.Add(new HelpBox("Add a style above, then write {Name}text{/Name} in dialogue to use it.", HelpBoxMessageType.Info));
            }
        }
    }
}
