using UnityEngine.Rendering.Universal;

namespace RPGFramework.Core.Rendering
{
    public interface IRendererDataProvider
    {
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