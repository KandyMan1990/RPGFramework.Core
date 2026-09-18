using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Dialogue.UI
{
    /// <summary>
    /// Types a <see cref="DialogueText" /> into a <see cref="Label" />: reveals it a character at a time, holds at
    /// its pauses, and colours and flashes its styled spans.<br /><br />
    /// The label is given the plain text only — rich text is off — and everything else is applied per glyph by
    /// character position as the text is drawn, so markup can never be shown by mistake and a stray <c>&lt;</c>
    /// in a line is just a character.<br /><br />
    /// Driven by <see cref="Tick" /> rather than by frames of its own, so the editor's preview can run it too.
    /// </summary>
    public sealed class DialogueTextView
    {
        private const float BLINK_SECONDS = 0.8f;
        private const float BLINK_FLOOR   = 0.2f;

        private readonly Label m_Label;

        private DialogueText m_Text;
        private int          m_Offset;
        private int          m_Length;
        private float        m_Revealed;
        private int          m_NextPause;
        private float        m_PauseRemaining;
        private float        m_Time;
        private bool         m_Attached;

        public DialogueTextView(Label label)
        {
            m_Label                = label;
            m_Label.enableRichText = false;
        }

        public bool IsTyping => m_Text != null && m_Revealed < m_Length;

        /// <summary>
        /// Show a page, typed from the start. <paramref name="prefix" /> is shown in full and unstyled before it,
        /// such as the speaker's name on its own line.
        /// </summary>
        public void Show(string prefix, DialogueText text)
        {
            m_Text           = text;
            m_Offset         = prefix.Length;
            m_Length         = prefix.Length + text.Text.Length;
            m_Revealed       = prefix.Length;
            m_NextPause      = 0;
            m_PauseRemaining = 0f;
            m_Time           = 0f;
            m_Label.text     = prefix + text.Text;

            if (!m_Attached)
            {
                m_Label.PostProcessTextVertices += PostProcessTextVertices;
                m_Attached                      =  true;
            }

            m_Label.MarkDirtyRepaint();
        }

        public void Clear()
        {
            if (m_Attached)
            {
                m_Label.PostProcessTextVertices -= PostProcessTextVertices;
                m_Attached                      =  false;
            }

            m_Text       = null;
            m_Label.text = string.Empty;
        }

        /// <summary>
        /// Advance the typing and the blinking. Keep calling after typing ends while blinking text is showing.
        /// </summary>
        public void Tick(float deltaTime, float charactersPerSecond)
        {
            if (m_Text == null)
            {
                return;
            }

            m_Time += deltaTime;

            if (IsTyping)
            {
                Type(deltaTime * charactersPerSecond, deltaTime);
            }

            m_Label.MarkDirtyRepaint();
        }

        /// <summary>
        /// Show the rest at once, skipping any pauses left.
        /// </summary>
        public void SkipToEnd()
        {
            if (m_Text == null)
            {
                return;
            }

            m_Revealed       = m_Length;
            m_NextPause      = m_Text.Pauses.Count;
            m_PauseRemaining = 0f;

            m_Label.MarkDirtyRepaint();
        }

        private void Type(float characters, float deltaTime)
        {
            if (m_PauseRemaining > 0f)
            {
                m_PauseRemaining -= deltaTime;
                return;
            }

            float next = m_Revealed + characters;

            if (m_NextPause < m_Text.Pauses.Count)
            {
                DialoguePause pause   = m_Text.Pauses[m_NextPause];
                int           pauseAt = m_Offset + pause.Index;

                if (next >= pauseAt)
                {
                    next             = pauseAt;
                    m_PauseRemaining = pause.Seconds;
                    m_NextPause++;
                }
            }

            m_Revealed = Mathf.Min(next, m_Length);
        }

        private void PostProcessTextVertices(TextElement.GlyphsEnumerable glyphs)
        {
            if (m_Text == null)
            {
                return;
            }

            Color shadow = m_Label.resolvedStyle.textShadow.color;
            float blink  = BLINK_FLOOR + (1f - BLINK_FLOOR) * (0.5f + 0.5f * Mathf.Cos(m_Time * 2f * Mathf.PI / BLINK_SECONDS));

            foreach (TextElement.Glyph glyph in glyphs)
            {
                int index = glyph.textRange.start;

                if (index >= m_Revealed)
                {
                    Tint(glyph, null, 0f);
                    glyph.SetTints(Color.clear, Color.clear);
                    continue;
                }

                int  textIndex = index - m_Offset;
                bool styled    = textIndex >= 0;

                Color? colour = styled && m_Text.TryGetColour(textIndex, out Color spanColour) ? spanColour : (Color?)null;
                float  alpha  = styled && m_Text.IsBlinking(textIndex) ? blink : 1f;

                if (colour == null && alpha >= 1f)
                {
                    continue;
                }

                Tint(glyph, colour, alpha);

                if (alpha < 1f)
                {
                    glyph.SetTints(null, new Color(shadow.r, shadow.g, shadow.b, shadow.a * alpha));
                }
            }
        }

        private static void Tint(TextElement.Glyph glyph, Color? colour, float alpha)
        {
            NativeSlice<Vertex> vertices = glyph.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vertex  vertex = vertices[i];
                Color32 tint   = colour.HasValue ? (Color32)colour.Value : vertex.tint;

                tint.a      = (byte)(tint.a * alpha);
                vertex.tint = tint;
                vertices[i] = vertex;
            }
        }
    }
}
