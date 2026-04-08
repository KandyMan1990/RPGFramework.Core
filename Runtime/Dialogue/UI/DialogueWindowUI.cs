using System;
using System.Text;
using System.Threading.Tasks;
using RPGFramework.Core.Audio;
using RPGFramework.Core.Input;
using RPGFramework.Core.UI;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue.UI
{
    internal sealed class DialogueWindowUI : IDialogueWindowUI
    {
        private const string TEXT = "Text";

        private readonly IDialogueWindowUiProvider m_DialogueWindowUiProvider;
        private readonly IAudioIntentPlayer        m_AudioIntentPlayer;
        private readonly float                     m_TextSpeed;
        private readonly float                     m_WindowSpeed;

        private RectInt       m_Rect;
        private bool          m_SkipRequested;
        private Label         m_Text;
        private VisualElement m_UiInstance;
        private RPGUIButton[] m_Choices;
        private byte          m_SelectedIndex;
        private float         m_CurrentTextLength;

        public DialogueWindowUI(IDialogueWindowUiProvider uiProvider, IAudioIntentPlayer audioIntentPlayer)
        {
            m_DialogueWindowUiProvider = uiProvider;
            m_AudioIntentPlayer        = audioIntentPlayer;
            m_TextSpeed                = uiProvider.GetTextSpeed;
            m_WindowSpeed              = uiProvider.GetWindowSpeed;
            m_Choices                  = Array.Empty<RPGUIButton>();
        }

        async Task IDialogueWindowUI.AnimateWindowClosedAsync()
        {
            for (byte i = 0; i < m_Choices.Length; i++)
            {
                m_Choices[i].UnregisterCallback<ClickEvent, byte>(OnChoiceChosenBtnClicked);
                m_Choices[i].UnregisterCallback<NavigationSubmitEvent, byte>(OnChoiceChosenBtnSubmitted);
                m_Choices[i].UnregisterCallback<NavigationMoveEvent, byte>(OnChoiceChosenBtnNavigate);

                m_Choices[i].RemoveFromHierarchy();
                m_Choices[i] = null;
            }

            m_Text.text = string.Empty;

            float targetX  = m_Rect.x + (m_Rect.width  / 2f);
            float targetY  = m_Rect.y + (m_Rect.height / 2f);
            float progress = 0f;

            while (progress < 1f)
            {
                float x      = math.lerp(m_Rect.x,      targetX, progress);
                float y      = math.lerp(m_Rect.y,      targetY, progress);
                float width  = math.lerp(m_Rect.width,  0f,      progress);
                float height = math.lerp(m_Rect.height, 0f,      progress);

                m_UiInstance.style.left   = x;
                m_UiInstance.style.top    = y;
                m_UiInstance.style.width  = width;
                m_UiInstance.style.height = height;

                progress += Time.deltaTime * m_WindowSpeed;

                await Awaitable.NextFrameAsync();
            }

            m_UiInstance.style.width  = 0f;
            m_UiInstance.style.height = 0f;
        }

        async Task IDialogueWindowUI.AnimateWindowOpenAsync()
        {
            m_UiInstance.style.left   = m_Rect.x;
            m_UiInstance.style.top    = m_Rect.y;
            m_UiInstance.style.width  = 0f;
            m_UiInstance.style.height = 0f;

            m_UiInstance.style.display = DisplayStyle.Flex;

            float progress = 0f;

            while (progress < 1f)
            {
                float x = math.lerp(0f, m_Rect.width,  progress);
                float y = math.lerp(0f, m_Rect.height, progress);

                m_UiInstance.style.width  = x;
                m_UiInstance.style.height = y;

                progress += Time.deltaTime * m_WindowSpeed;

                await Awaitable.NextFrameAsync();
            }

            m_UiInstance.style.width  = m_Rect.width;
            m_UiInstance.style.height = m_Rect.height;
        }

        void IDialogueWindowUI.Destroy()
        {
            m_UiInstance.RemoveFromHierarchy();
            m_UiInstance = null;
        }

        byte IDialogueWindowUI.GetSelectedChoice()
        {
            m_Choices[m_SelectedIndex].SendEvent(new NavigationSubmitEvent());

            return m_SelectedIndex;
        }

        void IDialogueWindowUI.Init(VisualElement container)
        {
            VisualTreeAsset uiAsset = m_DialogueWindowUiProvider.Get<IDialogueWindowUI>();

            uiAsset.CloneTree(container, out int index, out _);

            m_UiInstance               = container[index];
            m_UiInstance.style.display = DisplayStyle.None;

            m_Text      = m_UiInstance.Q<Label>(TEXT);
            m_Text.text = string.Empty;
        }

        async Task IDialogueWindowUI.RunAsync()
        {
            await RunAsync(m_Text);

            if (m_Choices.Length > 0)
            {
                for (int i = 0; i < m_Choices.Length; i++)
                {
                    m_Choices[i].SetEnabledAndVisible(true);
                    await RunAsync(m_Choices[i].Label);
                }

                m_Choices[0].Focus();
                m_SelectedIndex = 0;
            }
        }

        void IDialogueWindowUI.SetChoices(ReadOnlySpan<string> choices)
        {
            int length = choices.Length;

            m_Choices = new RPGUIButton[length];

            for (byte i = 0; i < length; i++)
            {
                RPGUIButton button = new RPGUIButton
                                     {
                                             text = choices[i],
                                             style =
                                             {
                                                     fontSize = m_Text.resolvedStyle.fontSize
                                             }
                                     };

                button.RegisterCallback<NavigationMoveEvent, byte>(OnChoiceChosenBtnNavigate, i);
                button.RegisterCallback<NavigationSubmitEvent, byte>(OnChoiceChosenBtnSubmitted, i);
                button.RegisterCallback<ClickEvent, byte>(OnChoiceChosenBtnClicked, i);
                button.SetEnabledAndVisible(false);

                m_UiInstance.Add(button);
                m_Choices[i] = button;
            }
        }

        void IDialogueWindowUI.SetRect(RectInt rect)
        {
            m_Rect = rect;
        }

        void IDialogueWindowUI.SetText(DialoguePage dialoguePage)
        {
            int           capacity = dialoguePage.SpeakerId.Length + dialoguePage.Text.Length + 1;
            StringBuilder sb       = new StringBuilder(capacity);

            if (!string.IsNullOrWhiteSpace(dialoguePage.SpeakerId))
            {
                sb.AppendLine(dialoguePage.SpeakerId);
            }
            sb.AppendLine(dialoguePage.Text);

            m_Text.text     = sb.ToString();
            m_SkipRequested = false;
        }

        void IDialogueWindowUI.SkipToAnimationEnd()
        {
            m_SkipRequested = true;
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

        private async Task RunAsync(Label label)
        {
            int textLength = label.text.Length;
            m_CurrentTextLength = 0;

            label.PostProcessTextVertices += PostProcessTextVertices;

            while (m_CurrentTextLength <= textLength)
            {
                if (m_SkipRequested)
                {
                    m_CurrentTextLength = textLength;
                }

                m_CurrentTextLength += Time.deltaTime * m_TextSpeed;
                label.MarkDirtyRepaint();
                await Awaitable.NextFrameAsync();
            }

            label.PostProcessTextVertices -= PostProcessTextVertices;
        }

        private void PostProcessTextVertices(TextElement.GlyphsEnumerable glyphs)
        {
            int visibleGlyphs = (int)math.floor(m_CurrentTextLength);

            int index = 0;
            foreach (TextElement.Glyph glyph in glyphs)
            {
                NativeSlice<Vertex> verts = glyph.vertices;

                for (int i = 0; i < verts.Length; i++)
                {
                    Vertex  v    = verts[i];
                    Color32 tint = v.tint;

                    tint.a = index < visibleGlyphs ? (byte)255 : (byte)0;

                    v.tint   = tint;
                    verts[i] = v;
                }

                index++;
            }
        }
    }
}