using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;
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

        internal static async Task RunPageAsync(IDialogueWindowUI uiInstance, DialoguePage dialoguePage, DialogueInputContext inputContext, CancellationToken close)
        {
            Task closedTask = WhenClosed(close);

            if (await TypePageAsync(uiInstance, dialoguePage, inputContext, closedTask))
            {
                return;
            }

            await Task.WhenAny(inputContext.WaitForConfirmAsync(), closedTask);
        }

        /// <summary>
        /// A page asking a question, its choices set: answered by a confirm press, with the focused choice, or by a
        /// click or a submit on a choice.
        /// </summary>
        internal static async Task RunQuestionPageAsync(IDialogueWindowUI uiInstance, DialoguePage dialoguePage, DialogueInputContext inputContext, CancellationToken close)
        {
            Task closedTask = WhenClosed(close);

            if (await TypePageAsync(uiInstance, dialoguePage, inputContext, closedTask))
            {
                return;
            }

            Task confirmTask = inputContext.WaitForConfirmAsync();
            Task completed   = await Task.WhenAny(confirmTask, uiInstance.WaitForChoiceAsync(), closedTask);

            if (completed == confirmTask)
            {
                uiInstance.ChooseFocusedChoice();
            }
        }

        // A press while the page types finishes the typing rather than moving on. True if the window was closed.
        private static async Task<bool> TypePageAsync(IDialogueWindowUI uiInstance, DialoguePage dialoguePage, DialogueInputContext inputContext, Task closedTask)
        {
            uiInstance.SetText(dialoguePage);

            Task animationTask = uiInstance.RunAsync();
            Task confirmTask   = inputContext.WaitForConfirmAsync();

            Task completed = await Task.WhenAny(animationTask, confirmTask, closedTask);

            if (completed != animationTask)
            {
                uiInstance.SkipToAnimationEnd();
                await animationTask;
            }

            bool closed = completed == closedTask;

            return closed;
        }

        internal static async Task RunUnansweredPageAsync(IDialogueWindowUI uiInstance, DialoguePage dialoguePage, CancellationToken close)
        {
            uiInstance.SetText(dialoguePage);

            Task animationTask = uiInstance.RunAsync();
            Task closedTask    = WhenClosed(close);

            if (await Task.WhenAny(animationTask, closedTask) == closedTask)
            {
                uiInstance.SkipToAnimationEnd();
            }

            await animationTask;
            await closedTask;
        }

        internal static string JoinPages(string dialogue)
        {
            string joined = dialogue.Replace(NEW_PAGE_MARKER, "\n");

            return joined;
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