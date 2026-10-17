using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>The panel the transitions draw on, and the transitions used when none is named.</summary>
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

        PanelSettings ITransitionConfig.  PanelSettings => m_PanelSettings;
        TransitionAsset ITransitionConfig.DefaultCover  => m_DefaultCover;
        TransitionAsset ITransitionConfig.DefaultReveal => m_DefaultReveal;
    }
}