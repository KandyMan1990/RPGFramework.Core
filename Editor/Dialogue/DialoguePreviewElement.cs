using System.Collections.Generic;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor.Dialogue
{
    /// <summary>
    /// A page of dialogue drawn by the same <see cref="DialogueTextView" /> the game uses, so what the editor shows
    /// is what a player sees: the colours, the flashing and the pauses in the typing.
    /// </summary>
    public sealed class DialoguePreviewElement : VisualElement
    {
        private readonly DialogueTextView m_View;

        public float CharactersPerSecond { get; set; } = 40f;

        public DialoguePreviewElement()
        {
            style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);
            style.paddingLeft     = 10;
            style.paddingRight    = 10;
            style.paddingTop      = 8;
            style.paddingBottom   = 8;
            style.minHeight       = 48;

            Label label = new Label
                          {
                              style =
                              {
                                  color              = new Color(0.82f, 0.82f, 0.82f),
                                  fontSize           = 16,
                                  whiteSpace         = WhiteSpace.PreWrap,
                                  unityTextAlign     = TextAnchor.UpperLeft,
                                  unityTextGenerator = TextGeneratorType.Advanced
                              }
                          };

            Add(label);

            m_View = new DialogueTextView(label);

            schedule.Execute(timer => m_View.Tick(timer.deltaTime / 1000f, CharactersPerSecond)).Every(0);
        }

        /// <param name="typed">Type it out as the game does, rather than showing it all at once.</param>
        public void Show(DialoguePage page, IReadOnlyList<int> variables, IDialogueTextStyles styles, bool typed)
        {
            string speaker = string.IsNullOrWhiteSpace(page.SpeakerId) ? string.Empty : page.SpeakerId + "\n";

            m_View.Show(speaker, DialogueMarkup.Parse(page.Text, variables, styles, null));

            if (!typed)
            {
                m_View.SkipToEnd();
            }
        }

        public void Skip()
        {
            m_View.SkipToEnd();
        }
    }
}
