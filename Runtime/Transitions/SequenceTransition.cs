using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace RPGFramework.Core.Transitions
{
    /// <summary>
    /// Several transitions played one after another — a flash, then a swirl — each through its own curve, in a share of
    /// this one's seconds in proportion to its own. A stage stays as it finished beneath those after it, and a reveal
    /// plays the stages in reverse. It captures the frame if any stage does, and hands it to those that do. Its own sound
    /// plays as it starts; its stages' do not.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Transitions/Sequence Transition", fileName = "SequenceTransition")]
    public sealed class SequenceTransition : TransitionAsset
    {
        [SerializeField]
        [Tooltip("Played first to last as it covers, each drawn over those before it")]
        private TransitionAsset[] m_Stages;

        protected internal override bool CapturesFrame
        {
            get
            {
                bool captures = false;

                for (int i = 0; i < m_Stages.Length; i++)
                {
                    captures |= m_Stages[i].CapturesFrame;
                }

                return captures;
            }
        }

        protected internal override TransitionPlayback Begin(TransitionContext context)
        {
            TransitionPlayback[] playbacks = new TransitionPlayback[m_Stages.Length];
            float[]              ends      = new float[m_Stages.Length];
            float                seconds   = 0f;
            float                reached   = 0f;

            for (int i = 0; i < m_Stages.Length; i++)
            {
                seconds += m_Stages[i].Seconds;
            }

            for (int i = 0; i < m_Stages.Length; i++)
            {
                TransitionAsset stage = m_Stages[i];

                reached += stage.Seconds;

                ends[i]      = seconds > 0f ? reached / seconds : (i + 1f) / m_Stages.Length;
                playbacks[i] = stage.Begin(stage.CapturesFrame ? context : context.WithoutFrame());
            }

            TransitionPlayback playback = new SequencePlayback(m_Stages, playbacks, ends);

            return playback;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_Stages == null || m_Stages.Length == 0)
            {
                Debug.LogError($"{nameof(SequenceTransition)} [{name}] has no stages, so it covers nothing", this);

                return;
            }

            for (int i = 0; i < m_Stages.Length; i++)
            {
                if (m_Stages[i] == null)
                {
                    Debug.LogError($"{nameof(SequenceTransition)} [{name}] has no transition at stage {i}", this);
                }
            }

            if (PlaysItself())
            {
                Debug.LogError($"{nameof(SequenceTransition)} [{name}] plays itself, directly or through another sequence, which would never end", this);
            }
        }

        private bool PlaysItself()
        {
            HashSet<SequenceTransition> seen  = new HashSet<SequenceTransition>();
            Stack<SequenceTransition>   ahead = new Stack<SequenceTransition>();
            bool                        plays = false;

            ahead.Push(this);

            while (ahead.Count > 0 && !plays)
            {
                TransitionAsset[] stages = ahead.Pop().m_Stages ?? Array.Empty<TransitionAsset>();

                for (int i = 0; i < stages.Length && !plays; i++)
                {
                    plays = stages[i] == this;

                    if (stages[i] is SequenceTransition sequence && seen.Add(sequence))
                    {
                        ahead.Push(sequence);
                    }
                }
            }

            return plays;
        }
#endif

        private sealed class SequencePlayback : TransitionPlayback
        {
            private readonly TransitionAsset[]    m_Stages;
            private readonly TransitionPlayback[] m_Playbacks;
            private readonly float[]              m_Ends;

            internal SequencePlayback(TransitionAsset[] stages, TransitionPlayback[] playbacks, float[] ends)
            {
                m_Stages    = stages;
                m_Playbacks = playbacks;
                m_Ends      = ends;
            }

            protected internal override void Show(float covered)
            {
                float start = 0f;

                for (int i = 0; i < m_Playbacks.Length; i++)
                {
                    float end  = m_Ends[i];
                    float time = end > start ? math.saturate((covered - start) / (end - start)) : covered >= end ? 1f : 0f;

                    m_Playbacks[i].Show(m_Stages[i].Evaluate(time));

                    start = end;
                }
            }

            protected internal override void End()
            {
                for (int i = 0; i < m_Playbacks.Length; i++)
                {
                    m_Playbacks[i].End();
                }
            }
        }
    }
}