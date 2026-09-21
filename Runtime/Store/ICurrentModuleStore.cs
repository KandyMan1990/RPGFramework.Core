using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// The module the playthrough is in: where a new game begins and where a loaded one resumes. Not the same
    /// as <see cref="IChangeModuleStore" />, which is the next transition's target and is a menu or a battle as
    /// often as it is a place.
    /// </summary>
    public interface ICurrentModuleStore
    {
        byte GetModuleId { get; }
        void SetModuleId(byte moduleId);
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.CURRENT_MODULE" />, so it is saved with the rest of persistent memory and
    /// a new game's value is that variable's default.
    /// </summary>
    internal sealed class CurrentModuleStore : ICurrentModuleStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_Address;

        public CurrentModuleStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.CURRENT_MODULE, out VariableDefinition currentModule);

            m_MemoryService = memoryService;
            m_Address       = (ushort)currentModule.Offset;
        }

        byte ICurrentModuleStore.GetModuleId
        {
            get
            {
                byte moduleId = m_MemoryService.ReadByte(MemoryBank.Persistent, m_Address);

                return moduleId;
            }
        }

        void ICurrentModuleStore.SetModuleId(byte moduleId) => m_MemoryService.WriteByte(MemoryBank.Persistent, m_Address, moduleId);
    }
}
