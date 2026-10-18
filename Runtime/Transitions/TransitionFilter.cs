using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// A UI Toolkit filter of one pass, made from one of the config's shaders for as long as something draws with it,
    /// and destroyed after. The pass's settings come from its callback, which sees the filter's parameters as they are.
    /// </summary>
    internal sealed class TransitionFilter
    {
        private static readonly int m_ReadsGammaId  = Shader.PropertyToID("_ReadsGamma");
        private static readonly int m_WritesGammaId = Shader.PropertyToID("_WritesGamma");

        private readonly FilterFunctionDefinition m_Definition;
        private readonly Material                 m_Material;

        internal TransitionFilter(string name, Shader shader, FilterParameterDeclaration[] parameters,
                                  PostProcessingPass.ApplyFilterPassSettingsDelegate applySettings)
        {
            m_Material   = new Material(shader);
            m_Definition = ScriptableObject.CreateInstance<FilterFunctionDefinition>();

            m_Definition.filterName = name;
            m_Definition.parameters = parameters;
            m_Definition.passes     = new[] { new PostProcessingPass { material = m_Material, applySettingsCallback = applySettings } };
        }

        internal FilterFunctionDefinition Definition => m_Definition;

        /// <summary>
        /// Tells the shader whether its source reads, and what it writes must be, in gamma space rather than linear.
        /// </summary>
        internal static void SetColourSpace(MaterialPropertyBlock properties, FilterPassContext context)
        {
            properties.SetFloat(m_ReadsGammaId,  context.readsGamma ? 1f : 0f);
            properties.SetFloat(m_WritesGammaId, context.writesGamma ? 1f : 0f);
        }

        internal void Destroy()
        {
            Object.Destroy(m_Definition);
            Object.Destroy(m_Material);
        }
    }
}