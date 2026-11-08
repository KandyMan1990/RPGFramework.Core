using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.Dialogue.UI;
using RPGFramework.Core.UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue
{
    internal sealed class DialogueWindow : IDialogueWindow
    {
        event Action IDialogueWindow.OnCursorMoved
        {
            add => m_OnCursorMoved += value;
            remove => m_OnCursorMoved -= value;
        }

        private const string TEXT = "Text";

        private const string STYLE_CLASS_PREFIX = "dialogue-window--";

        private const string FITTED_CLASS = "dialogue-window--fitted";

        private const string SAMPLE_ANSWER = "X";

        private readonly IDialogueWindowUIProvider m_DialogueWindowUIProvider;
        private readonly float                     m_TextSpeed;
        private readonly float                     m_WindowSpeed;
        private readonly DialogueTextStyles        m_TextStyles;

        private event Action m_OnCursorMoved;

        private RectInt                     m_Rect;
        private Vector2Int                  m_FitPosition;
        private string                      m_FitMessage;
        private string[]                    m_FitAnswers;
        private bool                        m_SkipRequested;
        private Label                       m_Text;
        private DialogueTextView            m_TextView;
        private VisualElement               m_UIInstance;
        private RPGUIButton[]               m_Choices;
        private DialogueTextView[]          m_ChoiceViews;
        private string[]                    m_ChoiceTexts;
        private byte                        m_SelectedIndex;
        private TaskCompletionSource<bool>  m_Chosen;
        private SelectionScope              m_Selection;
        private IReadOnlyList<int>          m_MessageVariables;
        private DialogueWindowStyle         m_Style;
        private IVisualElementScheduledItem m_Ticker;
        private float                       m_TextSpeedMultiplier;

        public DialogueWindow(IDialogueWindowUIProvider uiProvider)
        {
            m_DialogueWindowUIProvider = uiProvider;
            m_TextSpeed                = uiProvider.TextSpeed;
            m_WindowSpeed              = uiProvider.WindowSpeed;
            m_TextStyles               = uiProvider.TextStyles;
            m_Choices                  = Array.Empty<RPGUIButton>();
            m_ChoiceViews              = Array.Empty<DialogueTextView>();
            m_ChoiceTexts              = Array.Empty<string>();
            m_FitAnswers               = Array.Empty<string>();
            m_Chosen                   = NewChosen();
            m_TextSpeedMultiplier      = 1f;
        }

        async Task IDialogueWindow.AnimateWindowClosedAsync()
        {
            m_Selection.SetInputEnabled(false);

            for (int i = 0; i < m_Choices.Length; i++)
            {
                m_ChoiceViews[i].Clear();
                m_Choices[i].RemoveFromHierarchy();
            }

            m_Choices     = Array.Empty<RPGUIButton>();
            m_ChoiceViews = Array.Empty<DialogueTextView>();
            m_ChoiceTexts = Array.Empty<string>();

            m_TextView.Clear();
            m_UIInstance.RemoveFromClassList(FITTED_CLASS);

            float targetX  = m_Rect.x + (m_Rect.width  / 2f);
            float targetY  = m_Rect.y + (m_Rect.height / 2f);
            float progress = 0f;

            while (progress < 1f)
            {
                float x      = math.lerp(m_Rect.x,      targetX, progress);
                float y      = math.lerp(m_Rect.y,      targetY, progress);
                float width  = math.lerp(m_Rect.width,  0f,      progress);
                float height = math.lerp(m_Rect.height, 0f,      progress);

                m_UIInstance.style.left   = x;
                m_UIInstance.style.top    = y;
                m_UIInstance.style.width  = width;
                m_UIInstance.style.height = height;

                progress += Time.unscaledDeltaTime * m_WindowSpeed;

                await Awaitable.NextFrameAsync();
            }

            m_UIInstance.style.width  = 0f;
            m_UIInstance.style.height = 0f;
        }

        async Task IDialogueWindow.AnimateWindowOpenAsync()
        {
            if (m_FitMessage != null)
            {
                await FitRectToTextAsync();
            }

            m_UIInstance.style.left   = m_Rect.x;
            m_UIInstance.style.top    = m_Rect.y;
            m_UIInstance.style.width  = 0f;
            m_UIInstance.style.height = 0f;

            m_UIInstance.style.display = DisplayStyle.Flex;

            float progress = 0f;

            while (progress < 1f)
            {
                float x = math.lerp(0f, m_Rect.width,  progress);
                float y = math.lerp(0f, m_Rect.height, progress);

                m_UIInstance.style.width  = x;
                m_UIInstance.style.height = y;

                progress += Time.unscaledDeltaTime * m_WindowSpeed;

                await Awaitable.NextFrameAsync();
            }

            m_UIInstance.style.width  = m_Rect.width;
            m_UIInstance.style.height = m_Rect.height;

            if (m_FitMessage != null)
            {
                m_UIInstance.AddToClassList(FITTED_CLASS);
            }
        }

        void IDialogueWindow.Destroy()
        {
            m_Ticker.Pause();
            m_UIInstance.RemoveFromHierarchy();
            m_UIInstance = null;
        }

        byte IDialogueWindow.GetSelectedChoice()
        {
            return m_SelectedIndex;
        }

        void IDialogueWindow.ChooseFocusedChoice()
        {
            Choose(m_SelectedIndex);
        }

        Task IDialogueWindow.WaitForChoiceAsync()
        {
            return m_Chosen.Task;
        }

        void IDialogueWindow.Init(VisualElement container)
        {
            m_UIInstance               = CloneWindow(container);
            m_UIInstance.style.display = DisplayStyle.None;

            m_Text      = m_UIInstance.Q<Label>(TEXT);
            m_Text.text = string.Empty;
            m_TextView  = new DialogueTextView(m_Text);

            ApplyStyle(m_UIInstance);

            m_Selection = new SelectionScope(m_UIInstance);

            m_Selection.Moved += OnSelectionMoved;

            m_Ticker = m_UIInstance.schedule.Execute(Tick).Every(0);
        }

        async Task IDialogueWindow.RunAsync()
        {
            await TypeAsync(m_TextView);

            if (m_Choices.Length > 0)
            {
                for (int i = 0; i < m_Choices.Length; i++)
                {
                    m_Choices[i].SetEnabledAndVisible(true);
                    m_ChoiceViews[i].Show(string.Empty, Parse(m_ChoiceTexts[i]));
                    await TypeAsync(m_ChoiceViews[i]);
                }

                m_Choices[m_SelectedIndex].Focus();
                m_Selection.SetInputEnabled(true);
            }
        }

        void IDialogueWindow.FocusChoice(byte index)
        {
            m_SelectedIndex = index;
            m_Choices[index].Focus();
        }

        void IDialogueWindow.SetChoices(ReadOnlySpan<string> choices, byte startingChoice)
        {
            int length = choices.Length;

            m_Choices       = new RPGUIButton[length];
            m_ChoiceViews   = new DialogueTextView[length];
            m_ChoiceTexts   = choices.ToArray();
            m_SelectedIndex = startingChoice;
            m_Chosen        = NewChosen();

            for (byte i = 0; i < length; i++)
            {
                RPGUIButton button = NewAnswerButton(string.Empty, m_Text);

                byte index = i;

                button.Chosen += () => Choose(index);
                button.RegisterCallback<FocusInEvent, byte>(OnChoiceFocused, i);
                button.SetEnabledAndVisible(false);

                m_UIInstance.Add(button);
                m_Choices[i]     = button;
                m_ChoiceViews[i] = new DialogueTextView(button.Label);
            }

            RPGUIButton.LinkColumn(m_Choices, true);
        }

        void IDialogueWindow.SetMessageVariables(IReadOnlyList<int> variables)
        {
            m_MessageVariables = variables;
        }

        void IDialogueWindow.SetMessageSpeed(float messageSpeed)
        {
            m_TextSpeedMultiplier = math.pow(2f, 2f * messageSpeed - 1f);
        }

        void IDialogueWindow.SetRect(RectInt rect)
        {
            m_Rect = rect;
        }

        void IDialogueWindow.FitToText(Vector2Int position, string message, ReadOnlySpan<string> answers)
        {
            m_FitPosition = position;
            m_FitMessage  = message;
            m_FitAnswers  = answers.ToArray();
        }

        void IDialogueWindow.SetStyle(DialogueWindowStyle style)
        {
            m_Style = style;

            if (m_UIInstance != null)
            {
                ApplyStyle(m_UIInstance);
            }
        }

        void IDialogueWindow.SetText(DialoguePage dialoguePage)
        {
            string speaker = SpeakerLine(dialoguePage);

            m_TextView.Show(speaker, Parse(dialoguePage.Text));
            m_SkipRequested = false;
        }

        void IDialogueWindow.SkipToAnimationEnd()
        {
            m_SkipRequested = true;
        }

        // An explicit '\n' rather than a platform line ending, so the character positions the styling is keyed to are the
        // same everywhere.
        private static string SpeakerLine(DialoguePage dialoguePage)
        {
            return string.IsNullOrWhiteSpace(dialoguePage.SpeakerId) ? string.Empty : dialoguePage.SpeakerId + "\n";
        }

        private VisualElement CloneWindow(VisualElement container)
        {
            VisualTreeAsset uiAsset = m_DialogueWindowUIProvider.Get<IDialogueWindow>();

            uiAsset.CloneTree(container, out int index, out _);

            return container[index];
        }

        private static RPGUIButton NewAnswerButton(string text, Label label)
        {
            RPGUIButton button = new RPGUIButton
                                 {
                                     text = text,
                                     style =
                                     {
                                         fontSize = label.resolvedStyle.fontSize
                                     }
                                 };

            return button;
        }

        // Measures an unseen copy of the window, as measuring a label writes into what it draws from. A label measures
        // nothing until its styles are worked out, the frame after it joins the panel, and an answer's own layout takes
        // a frame more.
        private async Task FitRectToTextAsync()
        {
            VisualElement container = m_UIInstance.parent;
            VisualElement copy      = CloneWindow(container);
            Label         label     = copy.Q<Label>(TEXT);

            copy.AddToClassList(FITTED_CLASS);
            ApplyStyle(copy);
            copy.style.visibility = Visibility.Hidden;

            await Awaitable.NextFrameAsync();

            RPGUIButton sample = null;

            if (m_FitAnswers.Length > 0)
            {
                sample = NewAnswerButton(SAMPLE_ANSWER, label);

                copy.Add(sample);

                await Awaitable.NextFrameAsync();
            }

            Vector2 screen = container.layout.size;

            m_Rect = DialogueWindowFit.Rect(m_FitPosition, MeasureMessage(copy, label, sample, screen), screen);

            copy.RemoveFromHierarchy();
        }

        private Vector2 MeasureMessage(VisualElement copy, Label label, RPGUIButton sample, Vector2 screen)
        {
            IResolvedStyle window = copy.resolvedStyle;
            IResolvedStyle text   = label.resolvedStyle;

            float frameWidth = window.paddingLeft + window.paddingRight + window.borderLeftWidth + window.borderRightWidth +
                               text.marginLeft    + text.marginRight    + text.paddingLeft       + text.paddingRight;

            float frameHeight = window.paddingTop + window.paddingBottom + window.borderTopWidth + window.borderBottomWidth +
                                text.marginTop    + text.marginBottom    + text.paddingTop       + text.paddingBottom;

            float widest = DialogueWindowFit.WidestText(screen.x, frameWidth);

            MeasureAnswers(sample, widest, out float width, out float answersHeight);

            DialogueBlock block  = DialogueUtils.ParseIntoPages(m_FitMessage);
            float         height = 0f;

            for (int i = 0; i < block.Pages.Length; i++)
            {
                DialoguePage page = block.Pages[i];
                Vector2      size = MeasureText(label, SpeakerLine(page) + Parse(page.Text).Text, widest);

                // The answers show under the last page alone.
                float pageHeight = i == block.Pages.Length - 1 ? size.y + answersHeight : size.y;

                width  = math.max(width,  size.x);
                height = math.max(height, pageHeight);
            }

            return new Vector2(width + frameWidth, height + frameHeight);
        }

        private void MeasureAnswers(RPGUIButton sample, float widest, out float width, out float height)
        {
            width  = 0f;
            height = 0f;

            if (sample == null)
            {
                return;
            }

            float textStart   = sample.Label.layout.x;
            float rowHeight   = sample.layout.height + sample.resolvedStyle.marginTop + sample.resolvedStyle.marginBottom;
            float lineHeight  = MeasureText(sample.Label, SAMPLE_ANSWER, widest).y;

            for (int i = 0; i < m_FitAnswers.Length; i++)
            {
                Vector2 size = MeasureText(sample.Label, Parse(m_FitAnswers[i]).Text, widest - textStart);

                width   = math.max(width, textStart + size.x);
                height += rowHeight + math.max(0f, size.y - lineHeight);
            }
        }

        // A point more than the text's own width, as one exactly as wide can wrap its last word from rounding.
        private static Vector2 MeasureText(Label label, string text, float widest)
        {
            Vector2 size = label.MeasureTextSize(text, widest, VisualElement.MeasureMode.AtMost, 0f, VisualElement.MeasureMode.Undefined);

            return new Vector2(math.ceil(size.x) + 1f, math.ceil(size.y));
        }

        private DialogueText Parse(string text)
        {
            DialogueText parsed = DialogueMarkup.Parse(text, m_MessageVariables, m_TextStyles, null);

            return parsed;
        }

        private void ApplyStyle(VisualElement window)
        {
            DialogueWindowStyle[] styles = (DialogueWindowStyle[])Enum.GetValues(typeof(DialogueWindowStyle));

            for (int i = 0; i < styles.Length; i++)
            {
                DialogueWindowStyle style = styles[i];

                window.EnableInClassList(StyleClass(style), style == m_Style);
            }
        }

        private static string StyleClass(DialogueWindowStyle style)
        {
            string styleClass = STYLE_CLASS_PREFIX + style.ToString().ToLowerInvariant();

            return styleClass;
        }

        private void Tick(TimerState timerState)
        {
            float deltaTime           = timerState.deltaTime / 1000f;
            float charactersPerSecond = m_TextSpeed          * m_TextSpeedMultiplier;

            m_TextView.Tick(deltaTime, charactersPerSecond);

            for (int i = 0; i < m_ChoiceViews.Length; i++)
            {
                DialogueTextView choiceView = m_ChoiceViews[i];

                choiceView.Tick(deltaTime, charactersPerSecond);
            }
        }

        private void OnSelectionMoved()
        {
            m_OnCursorMoved?.Invoke();
        }

        private void OnChoiceFocused(FocusInEvent evt, byte index)
        {
            m_SelectedIndex = index;
        }

        private void Choose(byte index)
        {
            if (m_Chosen.Task.IsCompleted)
            {
                return;
            }

            m_SelectedIndex = index;
            m_Selection.SetInputEnabled(false);
            m_Chosen.TrySetResult(true);
        }

        private static TaskCompletionSource<bool> NewChosen()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private async Task TypeAsync(DialogueTextView view)
        {
            while (view.IsTyping)
            {
                if (m_SkipRequested)
                {
                    view.SkipToEnd();
                    break;
                }

                await Awaitable.NextFrameAsync();
            }
        }
    }
}