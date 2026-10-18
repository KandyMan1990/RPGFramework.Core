using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// The panel the transitions draw on, the transitions used when none is named, and the shaders Core's own draw with.
    /// </summary>
    public interface ITransitionConfig
    {
        /// <summary>
        /// The panel the overlay draws on, which must sort above every panel the game's modules use, so it covers their
        /// UI. Core ships one, <c>TransitionPanelSettings</c>.
        /// </summary>
        PanelSettings PanelSettings { get; }

        /// <summary>The transition that covers the screen when none is named.</summary>
        TransitionAsset DefaultCover { get; }

        /// <summary>The transition that reveals the screen when none is named.</summary>
        TransitionAsset DefaultReveal { get; }

        /// <summary>
        /// What a colour taken from or added to the screen draws with: <see cref="ScreenColour" />, the subtractive
        /// fade and a field's colour fades. Core ships one, <c>ScreenColour</c>.
        /// </summary>
        Shader ScreenColourShader { get; }

        /// <summary>What a mask wipe draws with. Core ships one, <c>MaskWipe</c>.</summary>
        Shader MaskWipeShader { get; }
    }

    [CreateAssetMenu(menuName = "RPG Framework/Transitions/Transition Config", fileName = "TransitionConfig")]
    public sealed class TransitionConfig : ScriptableObject, ITransitionConfig
    {
        [SerializeField]
        [Tooltip("The overlay's panel, sorted above every module's. Core ships one: TransitionPanelSettings")]
        private PanelSettings m_PanelSettings;

        [SerializeField]
        private TransitionAsset m_DefaultCover;

        [SerializeField]
        private TransitionAsset m_DefaultReveal;

        [SerializeField]
        [Tooltip("What colours taken from or added to the screen draw with. Core's, ScreenColour, is set as a config is made")]
        private Shader m_ScreenColourShader;

        [SerializeField]
        [Tooltip("What mask wipes draw with. Core's, MaskWipe, is set as a config is made")]
        private Shader m_MaskWipeShader;

        PanelSettings ITransitionConfig.  PanelSettings      => m_PanelSettings;
        TransitionAsset ITransitionConfig.DefaultCover       => m_DefaultCover;
        TransitionAsset ITransitionConfig.DefaultReveal      => m_DefaultReveal;
        Shader ITransitionConfig.         ScreenColourShader => m_ScreenColourShader;
        Shader ITransitionConfig.         MaskWipeShader     => m_MaskWipeShader;

#if UNITY_EDITOR
        private void Reset()
        {
            m_ScreenColourShader = Shader.Find("Hidden/RPG Framework/Screen Colour");
            m_MaskWipeShader     = Shader.Find("Hidden/RPG Framework/Mask Wipe");
        }

        private void OnValidate()
        {
            if (m_ScreenColourShader == null || m_MaskWipeShader == null)
            {
                Debug.LogError($"{nameof(TransitionConfig)} [{name}] is missing a shader; Core's are ScreenColour and MaskWipe, and Reset sets them", this);
            }
        }
#endif
    }
}