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
        uint PlayTime { get; }

        /// <summary>
        /// Start the clock, as the player leaves the title for the game. It runs from then on, in every module.
        /// </summary>
        void StartCounting();
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

        public PlayTimeCounter(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.PLAY_TIME, out VariableDefinition playTime);

            m_MemoryService = memoryService;
            m_Address       = (ushort)playTime.Offset;
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
            UpdateManager.RegisterUpdatable(this);
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