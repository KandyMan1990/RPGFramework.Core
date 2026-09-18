using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace RPGFramework.Core.Dialogue
{
    /// <summary>
    /// Turns a page of dialogue as a writer typed it into the text shown and what happens to it.<br /><br />
    /// Commands sit in curly braces. A command that takes a value is followed by a space and the value; a run
    /// of styled text is opened with the style's name and closed with a slash before it:
    /// <code>
    /// You found a {KeyItem}sword{/KeyItem}!{Wait 1} You have {Var 0} gold.
    /// </code>
    /// <list type="bullet">
    /// <item><c>{Var n}</c> — the value of message variable n, 0 to <see cref="MESSAGE_VARIABLE_COUNT" /> - 1.</item>
    /// <item><c>{Wait s}</c> — pause the typing for s seconds.</item>
    /// <item><c>{Name}…{/Name}</c> — a style declared in <see cref="DialogueTextStyles" />.</item>
    /// <item><c>{Colour c}…{/Colour}</c> — a colour, as <c>#RRGGBB</c> or a name such as <c>yellow</c>.</item>
    /// <item><c>{Blink}…{/Blink}</c> — flash the text.</item>
    /// <item><c>{{</c> — a literal brace.</item>
    /// </list>
    /// The text produced carries no markup of any kind, so a character position in it is a character position on
    /// screen; colour, blinking and pauses are kept beside it by position.
    /// </summary>
    public static class DialogueMarkup
    {
        public const int MESSAGE_VARIABLE_COUNT = 8;

        private const string VAR    = "Var";
        private const string WAIT   = "Wait";
        private const string COLOUR = "Colour";
        private const string BLINK  = "Blink";

        private static readonly Dictionary<string, Color> s_ColourNames = new Dictionary<string, Color>(System.StringComparer.OrdinalIgnoreCase)
                                                                          {
                                                                              { "white", Color.white },
                                                                              { "black", Color.black },
                                                                              { "grey", Color.grey },
                                                                              { "gray", Color.grey },
                                                                              { "red", Color.red },
                                                                              { "green", new Color(0f, 0.5f, 0f) },
                                                                              { "lime", Color.green },
                                                                              { "blue", Color.blue },
                                                                              { "yellow", new Color(1f, 0.92f, 0.016f) },
                                                                              { "cyan", Color.cyan },
                                                                              { "magenta", Color.magenta },
                                                                              { "orange", new Color(1f,   0.647f, 0f) },
                                                                              { "purple", new Color(0.5f, 0f,     0.5f) }
                                                                          };

        /// <param name="source">One page of text, with <c>{NewPage}</c> already split out.</param>
        /// <param name="variables">The message variables; missing slots read as 0.</param>
        /// <param name="styles">The named styles; null means none are declared.</param>
        /// <param name="problems">Collects a readable line per mistake. Null to ignore them.</param>
        public static DialogueText Parse(string source, IReadOnlyList<int> variables, IDialogueTextStyles styles, List<string> problems)
        {
            StringBuilder          text   = new StringBuilder(source.Length);
            List<DialogueTextSpan> spans  = new List<DialogueTextSpan>();
            List<DialoguePause>    pauses = new List<DialoguePause>();
            Stack<OpenSpan>        open   = new Stack<OpenSpan>();

            int i = 0;

            while (i < source.Length)
            {
                char c = source[i];

                if (c != '{')
                {
                    text.Append(c);
                    i++;
                    continue;
                }

                if (i + 1 < source.Length && source[i + 1] == '{')
                {
                    text.Append('{');
                    i += 2;
                    continue;
                }

                int close = source.IndexOf('}', i + 1);

                if (close < 0)
                {
                    problems?.Add($"'{{' at character {i} is never closed with '}}'. Write {{{{ for a literal brace");
                    text.Append(source, i, source.Length - i);
                    break;
                }

                string command = source.Substring(i + 1, close - i - 1).Trim();
                i = close + 1;

                if (command.StartsWith("/"))
                {
                    CloseSpan(command.Substring(1).Trim(), text.Length, open, spans, problems);
                    continue;
                }

                int    space    = command.IndexOf(' ');
                string name     = space < 0 ? command : command.Substring(0, space);
                string argument = space < 0 ? string.Empty : command.Substring(space + 1).Trim();

                switch (name)
                {
                    case VAR:
                        text.Append(ReadVariable(argument, variables, problems));
                        break;

                    case WAIT:
                        if (TryParseSeconds(argument, out float seconds))
                        {
                            pauses.Add(new DialoguePause(text.Length, seconds));
                        }
                        else
                        {
                            problems?.Add($"{{{WAIT} {argument}}} needs a number of seconds, such as {{{WAIT} 1}} or {{{WAIT} 0.5}}");
                        }

                        break;

                    case COLOUR:
                        if (TryParseColour(argument, out Color colour))
                        {
                            open.Push(new OpenSpan(COLOUR, text.Length, colour, false));
                        }
                        else
                        {
                            problems?.Add($"{{{COLOUR} {argument}}} needs a colour, such as {{{COLOUR} #2CE2DD}} or {{{COLOUR} yellow}}");
                            open.Push(new OpenSpan(COLOUR, text.Length, null, false));
                        }

                        break;

                    case BLINK:
                        open.Push(new OpenSpan(BLINK, text.Length, null, true));
                        break;

                    default:
                        if (styles != null && styles.TryGet(name, out DialogueTextStyle style))
                        {
                            open.Push(new OpenSpan(name, text.Length, style.Colour, style.Blink));
                        }
                        else
                        {
                            problems?.Add($"{{{command}}} is not a command or a declared style");
                        }

                        break;
                }
            }

            while (open.Count > 0)
            {
                OpenSpan span = open.Pop();

                problems?.Add($"{{{span.Name}}} is never closed with {{/{span.Name}}}");
                spans.Add(new DialogueTextSpan(span.Start, text.Length, span.Colour, span.Blink));
            }

            DialogueText result = new DialogueText(text.ToString(), pauses, spans);

            return result;
        }

        /// <summary>
        /// Parse only to find mistakes, as the editor does when text arrives from a sheet.
        /// </summary>
        public static List<string> Validate(string source, IDialogueTextStyles styles)
        {
            List<string> problems = new List<string>();

            foreach (string page in DialogueUtils.SplitPages(source))
            {
                Parse(page, null, styles, problems);
            }

            return problems;
        }

        /// <summary>
        /// A line split into its pages, each with its speaker taken off the front, as a window shows them.
        /// </summary>
        public static IReadOnlyList<DialoguePage> ToPages(string source)
        {
            IReadOnlyList<DialoguePage> pages = DialogueUtils.ParseIntoPages(source).Pages;

            return pages;
        }

        private static bool TryParseColour(string value, out Color colour)
        {
            if (s_ColourNames.TryGetValue(value, out colour))
            {
                return true;
            }

            string hex = value.StartsWith("#") ? value.Substring(1) : string.Empty;

            if ((hex.Length != 6 && hex.Length != 8) || !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint packed))
            {
                colour = default;
                return false;
            }

            if (hex.Length == 6)
            {
                packed = (packed << 8) | 0xFF;
            }

            colour = new Color32((byte)(packed >> 24), (byte)(packed >> 16), (byte)(packed >> 8), (byte)packed);

            return true;
        }

        private static void CloseSpan(string name, int end, Stack<OpenSpan> open, List<DialogueTextSpan> spans, List<string> problems)
        {
            if (open.Count == 0 || open.Peek().Name != name)
            {
                string expected = open.Count == 0 ? "nothing is open" : $"{{/{open.Peek().Name}}} closes first";
                problems?.Add($"{{/{name}}} does not match: {expected}");
                return;
            }

            OpenSpan span = open.Pop();

            spans.Add(new DialogueTextSpan(span.Start, end, span.Colour, span.Blink));
        }

        private static string ReadVariable(string argument, IReadOnlyList<int> variables, List<string> problems)
        {
            if (!int.TryParse(argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out int slot) || slot < 0 || slot >= MESSAGE_VARIABLE_COUNT)
            {
                problems?.Add($"{{{VAR} {argument}}} needs a slot from 0 to {MESSAGE_VARIABLE_COUNT - 1}, such as {{{VAR} 0}}");
                return string.Empty;
            }

            int    value = variables != null && slot < variables.Count ? variables[slot] : 0;
            string shown = value.ToString(CultureInfo.InvariantCulture);

            return shown;
        }

        private static bool TryParseSeconds(string value, out float seconds)
        {
            bool parsed = float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds) && seconds >= 0f;

            return parsed;
        }

        private readonly struct OpenSpan
        {
            internal readonly string Name;
            internal readonly int    Start;
            internal readonly Color? Colour;
            internal readonly bool   Blink;

            internal OpenSpan(string name, int start, Color? colour, bool blink)
            {
                Name   = name;
                Start  = start;
                Colour = colour;
                Blink  = blink;
            }
        }
    }
}