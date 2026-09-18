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
            List<string>  pages = SplitPages(dialogue);
            DialogueBlock block = new DialogueBlock(pages.Count);

            for (int pageIndex = 0; pageIndex < pages.Count; pageIndex++)
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

        internal static List<string> SplitPages(string dialogue)
        {
            List<string> pages = new List<string>();
            int          start = 0;

            while (true)
            {
                int marker = dialogue.IndexOf(NEW_PAGE_MARKER, start, StringComparison.Ordinal);

                if (marker < 0)
                {
                    pages.Add(dialogue.Substring(start));
                    return pages;
                }

                pages.Add(dialogue.Substring(start, marker - start));
                start = marker + NEW_PAGE_MARKER.Length;
            }
        }

        internal static async Task RunPageAsync(IDialogueWindowUI uiInstance, DialoguePage dialoguePage, DialogueInputContext inputContext, CancellationToken close)
        {
            uiInstance.SetText(dialoguePage);

            Task animationTask = uiInstance.RunAsync();
            Task confirmTask   = inputContext.WaitForConfirmAsync();
            Task closedTask    = WhenClosed(close);

            Task completed = await Task.WhenAny(animationTask, confirmTask, closedTask);

            if (completed != animationTask)
            {
                uiInstance.SkipToAnimationEnd();
                await animationTask;
            }

            if (completed == closedTask)
            {
                return;
            }

            await Task.WhenAny(inputContext.WaitForConfirmAsync(), closedTask);
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