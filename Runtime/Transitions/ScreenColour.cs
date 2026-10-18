using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// A colour taken from or added to each channel of everything beneath an element filling its host, live, and
    /// clamped, so colours sink into black or wash out to white rather than a colour being laid over them. Drawn by a UI
    /// Toolkit backdrop filter, which only the Universal Render Pipeline draws.
    /// </summary>
    public sealed class ScreenColour
    {
        private static readonly int m_SubtractId = Shader.PropertyToID("_Subtract");
        private static readonly int m_AddId      = Shader.PropertyToID("_Add");

        private readonly VisualElement    m_Element;
        private readonly TransitionFilter m_Filter;

        /// <summary>
        /// Adds the element to <paramref name="host" />, over what it already holds, changing nothing yet. It draws with
        /// <paramref name="shader" />, the transition config's <see cref="ITransitionConfig.ScreenColourShader" />.
        /// </summary>
        public ScreenColour(VisualElement host, Shader shader)
        {
            FilterParameterDeclaration[] parameters =
            {
                new FilterParameterDeclaration { name = "subtract" },
                new FilterParameterDeclaration { name = "add" }
            };

            m_Filter  = new TransitionFilter("screen-colour", shader, parameters, ApplySettings);
            m_Element = TransitionAsset.AddFill(host, new VisualElement());
        }

        /// <summary>Takes the colour from each channel beneath, in place of whatever it changed before.</summary>
        public void Subtract(Color colour)
        {
            Show(colour, Color.clear);
        }

        /// <summary>Adds the colour to each channel beneath, in place of whatever it changed before.</summary>
        public void Add(Color colour)
        {
            Show(Color.clear, colour);
        }

        /// <summary>Removes the element, and the filter it drew with.</summary>
        public void Remove()
        {
            m_Element.RemoveFromHierarchy();
            m_Filter.Destroy();
        }

        // A filter's parameters are part of the style, so a new list is set for each change.
        private void Show(Color subtract, Color add)
        {
            FilterFunction colour = new FilterFunction(m_Filter.Definition);

            colour.AddParameter(new FilterParameter(subtract));
            colour.AddParameter(new FilterParameter(add));

            m_Element.style.backdropFilter = new List<FilterFunction> { colour };
        }

        private static void ApplySettings(MaterialPropertyBlock properties, FilterPassContext context)
        {
            properties.SetVector(m_SubtractId, context.filterFunction.GetParameter(0).colorValue);
            properties.SetVector(m_AddId, context.filterFunction.GetParameter(1).colorValue);

            TransitionFilter.SetColourSpace(properties, context);
        }
    }
}