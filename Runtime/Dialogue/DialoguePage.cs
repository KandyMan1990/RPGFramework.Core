namespace RPGFramework.Core.Dialogue
{
    public sealed class DialoguePage
    {
        internal string SpeakerId { get; }
        internal string Text      { get; }

        internal DialoguePage(string speakerId, string text)
        {
            SpeakerId = speakerId;
            Text      = text;
        }
    }
}