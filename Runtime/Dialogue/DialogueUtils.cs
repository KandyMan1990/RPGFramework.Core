using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGFramework.Core.Dialogue
{
    internal static class DialogueUtils
    {
        private const string NEW_PAGE_MARKER = "{NewPage}";

        internal static DialogueBlock ParseIntoPages(string dialogue)
        {
            string[]      pages = SplitPages(dialogue);
            DialogueBlock block = new DialogueBlock(pages.Length);

            for (int pageIndex = 0; pageIndex < pages.Length; pageIndex++)
            {
                ReadOnlySpan<char> pageSpan    = Trim(pages[pageIndex].AsSpan());
                ReadOnlySpan<char> speakerSpan = default;

                if (!pageSpan.IsEmpty && pageSpan[0] == '[')
                {
                    int end = pageSpan.IndexOf(']');
                    if (end > 0)
                    {
                        speakerSpan = pageSpan[1..end];
                        pageSpan    = TrimStart(pageSpan[(end + 1)..]);
                    }
                }

                string speaker = speakerSpan.IsEmpty ? string.Empty : speakerSpan.ToString();
                string text    = pageSpan.ToString();

                block.AddPage(pageIndex, new DialoguePage(speaker, text));
            }

            return block;
        }

        internal static string[] SplitPages(string dialogue)
        {
            return dialogue.Split(NEW_PAGE_MARKER);
        }

        /// <summary>
        /// A page the player reads, then turns with a press while the window has the focus — or, the last, closes with
        /// one wherever the focus is.
        /// </summary>
        internal static async Task RunPageAsync(IDialogueWindow window, DialoguePage dialoguePage, DialogueInputContext inputContext, CancellationToken close, bool isLast)
        {
            Task closedTask = WhenClosed(close);

            if (await TypePageAsync(window, dialoguePage, inputContext, closedTask))
            {
                return;
            }

            Task press = isLast ? inputContext.WaitForPressAsync() : inputContext.WaitForFocusedPressAsync(window);

            await Task.WhenAny(press, closedTask);
        }

        /// <summary>
        /// A page of a background window, turned with a press while the window has the focus. The last stays up until a
        /// script closes it, or a press does once a script waits for it.
        /// </summary>
        /// <returns>False once the window is closed.</returns>
        internal static async Task<bool> RunBackgroundPageAsync(IDialogueWindow window, DialoguePage dialoguePage, DialogueInputContext inputContext, CancellationToken close, bool isLast)
        {
            Task closedTask = WhenClosed(close);

            if (await TypePageAsync(window, dialoguePage, inputContext, closedTask))
            {
                return false;
            }

            Task press = isLast ? inputContext.WaitForPressToCloseAsync(window) : inputContext.WaitForFocusedPressAsync(window);

            return await Task.WhenAny(press, closedTask) != closedTask;
        }

        /// <summary>
        /// A page asking a question, its choices set: answered by a confirm press, with the focused choice, or by a
        /// click or a submit on a choice. A cancel press while it waits calls <paramref name="cancelled" />, if given.
        /// </summary>
        /// <returns>True once answered, false if the window was closed first.</returns>
        internal static async Task<bool> RunQuestionPageAsync(IDialogueWindow window, DialoguePage dialoguePage, DialogueInputContext inputContext, CancellationToken close, Action cancelled)
        {
            Task closedTask = WhenClosed(close);

            if (await TypePageAsync(window, dialoguePage, inputContext, closedTask))
            {
                return false;
            }

            Task choiceTask = window.WaitForChoiceAsync();

            while (true)
            {
                Task confirmTask = inputContext.WaitForFocusedPressAsync(window);
                Task cancelTask  = inputContext.WaitForCancelAsync(window);
                Task completed   = await Task.WhenAny(confirmTask, cancelTask, choiceTask, closedTask);

                if (completed == cancelTask)
                {
                    cancelled?.Invoke();
                    continue;
                }

                if (completed == confirmTask)
                {
                    window.ChooseFocusedChoice();
                }

                return completed != closedTask;
            }
        }

        // A press while the page types finishes the typing rather than moving on. True if the window was closed.
        private static async Task<bool> TypePageAsync(IDialogueWindow window, DialoguePage dialoguePage, DialogueInputContext inputContext, Task closedTask)
        {
            window.SetText(dialoguePage);

            Task animationTask = window.RunAsync();
            Task confirmTask   = inputContext.WaitForFocusedPressAsync(window);

            Task completed = await Task.WhenAny(animationTask, confirmTask, closedTask);

            if (completed != animationTask)
            {
                window.SkipToAnimationEnd();
                await animationTask;
            }

            bool closed = completed == closedTask;

            return closed;
        }

        private static Task WhenClosed(CancellationToken close)
        {
            TaskCompletionSource<bool> closed = new TaskCompletionSource<bool>();

            close.Register(() => closed.TrySetResult(true));

            Task task = closed.Task;

            return task;
        }

        private static ReadOnlySpan<char> Trim(ReadOnlySpan<char> span)
        {
            return TrimEnd(TrimStart(span));
        }

        private static ReadOnlySpan<char> TrimStart(ReadOnlySpan<char> span)
        {
            int i = 0;
            while (i < span.Length && char.IsWhiteSpace(span[i]))
            {
                i++;
            }

            return span[i..];
        }

        private static ReadOnlySpan<char> TrimEnd(ReadOnlySpan<char> span)
        {
            int i = span.Length - 1;
            while (i >= 0 && char.IsWhiteSpace(span[i]))
            {
                i--;
            }

            return span[..(i + 1)];
        }
    }
}