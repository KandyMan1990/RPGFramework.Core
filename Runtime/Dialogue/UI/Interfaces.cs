using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue.UI
{
    /// <summary>
    /// What a dialogue window shows: its frame, its text typed out, and any choices. A flow drives it a page at a time.
    /// </summary>
    public interface IDialogueWindowUI
    {
        /// <summary>Shrinks the window shut and clears its text and choices.</summary>
        Task AnimateWindowClosedAsync();

        /// <summary>Grows the window open at its rect.</summary>
        Task AnimateWindowOpenAsync();

        /// <summary>Removes the window from the screen for good.</summary>
        void Destroy();

        /// <summary>The choice focused when the player confirmed, counting from 0.</summary>
        byte GetSelectedChoice();

        /// <summary>Builds the window, hidden, in the container, from the provider's layout.</summary>
        void Init(VisualElement container);

        /// <summary>
        /// Types the page's text, then each choice in turn, focusing the first; done when everything is shown.
        /// </summary>
        Task RunAsync();

        /// <summary>The answers the page offers, typed after its text; none for a page without a question.</summary>
        void SetChoices(ReadOnlySpan<string> choices);

        /// <summary>The values the text's variable markup shows, by index.</summary>
        void SetMessageVariables(IReadOnlyList<int> variables);

        /// <summary>
        /// How fast the text types, as the player's message speed setting: 0 is half the authored speed, 0.5 the
        /// authored speed and 1 double it.
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

    /// <summary>
    /// The dialogue window's layout, the speeds it types and opens at, and the text styles, set by the game.
    /// </summary>
    public interface IDialogueWindowUIProvider
    {
        /// <summary>The layout a window of this kind is built from.</summary>
        VisualTreeAsset Get<T>() where T : IDialogueWindowUI;

        /// <summary>How many characters a second the text types at the authored speed.</summary>
        float TextSpeed { get; }

        /// <summary>How fast the window opens and closes: the reciprocal of the seconds it takes.</summary>
        float WindowSpeed { get; }

        /// <summary>The named styles dialogue markup can use, such as <c>{Location}</c>.</summary>
        DialogueTextStyles TextStyles { get; }
    }
}