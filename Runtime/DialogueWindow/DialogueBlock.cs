namespace RPGFramework.Core.DialogueWindow
{
    internal sealed class DialogueBlock
    {
        internal DialoguePage[] Pages { get; }

        internal DialogueBlock(int count)
        {
            Pages = new DialoguePage[count];
        }

        internal void AddPage(int index, DialoguePage page)
        {
            Pages[index] = page;
        }
    }
}