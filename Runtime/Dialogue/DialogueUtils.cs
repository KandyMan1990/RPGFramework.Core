using System;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;

namespace RPGFramework.Core.Dialogue
{
    internal static class DialogueUtils
    {
        private const string NEW_PAGE_MARKER = "{NewPage}";

        internal static DialogueBlock ParseIntoPages(string dialogue)
        {
            ReadOnlySpan<char> span   = dialogue.AsSpan();
            ReadOnlySpan<char> marker = NEW_PAGE_MARKER.AsSpan();

            int pageCount = 1;
            int idx       = 0;

            while ((idx = span.IndexOf(marker)) >= 0)
            {
                pageCount++;
                span = span[(idx + marker.Length)..];
            }

            DialogueBlock block = new DialogueBlock(pageCount);

            span = dialogue.AsSpan();
            int pageIndex = 0;

            while (true)
            {
                int splitIndex = span.IndexOf(marker);

                ReadOnlySpan<char> pageSpan = splitIndex >= 0 ? span[..splitIndex] : span;

                pageSpan = Trim(pageSpan);

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

                block.AddPage(pageIndex++, new DialoguePage(speaker, text));

                if (splitIndex < 0)
                {
                    break;
                }

                span = span[(splitIndex + marker.Length)..];
            }

            return block;
        }

        internal static async Task RunPageAsync(IDialogueWindowUI uiInstance, DialoguePage dialoguePage, DialogueInputContext inputContext)
        {
            uiInstance.SetText(dialoguePage);

            inputContext.Reset();

            Task animationTask = uiInstance.RunAsync();
            Task confirmTask   = inputContext.WaitForConfirmAsync();

            Task completed = await Task.WhenAny(animationTask, confirmTask);

            if (completed == confirmTask)
            {
                uiInstance.SkipToAnimationEnd();
                await animationTask;
            }

            inputContext.Reset();
            await inputContext.WaitForConfirmAsync();
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