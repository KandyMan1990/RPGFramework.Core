using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// The screen fading to or from black by subtraction: an amount taken from each channel of everything beneath the
    /// overlay, live, and clamped at zero, so dark colours reach black first and bright, saturated ones hold longest.
    /// Its colour is what is taken once covered; white takes everything. Drawn by a UI Toolkit backdrop filter, which
    /// only the Universal Render Pipeline draws: a game on another fades with a <see cref="ColourTransition" />.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Transitions/Subtractive Transition", fileName = "SubtractiveTransition")]
    public sealed class SubtractiveTransition : TransitionAsset
    {
        private static readonly int m_SubtractId = Shader.PropertyToID("_Subtract");

        [SerializeField]
        [Tooltip("What is taken from each channel once covered: white takes everything, to black")]
        private Color m_Colour = Color.white;

        [SerializeField]
        [HideInInspector]
        private Shader m_Shader;

        protected internal override TransitionPlayback Begin(VisualElement host, Texture capturedFrame)
        {
            FilterParameterDeclaration[] parameters = { new FilterParameterDeclaration { name = "colour" } };
            TransitionFilter             subtract   = new TransitionFilter("subtract", m_Shader, parameters, ApplySettings);
            VisualElement                backdrop   = AddFill(host, new VisualElement());

            TransitionPlayback playback = new SubtractivePlayback(backdrop, subtract, m_Colour);

            return playback;
        }

        private static void ApplySettings(MaterialPropertyBlock properties, FilterPassContext context)
        {
            properties.SetVector(m_SubtractId, context.filterFunction.GetParameter(0).colorValue);

            TransitionFilter.SetColourSpace(properties, context);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            m_Shader = Shader.Find("Hidden/RPG Framework/Subtractive Transition");
        }
#endif

        private sealed class SubtractivePlayback : TransitionPlayback
        {
            private readonly VisualElement    m_Backdrop;
            private readonly TransitionFilter m_Subtract;
            private readonly Color            m_Colour;

            internal SubtractivePlayback(VisualElement backdrop, TransitionFilter subtract, Color colour)
            {
                m_Backdrop = backdrop;
                m_Subtract = subtract;
                m_Colour   = colour;
            }

            protected internal override void Show(float covered)
            {
                FilterFunction subtract = new FilterFunction(m_Subtract.Definition);

                subtract.AddParameter(new FilterParameter(m_Colour * covered));

                m_Backdrop.style.backdropFilter = new List<FilterFunction> { subtract };
            }

            protected internal override void End()
            {
                m_Backdrop.RemoveFromHierarchy();
                m_Subtract.Destroy();
            }
        }
    }
}