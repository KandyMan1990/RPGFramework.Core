using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// The screen wiped to or from a colour through a greyscale texture stretched over it: a cover reaches its darkest
    /// parts first and white last, and a reveal through the same texture uncovers white first, the wipe reversed.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Transitions/Mask Wipe Transition", fileName = "MaskWipeTransition")]
    public sealed class MaskWipeTransition : TransitionAsset
    {
        private static readonly int m_ColourId   = Shader.PropertyToID("_Colour");
        private static readonly int m_CoveredId  = Shader.PropertyToID("_Covered");
        private static readonly int m_SoftnessId = Shader.PropertyToID("_Softness");

        [SerializeField]
        [Tooltip("Stretched over the screen, read as grey: black is covered first and uncovered last")]
        private Texture2D m_Mask;

        [SerializeField]
        private Color m_Colour = Color.black;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("How much of the mask's range the edge blends over: 0 is a hard edge")]
        private float m_Softness;

        [SerializeField]
        [HideInInspector]
        private Shader m_Shader;

        protected internal override TransitionPlayback Begin(VisualElement host, Texture capturedFrame)
        {
            FilterParameterDeclaration[] parameters =
            {
                new FilterParameterDeclaration { name = "covered" },
                new FilterParameterDeclaration { name = "softness" },
                new FilterParameterDeclaration { name = "colour" }
            };

            TransitionFilter wipe = new TransitionFilter("mask-wipe", m_Shader, parameters, ApplySettings);
            VisualElement    mask = AddFill(host, new VisualElement());

            mask.style.backgroundImage = new StyleBackground(m_Mask);
            mask.style.backgroundSize  = new BackgroundSize(Length.Percent(100f), Length.Percent(100f));

            TransitionPlayback playback = new MaskWipePlayback(mask, wipe, m_Softness, m_Colour);

            return playback;
        }

        private static void ApplySettings(MaterialPropertyBlock properties, FilterPassContext context)
        {
            FilterFunction wipe = context.filterFunction;

            properties.SetFloat(m_CoveredId, wipe.GetParameter(0).floatValue);
            properties.SetFloat(m_SoftnessId, wipe.GetParameter(1).floatValue);
            properties.SetVector(m_ColourId, wipe.GetParameter(2).colorValue);

            TransitionFilter.SetColourSpace(properties, context);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            m_Shader = Shader.Find("Hidden/RPG Framework/Mask Wipe Transition");
        }

        private void OnValidate()
        {
            if (m_Mask == null)
            {
                Debug.LogError($"{nameof(MaskWipeTransition)} [{name}] has no mask, so it covers nothing", this);
            }
        }
#endif

        private sealed class MaskWipePlayback : TransitionPlayback
        {
            private readonly VisualElement    m_Mask;
            private readonly TransitionFilter m_Wipe;
            private readonly float            m_Softness;
            private readonly Color            m_Colour;

            internal MaskWipePlayback(VisualElement mask, TransitionFilter wipe, float softness, Color colour)
            {
                m_Mask     = mask;
                m_Wipe     = wipe;
                m_Softness = softness;
                m_Colour   = colour;
            }

            protected internal override void Show(float covered)
            {
                FilterFunction wipe = new FilterFunction(m_Wipe.Definition);

                wipe.AddParameter(new FilterParameter(covered));
                wipe.AddParameter(new FilterParameter(m_Softness));
                wipe.AddParameter(new FilterParameter(m_Colour));

                m_Mask.style.filter = new List<FilterFunction> { wipe };
            }

            protected internal override void End()
            {
                m_Mask.RemoveFromHierarchy();
                m_Wipe.Destroy();
            }
        }
    }
}