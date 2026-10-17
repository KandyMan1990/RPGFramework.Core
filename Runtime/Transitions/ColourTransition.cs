using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>The screen fading to or from a colour.</summary>
    [CreateAssetMenu(menuName = "RPG Framework/Transitions/Colour Transition", fileName = "ColourTransition")]
    public sealed class ColourTransition : TransitionAsset
    {
        [SerializeField]
        private Color m_Colour = Color.black;

        protected internal override TransitionPlayback Begin(VisualElement host)
        {
            VisualElement colour = new VisualElement { pickingMode = PickingMode.Ignore };

            colour.style.position        = Position.Absolute;
            colour.style.left            = 0f;
            colour.style.top             = 0f;
            colour.style.right           = 0f;
            colour.style.bottom          = 0f;
            colour.style.backgroundColor = m_Colour;
            colour.style.opacity         = 0f;

            host.Add(colour);

            TransitionPlayback playback = new ColourPlayback(colour);

            return playback;
        }

        private sealed class ColourPlayback : TransitionPlayback
        {
            private readonly VisualElement m_Colour;

            internal ColourPlayback(VisualElement colour)
            {
                m_Colour = colour;
            }

            protected internal override void Show(float covered)
            {
                m_Colour.style.opacity = covered;
            }

            protected internal override void End()
            {
                m_Colour.RemoveFromHierarchy();
            }
        }
    }
}