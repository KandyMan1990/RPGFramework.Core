using System.Collections.Generic;
using UnityEngine;

namespace RPGFramework.Core.Dialogue
{
    /// <summary>
    /// A page of dialogue with its markup resolved: the characters shown, and by character position, what colour
    /// they are, which blink, and where the typing pauses. See <see cref="DialogueMarkup" />.
    /// </summary>
    public sealed class DialogueText
    {
        public  string                          Text   { get; }
        public  IReadOnlyList<DialoguePause>    Pauses { get; }
        private IReadOnlyList<DialogueTextSpan> Spans  { get; }

        public DialogueText(string text, IReadOnlyList<DialoguePause> pauses, IReadOnlyList<DialogueTextSpan> spans)
        {
            Text   = text;
            Pauses = pauses;
            Spans  = spans;
        }

        /// <summary>
        /// The colour of the innermost coloured span covering a character, if any.
        /// </summary>
        public bool TryGetColour(int index, out Color colour)
        {
            int innermost = -1;

            colour = default;

            foreach (DialogueTextSpan span in Spans)
            {
                if (span.Colour.HasValue && span.Contains(index) && span.Start >= innermost)
                {
                    innermost = span.Start;
                    colour    = span.Colour.Value;
                }
            }

            bool found = innermost >= 0;

            return found;
        }

        public bool IsBlinking(int index)
        {
            foreach (DialogueTextSpan span in Spans)
            {
                if (span.Blink && span.Contains(index))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Characters <see cref="Start" /> up to, but not including, <see cref="End" />.
    /// </summary>
    public readonly struct DialogueTextSpan
    {
        public readonly int    Start;
        public readonly int    End;
        public readonly Color? Colour;
        public readonly bool   Blink;

        public DialogueTextSpan(int start, int end, Color? colour, bool blink)
        {
            Start  = start;
            End    = end;
            Colour = colour;
            Blink  = blink;
        }

        public bool Contains(int index)
        {
            bool contains = index >= Start && index < End;

            return contains;
        }
    }

    /// <summary>
    /// Typing stops for <see cref="Seconds" /> once <see cref="Index" /> characters are showing.
    /// </summary>
    public readonly struct DialoguePause
    {
        public readonly int   Index;
        public readonly float Seconds;

        public DialoguePause(int index, float seconds)
        {
            Index   = index;
            Seconds = seconds;
        }
    }
}