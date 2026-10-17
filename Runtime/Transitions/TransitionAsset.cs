using RPGFramework.Hashing;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// How the screen is covered or uncovered — a colour fading, a wipe, or a game's own. The
    /// <see cref="ITransitionService" /> plays it over its seconds, through its curve; the asset keeps no state, each
    /// play drawing through a <see cref="TransitionPlayback" /> it begins. Made to be extended: a game's own
    /// transition derives from it.
    /// </summary>
    public abstract class TransitionAsset : ScriptableObject
    {
        [SerializeField]
        [Min(0f)]
        [Tooltip("Seconds the whole transition takes, from clear to covered or back")]
        private float m_Seconds = 0.5f;

        [SerializeField]
        [Tooltip("How far through the transition it is, 0 to 1, over its time, 0 to 1")]
        private AnimationCurve m_Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        /// <summary>What names it to a change, a script or a module's data: the hash of its asset's name.</summary>
        public ulong NameHash => Fnv1a64.Hash(name);

        internal float Seconds => m_Seconds;

        internal float Evaluate(float time)
        {
            float progress = m_Curve.Evaluate(time);

            return progress;
        }

        /// <summary>
        /// Whether it draws over a picture of the screen taken as it starts rather than over the live screen. A cover
        /// that does owns the whole screen, black beneath it, so what it covers can change at once.
        /// </summary>
        protected internal virtual bool CapturesFrame => false;

        /// <summary>
        /// Builds what this transition draws in <paramref name="host" />, which fills the screen: over
        /// <paramref name="capturedFrame" />, upright, when it captures one, and null otherwise.
        /// </summary>
        protected internal abstract TransitionPlayback Begin(VisualElement host, Texture capturedFrame);
    }

    /// <summary>One play of a <see cref="TransitionAsset" />: what it drew, shown as covered as the screen.</summary>
    public abstract class TransitionPlayback
    {
        /// <summary>Shows the screen <paramref name="covered" />: 0 clear, 1 covered entirely.</summary>
        protected internal abstract void Show(float covered);

        /// <summary>Removes what it drew.</summary>
        protected internal abstract void End();
    }
}