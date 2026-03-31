namespace RPGFramework.Core.DialogueWindow
{
    public sealed class DialoguePage
    {
        public string SpeakerId { get; }
        public string Text      { get; }

        public DialoguePage(string speakerId, string text)
        {
            SpeakerId = speakerId;
            Text      = text;
        }
    }
}