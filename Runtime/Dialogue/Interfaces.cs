using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue
{
    /// <summary>
    /// How a window plays its text through: page by page with a press to go on, ending on a choice, or typed and left
    /// up without the player's answer.
    /// </summary>
    public interface IDialogueFlow
    {
        /// <summary>
        /// Plays the dialogue in the window, reading presses from the input context, until it is done or
        /// <paramref name="close" /> is cancelled.
        /// </summary>
        Task RunAsync(IDialogueWindowUI uiInstance, string[] dialogues, DialogueInputContext inputContext, CancellationToken close);
    }

    /// <summary>
    /// One dialogue window: placed, styled and opened, then run through a flow, and closed and destroyed when done.
    /// </summary>
    public interface IDialogueWindow
    {
        /// <summary>Shrinks the window shut and clears its text and choices.</summary>
        Task AnimateWindowClosedAsync();

        /// <summary>Grows the window open at its rect.</summary>
        Task AnimateWindowOpenAsync();

        /// <summary>Removes the window from the screen for good.</summary>
        void Destroy();

        /// <summary>The choice the player answered with, counting from 0.</summary>
        byte GetSelectedChoice();

        /// <summary>Builds the window, hidden, in the container.</summary>
        void Init(VisualElement container);

        /// <summary>
        /// Runs the dialogue through the flow, until it is done or <paramref name="close" /> is cancelled.
        /// </summary>
        Task RunAsync(IDialogueFlow dialogueFlow, string[] dialogues, DialogueInputContext inputContext, CancellationToken close);

        /// <summary>The values the text's variable markup shows, by index.</summary>
        void SetMessageVariables(IReadOnlyList<int> variables);

        /// <summary>
        /// How fast the text types, as the player's message speed setting: 0 is half the authored speed, 0.5 the
        /// authored speed and 1 double it. Each step of the setting scales the speed by the same factor.
        /// </summary>
        void SetMessageSpeed(float messageSpeed);

        /// <summary>Where the window sits on screen, and how big it is when open.</summary>
        void SetRect(RectInt rect);

        /// <summary>Which look the window takes — spoken, thought, caption — as a USS class on it.</summary>
        void SetStyle(DialogueWindowStyle style);
    }
}