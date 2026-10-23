using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Editor.Dialogue
{
    /// <summary>
    /// <see cref="IDialogueMarkupValidator" /> against the styles dialogue uses in play: those on the project's dialogue
    /// window UI provider, or failing that the first styles asset in the project.
    /// </summary>
    public sealed class DialogueMarkupValidator : IDialogueMarkupValidator
    {
        private readonly IDialogueTextStyles m_Styles;

        public DialogueMarkupValidator()
        {
            m_Styles = ProjectDialogueStyles.Find();
        }

        string[] IDialogueMarkupValidator.Validate(string source)
        {
            return DialogueMarkup.Validate(source, m_Styles);
        }
    }
}
