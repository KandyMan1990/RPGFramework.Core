using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using Unity.Mathematics;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// What changes the party's money, so the game's maximum is held in one place. Anything showing the money reads
    /// <see cref="CoreVariables.MONEY" />.
    /// </summary>
    public interface IMoneyStore
    {
        /// <summary>
        /// Changes the party's money by an amount, negative to take some away, holding it between none and the game's
        /// maximum.
        /// </summary>
        void AddMoney(int amount);
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.MONEY" />, so a save holds it and a new game starts from its default, and limited
    /// by the default of <see cref="CoreVariables.MONEY_MAXIMUM" />.
    /// </summary>
    internal sealed class MoneyStore : IMoneyStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_MoneyAddress;
        private readonly ushort         m_MaximumAddress;

        public MoneyStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.MONEY,         out VariableDefinition money);
            variableMap.TryGetVariable(CoreVariables.MONEY_MAXIMUM, out VariableDefinition maximum);

            m_MemoryService  = memoryService;
            m_MoneyAddress   = (ushort)money.Offset;
            m_MaximumAddress = (ushort)maximum.Offset;
        }

        void IMoneyStore.AddMoney(int amount)
        {
            uint maximum = m_MemoryService.ReadUint(MemoryBank.Session, m_MaximumAddress);
            uint money   = m_MemoryService.ReadUint(MemoryBank.Persistent, m_MoneyAddress);
            long limit   = maximum == 0 ? uint.MaxValue : maximum;
            long changed = math.clamp((long)money + amount, 0L, limit);

            m_MemoryService.WriteUint(MemoryBank.Persistent, m_MoneyAddress, (uint)changed);
        }
    }
}
