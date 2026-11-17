using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using Unity.Mathematics;

namespace RPGFramework.Core.Store
{
    /// <summary>The game's range of message speeds, from which the player's setting picks how fast text types.</summary>
    internal interface IMessageSpeedStore
    {
        /// <summary>
        /// The characters a second text types at for a message speed setting: 0 the game's slowest, 1 its fastest, and
        /// in a straight line between them.
        /// </summary>
        float GetCharactersPerSecond(float messageSpeed);
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.MESSAGE_SPEED_SLOWEST" /> and <see cref="CoreVariables.MESSAGE_SPEED_FASTEST" />,
    /// Session variables whose defaults every new game and load writes, so the current build's range is used whatever
    /// the player's settings file holds.
    /// </summary>
    internal sealed class MessageSpeedStore : IMessageSpeedStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_SlowestAddress;
        private readonly ushort         m_FastestAddress;

        public MessageSpeedStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.MESSAGE_SPEED_SLOWEST, out VariableDefinition slowest);
            variableMap.TryGetVariable(CoreVariables.MESSAGE_SPEED_FASTEST, out VariableDefinition fastest);

            m_MemoryService  = memoryService;
            m_SlowestAddress = (ushort)slowest.Offset;
            m_FastestAddress = (ushort)fastest.Offset;
        }

        float IMessageSpeedStore.GetCharactersPerSecond(float messageSpeed)
        {
            float slowest = m_MemoryService.ReadFloat(MemoryBank.Session, m_SlowestAddress);
            float fastest = m_MemoryService.ReadFloat(MemoryBank.Session, m_FastestAddress);

            return math.lerp(slowest, fastest, messageSpeed);
        }
    }
}
