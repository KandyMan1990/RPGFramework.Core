using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Color32 = UnityEngine.Color32;
using Vertex = UnityEngine.UIElements.Vertex;

namespace RPGFramework.Core.DialogueWindow.UI
{
    public class DialogueWindowWithTextUI : IDialogueWindowWithTextUI
    {
        private readonly IDialogueWindowUiProvider m_DialogueWindowUiProvider;
        private readonly float                     m_TextSpeed;
        private readonly float                     m_WindowSpeed;

        private float         m_CurrentVisibleCharacters;
        private RectInt       m_Rect;
        private bool          m_SkipRequested;
        private Label         m_Text;
        private VisualElement m_UiInstance;

        public DialogueWindowWithTextUI(IDialogueWindowUiProvider uiProvider)
        {
            m_DialogueWindowUiProvider = uiProvider;
            m_TextSpeed                = uiProvider.GetTextSpeed;
            m_WindowSpeed              = uiProvider.GetWindowSpeed;
        }

        async Task IDialogueWindowUI.AnimateWindowClosedAsync()
        {
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

        void IDialogueWindowUI.Init(VisualElement container, ulong id)
        {
            VisualTreeAsset uiAsset = m_DialogueWindowUiProvider.Get<IDialogueWindowWithTextUI>();

            uiAsset.CloneTree(container, out int index, out _);

            m_UiInstance               = container[index];
            m_UiInstance.name          = $"DialogueWindowWithText_{id}";
            m_UiInstance.style.display = DisplayStyle.None;

            m_Text      = m_UiInstance.Q<Label>("Text");
            m_Text.text = string.Empty;
        }

        async Task IDialogueWindowUI.RunAsync()
        {
            int textLength = m_Text.text.Length;
            m_CurrentVisibleCharacters = 0;

            m_Text.PostProcessTextVertices += PostProcessTextVertices;

            while (m_CurrentVisibleCharacters <= textLength)
            {
                m_CurrentVisibleCharacters += Time.deltaTime * m_TextSpeed;
                m_Text.MarkDirtyRepaint();

                await Awaitable.NextFrameAsync();

                if (m_SkipRequested)
                {
                    m_CurrentVisibleCharacters = textLength;
                }
            }

            m_Text.PostProcessTextVertices -= PostProcessTextVertices;
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

            m_Text.text = sb.ToString();
        }

        void IDialogueWindowUI.SkipToAnimationEnd()
        {
            m_SkipRequested = true;
        }

        private void PostProcessTextVertices(TextElement.GlyphsEnumerable glyphs)
        {
            int visibleGlyphs = (int)math.floor(m_CurrentVisibleCharacters);

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