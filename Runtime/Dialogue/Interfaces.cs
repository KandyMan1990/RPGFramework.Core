using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        /// <summary>How the window takes the player's press, as it is opened in the input context.</summary>
        DialogueWindowKind Kind { get; }

        /// <summary>
        /// Plays the dialogue in the window, opened in the input context, reading presses from it, until it is done or
        /// <paramref name="close" /> is cancelled.
        /// </summary>
        Task RunAsync(IDialogueWindow window, string[] dialogues, DialogueInputContext inputContext, CancellationToken close);
    }

    /// <summary>
    /// One dialogue window: placed, styled and opened, its frame, its text typed out and any choices shown a page at a
    /// time as a flow drives it, then closed and destroyed when done. It plays no sound: it reports the cursor moving
    /// between choices, and the flow decides what is heard.
    /// </summary>
    public interface IDialogueWindow
    {
        /// <summary>The player moved the cursor to another choice.</summary>
        event Action OnCursorMoved;

        /// <summary>Shrinks the window shut and clears its text and choices.</summary>
        Task AnimateWindowClosedAsync();

        /// <summary>Grows the window open at its rect.</summary>
        Task AnimateWindowOpenAsync();

        /// <summary>Removes the window from the screen for good.</summary>
        void Destroy();

        /// <summary>The choice the player answered with, counting from 0.</summary>
        byte GetSelectedChoice();

        /// <summary>
        /// Answers the question with the focused choice, as a confirm press does. A question is answered once.
        /// </summary>
        void ChooseFocusedChoice();

        /// <summary>
        /// Completes once the question is answered: a click or a submit on a choice, or
        /// <see cref="ChooseFocusedChoice" />.
        /// </summary>
        Task WaitForChoiceAsync();

        /// <summary>Builds the window, hidden, in the container, from the provider's layout.</summary>
        void Init(VisualElement container);

        /// <summary>
        /// Types the page's text, then each choice in turn; done when everything is shown, the first choice focused and
        /// the choices taking the cursor.
        /// </summary>
        Task RunAsync();

        /// <summary>The answers the page offers, typed after its text; none for a page without a question.</summary>
        void SetChoices(ReadOnlySpan<string> choices);

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

        /// <summary>Shows a page, its speaker's name on the first line, ready to type.</summary>
        void SetText(DialoguePage dialoguePage);

        /// <summary>Finishes typing at once, as a press during it asks.</summary>
        void SkipToAnimationEnd();
    }
}