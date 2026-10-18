using UnityEngine;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// The screen fading to or from black by subtraction: an amount taken from each channel of everything beneath the
    /// overlay, live, and clamped at zero, so dark colours reach black first and bright, saturated ones hold longest.
    /// Its colour is what is taken once covered; white takes everything. Drawn as a <see cref="ScreenColour" />, which
    /// only the Universal Render Pipeline draws: a game on another fades with a <see cref="ColourTransition" />.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Transitions/Subtractive Transition", fileName = "SubtractiveTransition")]
    public sealed class SubtractiveTransition : TransitionAsset
    {
        [SerializeField]
        [Tooltip("What is taken from each channel once covered: white takes everything, to black")]
        private Color m_Colour = Color.white;

        protected internal override TransitionPlayback Begin(TransitionContext context)
        {
            ScreenColour       screen   = new ScreenColour(context.Host, context.ScreenColourShader);
            TransitionPlayback playback = new SubtractivePlayback(screen, m_Colour);

            return playback;
        }

        private sealed class SubtractivePlayback : TransitionPlayback
        {
            private readonly ScreenColour m_Screen;
            private readonly Color        m_Colour;

            internal SubtractivePlayback(ScreenColour screen, Color colour)
            {
                m_Screen = screen;
                m_Colour = colour;
            }

            protected internal override void Show(float covered)
            {
                m_Screen.Subtract(m_Colour * covered);
            }

            protected internal override void End()
            {
                m_Screen.Remove();
            }
        }
    }
}