using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue.UI
{
    /// <summary>
    /// The dialogue window's layout, the speeds it types and opens at, and the text styles, set by the game.
    /// </summary>
    public interface IDialogueWindowUIProvider
    {
        /// <summary>The layout a window of this kind is built from.</summary>
        VisualTreeAsset Get<T>() where T : IDialogueWindow;

        /// <summary>How many characters a second the text types at the authored speed.</summary>
        float TextSpeed { get; }

        /// <summary>How fast the window opens and closes: the reciprocal of the seconds it takes.</summary>
        float WindowSpeed { get; }

        /// <summary>The named styles dialogue markup can use, such as <c>{Location}</c>.</summary>
        DialogueTextStyles TextStyles { get; }
    }
}