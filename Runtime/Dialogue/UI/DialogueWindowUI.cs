using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.Audio;
using RPGFramework.Core.Input;
using RPGFramework.Core.UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue.UI
{
    internal sealed class DialogueWindowUI : IDialogueWindowUI
    {
        private const string TEXT = "Text";

        private const string STYLE_CLASS_PREFIX = "dialogue-window--";

        private readonly IDialogueWindowUIProvider m_DialogueWindowUIProvider;
        private readonly IAudioIntentPlayer        m_AudioIntentPlayer;
        private readonly float                     m_TextSpeed;
        private readonly float                     m_WindowSpeed;
        private readonly DialogueTextStyles        m_TextStyles;

        private RectInt                     m_Rect;
        private bool                        m_SkipRequested;
        private Label                       m_Text;
        private DialogueTextView            m_TextView;
        private VisualElement               m_UIInstance;
        private RPGUIButton[]               m_Choices;
        private DialogueTextView[]          m_ChoiceViews;
        private string[]                    m_ChoiceTexts;
        private byte                        m_SelectedIndex;
        private IReadOnlyList<int>          m_MessageVariables;
        private DialogueWindowStyle         m_Style;
        private IVisualElementScheduledItem m_Ticker;
        private float                       m_TextSpeedMultiplier = 1f;

        public DialogueWindowUI(IDialogueWindowUIProvider uiProvider, IAudioIntentPlayer audioIntentPlayer)
        {
            m_DialogueWindowUIProvider = uiProvider;
            m_AudioIntentPlayer        = audioIntentPlayer;
            m_TextSpeed                = uiProvider.TextSpeed;
            m_WindowSpeed              = uiProvider.WindowSpeed;
            m_TextStyles               = uiProvider.TextStyles;
            m_Choices                  = Array.Empty<RPGUIButton>();
            m_ChoiceViews              = Array.Empty<DialogueTextView>();
            m_ChoiceTexts              = Array.Empty<string>();
        }

        async Task IDialogueWindowUI.AnimateWindowClosedAsync()
        {
            for (byte i = 0; i < m_Choices.Length; i++)
            {
                m_Choices[i].UnregisterCallback<ClickEvent, byte>(OnChoiceChosenBtnClicked);
                m_Choices[i].UnregisterCallback<NavigationSubmitEvent, byte>(OnChoiceChosenBtnSubmitted);
                m_Choices[i].UnregisterCallback<NavigationMoveEvent, byte>(OnChoiceChosenBtnNavigate);

                m_ChoiceViews[i].Clear();
                m_Choices[i].RemoveFromHierarchy();
                m_Choices[i] = null;
            }

            m_TextView.Clear();

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

                progress += Time.deltaTime * m_WindowSpeed;

                await Awaitable.NextFrameAsync();
            }

            m_UIInstance.style.width  = 0f;
            m_UIInstance.style.height = 0f;
        }

        async Task IDialogueWindowUI.AnimateWindowOpenAsync()
        {
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

                progress += Time.deltaTime * m_WindowSpeed;

                await Awaitable.NextFrameAsync();
            }

            m_UIInstance.style.width  = m_Rect.width;
            m_UIInstance.style.height = m_Rect.height;
        }

        void IDialogueWindowUI.Destroy()
        {
            m_Ticker.Pause();
            m_UIInstance.RemoveFromHierarchy();
            m_UIInstance = null;
        }

        byte IDialogueWindowUI.GetSelectedChoice()
        {
            m_Choices[m_SelectedIndex].SendEvent(new NavigationSubmitEvent());

            return m_SelectedIndex;
        }

        void IDialogueWindowUI.Init(VisualElement container)
        {
            VisualTreeAsset uiAsset = m_DialogueWindowUIProvider.Get<IDialogueWindowUI>();

            uiAsset.CloneTree(container, out int index, out _);

            m_UIInstance               = container[index];
            m_UIInstance.style.display = DisplayStyle.None;

            m_Text      = m_UIInstance.Q<Label>(TEXT);
            m_Text.text = string.Empty;
            m_TextView  = new DialogueTextView(m_Text);

            ApplyStyle();

            // One tick for the window drives the typing and keeps blinking text flashing while a page waits.
            m_Ticker = m_UIInstance.schedule.Execute(Tick).Every(0);
        }

        async Task IDialogueWindowUI.RunAsync()
        {
            await TypeAsync(m_TextView);

            if (m_Choices.Length > 0)
            {
                for (int i = 0; i < m_Choices.Length; i++)
                {
                    // Each answer is typed after the one before it, so it is shown only when its turn comes.
                    m_Choices[i].SetEnabledAndVisible(true);
                    m_ChoiceViews[i].Show(string.Empty, Parse(m_ChoiceTexts[i]));
                    await TypeAsync(m_ChoiceViews[i]);
                }

                m_Choices[0].Focus();
                m_SelectedIndex = 0;
            }
        }

        void IDialogueWindowUI.SetChoices(ReadOnlySpan<string> choices)
        {
            int length = choices.Length;

            m_Choices     = new RPGUIButton[length];
            m_ChoiceViews = new DialogueTextView[length];
            m_ChoiceTexts = choices.ToArray();

            for (byte i = 0; i < length; i++)
            {
                RPGUIButton button = new RPGUIButton
                                     {
                                         text = string.Empty,
                                         style =
                                         {
                                             fontSize = m_Text.resolvedStyle.fontSize
                                         }
                                     };

                button.RegisterCallback<NavigationMoveEvent, byte>(OnChoiceChosenBtnNavigate, i);
                button.RegisterCallback<NavigationSubmitEvent, byte>(OnChoiceChosenBtnSubmitted, i);
                button.RegisterCallback<ClickEvent, byte>(OnChoiceChosenBtnClicked, i);
                button.SetEnabledAndVisible(false);

                m_UIInstance.Add(button);
                m_Choices[i]     = button;
                m_ChoiceViews[i] = new DialogueTextView(button.Label);
            }
        }

        void IDialogueWindowUI.SetMessageVariables(IReadOnlyList<int> variables)
        {
            m_MessageVariables = variables;
        }

        void IDialogueWindowUI.SetMessageSpeed(float messageSpeed)
        {
            m_TextSpeedMultiplier = math.pow(2f, 2f * messageSpeed - 1f);
        }

        void IDialogueWindowUI.SetRect(RectInt rect)
        {
            m_Rect = rect;
        }

        void IDialogueWindowUI.SetStyle(DialogueWindowStyle style)
        {
            m_Style = style;

            if (m_UIInstance != null)
            {
                ApplyStyle();
            }
        }

        void IDialogueWindowUI.SetText(DialoguePage dialoguePage)
        {
            // An explicit '\n' rather than a platform line ending, so the character positions the styling is
            // keyed to are the same everywhere.
            string speaker = string.IsNullOrWhiteSpace(dialoguePage.SpeakerId) ? string.Empty : dialoguePage.SpeakerId + "\n";

            m_TextView.Show(speaker, Parse(dialoguePage.Text));
            m_SkipRequested = false;
        }

        void IDialogueWindowUI.SkipToAnimationEnd()
        {
            m_SkipRequested = true;
        }

        private DialogueText Parse(string text)
        {
            DialogueText parsed = DialogueMarkup.Parse(text, m_MessageVariables, m_TextStyles, null);

            return parsed;
        }

        private void ApplyStyle()
        {
            DialogueWindowStyle[] styles = (DialogueWindowStyle[])Enum.GetValues(typeof(DialogueWindowStyle));

            for (int i = 0; i < styles.Length; i++)
            {
                DialogueWindowStyle style = styles[i];

                m_UIInstance.EnableInClassList(StyleClass(style), style == m_Style);
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

        private void OnChoiceChosenBtnNavigate(NavigationMoveEvent evt, byte index)
        {
            RPGUIButton root = m_Choices[index];
            RPGUIButton up   = m_Choices[(index - 1 + m_Choices.Length) % m_Choices.Length];
            RPGUIButton down = m_Choices[(index     + 1)                % m_Choices.Length];

            if (UIToolkitInputUtility.Navigate(evt, root, up, down))
            {
                m_AudioIntentPlayer.Play(AudioIntent.Navigate, AudioContext.Field);
            }
        }

        private void OnChoiceChosenBtnSubmitted(NavigationSubmitEvent evt, byte index)
        {
            OnChoiceChosen(index);
        }

        private void OnChoiceChosenBtnClicked(ClickEvent evt, byte index)
        {
            OnChoiceChosen(index);
        }

        private void OnChoiceChosen(byte index)
        {
            m_SelectedIndex = index;
            m_AudioIntentPlayer.Play(AudioIntent.Confirm, AudioContext.Field);
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