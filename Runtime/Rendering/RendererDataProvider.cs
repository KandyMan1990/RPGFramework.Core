using UnityEngine.Rendering.Universal;

namespace RPGFramework.Core.Rendering
{
    /// <summary>The game's URP renderer data, which the screen fade adds its full-screen pass to.</summary>
    public interface IRendererDataProvider
    {
        /// <summary>The renderer data the game renders with.</summary>
        UniversalRendererData Get();
    }

    public class RendererDataProvider : IRendererDataProvider
    {
        private readonly UniversalRendererData m_UniversalRendererData;

        UniversalRendererData IRendererDataProvider.Get() => m_UniversalRendererData;

        public RendererDataProvider(UniversalRendererData universalRendererData)
        {
            m_UniversalRendererData = universalRendererData;
        }
    }
}