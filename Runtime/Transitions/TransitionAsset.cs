using RPGFramework.Core.Audio;
using RPGFramework.Hashing;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// How the screen is covered or uncovered — a colour fading, a wipe, or a game's own. The
    /// <see cref="ITransitionService" /> plays it over its seconds, through its curve, playing its sound as it starts;
    /// the asset keeps no state, each play drawing through a <see cref="TransitionPlayback" /> it begins. Made to be
    /// extended: a game's own transition derives from it.
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

        [SerializeField]
        [SoundName]
        [Tooltip("Played as it starts, from the game's sound effects")]
        private ulong m_Sound;

        /// <summary>What names it to a change, a script or a module's data: the hash of its asset's name.</summary>
        public ulong NameHash => Fnv1a64.Hash(name);

        /// <summary>How long it takes, from clear to covered or back, for a transition timing its own motion.</summary>
        protected internal float Seconds => m_Seconds;

        /// <summary>The name hash of the sound played as it starts; 0 for none.</summary>
        internal ulong Sound => m_Sound;

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
        /// Builds what this transition draws in the context's host, over its captured frame when it captures one.
        /// </summary>
        protected internal abstract TransitionPlayback Begin(TransitionContext context);

        /// <summary>Adds an element filling <paramref name="host" />, which the pointer passes through.</summary>
        internal static T AddFill<T>(VisualElement host, T element) where T : VisualElement
        {
            element.pickingMode    = PickingMode.Ignore;
            element.style.position = Position.Absolute;
            element.style.left     = 0f;
            element.style.top      = 0f;
            element.style.right    = 0f;
            element.style.bottom   = 0f;

            host.Add(element);

            return element;
        }
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