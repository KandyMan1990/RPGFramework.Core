using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>What a transition plays into, handed to it as it begins.</summary>
    public sealed class TransitionContext
    {
        private readonly ITransitionConfig m_Config;

        internal TransitionContext(VisualElement host, Texture capturedFrame, ITransitionConfig config)
        {
            Host          = host;
            CapturedFrame = capturedFrame;
            m_Config      = config;
        }

        /// <summary>The element a transition draws in, which fills the screen or the module's layer.</summary>
        public VisualElement Host { get; }

        /// <summary>A picture of the screen taken as it started, upright, for a transition that captures one; null otherwise.</summary>
        public Texture CapturedFrame { get; }

        internal Shader ScreenColourShader => m_Config.ScreenColourShader;

        internal Shader MaskWipeShader => m_Config.MaskWipeShader;

        /// <summary>The same context with no captured frame, for a part of a transition that does not capture one.</summary>
        internal TransitionContext WithoutFrame()
        {
            TransitionContext context = new TransitionContext(Host, null, m_Config);

            return context;
        }
    }
}