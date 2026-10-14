using RPGFramework.Core.Memory;
using RPGFramework.Core.PlayerLoop;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// How long the playthrough has been played, in whole seconds.
    /// </summary>
    public interface IPlayTimeStore
    {
        /// <summary>The whole seconds played so far, saved with the game.</summary>
        uint PlayTime { get; }

        /// <summary>
        /// Start the clock, as the player leaves the title for the game. It runs from then on, in every module, until
        /// <see cref="StopCounting" />; starting it while it runs does nothing.
        /// </summary>
        void StartCounting();

        /// <summary>Stop the clock, as the game goes back to the title.</summary>
        void StopCounting();
    }

    /// <summary>
    /// Counts <see cref="CoreVariables.PLAY_TIME" /> up once the game has started, in every module, in real time rather
    /// than scaled time. Whole seconds are what is saved; the fraction is carried from frame to frame, since
    /// adding each frame's time to a float would drift within hours of play.
    /// </summary>
    internal sealed class PlayTimeCounter : IPlayTimeStore, IUpdatable
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_Address;

        private float m_Fraction;
        private bool  m_Counting;

        public PlayTimeCounter(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.PLAY_TIME, out VariableDefinition playTime);

            m_MemoryService = memoryService;
            m_Address       = (ushort)playTime.Offset;
            m_Fraction      = 0f;
            m_Counting      = false;
        }

        uint IPlayTimeStore.PlayTime
        {
            get
            {
                uint seconds = (uint)m_MemoryService.ReadInt(MemoryBank.Persistent, m_Address);

                return seconds;
            }
        }

        void IPlayTimeStore.StartCounting()
        {
            if (m_Counting)
            {
                return;
            }

            m_Counting = true;
            m_Fraction = 0f;

            UpdateManager.RegisterUpdatable(this);
        }

        void IPlayTimeStore.StopCounting()
        {
            if (!m_Counting)
            {
                return;
            }

            m_Counting = false;

            UpdateManager.UnregisterUpdatable(this);
        }

        void IUpdatable.Update()
        {
            Advance(Time.unscaledDeltaTime);
        }

        private void Advance(float deltaTime)
        {
            m_Fraction += deltaTime;

            if (m_Fraction < 1f)
            {
                return;
            }

            uint seconds = (uint)m_Fraction;
            m_Fraction -= seconds;

            uint playTime = (uint)m_MemoryService.ReadInt(MemoryBank.Persistent, m_Address);

            m_MemoryService.WriteInt(MemoryBank.Persistent, m_Address, (int)(playTime + seconds));
        }
    }
}