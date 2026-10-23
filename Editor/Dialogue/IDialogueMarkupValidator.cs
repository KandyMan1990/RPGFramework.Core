namespace RPGFramework.Core.Editor.Dialogue
{
    /// <summary>A line's dialogue markup checked, as a writer's text arrives and while they can still fix it.</summary>
    public interface IDialogueMarkupValidator
    {
        /// <summary>What is wrong with the markup of every page of <paramref name="source" />; none when it reads.</summary>
        string[] Validate(string source);
    }
}
