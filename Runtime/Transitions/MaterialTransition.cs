using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// Any UI material over the screen, such as a UI Shader Graph's, told how covered the screen is, 0 to 1, in its
    /// <c>_Covered</c> float. One that captures the frame draws over a picture of the screen taken as it starts, its
    /// element's texture; otherwise its element is plain white.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Transitions/Material Transition", fileName = "MaterialTransition")]
    public sealed class MaterialTransition : TransitionAsset
    {
        private const string COVERED_PROPERTY = "_Covered";

        [SerializeField]
        [Tooltip("A UI material, its _Covered float set from 0, clear, to 1, covered")]
        private Material m_Material;

        [SerializeField]
        [Tooltip("Whether its element's texture is a picture of the screen taken as it starts")]
        private bool m_CapturesFrame;

        protected internal override bool CapturesFrame => m_CapturesFrame;

        protected internal override TransitionPlayback Begin(VisualElement host, Texture capturedFrame)
        {
            Image element = AddFill(host, new Image { image = capturedFrame, scaleMode = ScaleMode.StretchToFill });

            if (capturedFrame == null)
            {
                element.style.backgroundColor = Color.white;
            }

            TransitionPlayback playback = new MaterialPlayback(element, m_Material);

            return playback;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_Material == null)
            {
                Debug.LogError($"{nameof(MaterialTransition)} [{name}] has no material, so it covers nothing", this);
            }
        }
#endif

        private sealed class MaterialPlayback : TransitionPlayback
        {
            private readonly Image    m_Element;
            private readonly Material m_Material;

            internal MaterialPlayback(Image element, Material material)
            {
                m_Element  = element;
                m_Material = material;
            }

            protected internal override void Show(float covered)
            {
                MaterialDefinition material = new MaterialDefinition(m_Material);

                material.SetFloat(COVERED_PROPERTY, covered);

                m_Element.style.unityMaterial = material;
            }

            protected internal override void End()
            {
                m_Element.RemoveFromHierarchy();
            }
        }
    }
}