using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// Whether the player can save from the party menu right now. Its default in the variable map is the game's policy,
    /// so save points are a choice a game makes rather than something the framework imposes.
    /// </summary>
    public interface ISaveEnabledStore
    {
        bool GetSaveEnabled { get; }
        void SetSaveEnabled(bool enabled);

        /// <summary>
        /// Back to the game's policy, as a module does on entering a place.
        /// </summary>
        void ResetSaveEnabled();
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.SAVE_ENABLED" />, in Session memory, so it is never saved.
    /// </summary>
    internal sealed class SaveEnabledStore : ISaveEnabledStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_Address;
        private readonly bool           m_Default;

        public SaveEnabledStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.SAVE_ENABLED, out VariableDefinition saveEnabled);

            m_MemoryService = memoryService;
            m_Address       = (ushort)saveEnabled.Offset;
            m_Default       = VariableDefaults.ToBool(saveEnabled.DefaultValue);
        }

        bool ISaveEnabledStore.GetSaveEnabled
        {
            get
            {
                bool enabled = m_MemoryService.ReadBool(MemoryBank.Session, m_Address);

                return enabled;
            }
        }

        void ISaveEnabledStore.SetSaveEnabled(bool enabled) => m_MemoryService.WriteBool(MemoryBank.Session, m_Address, enabled);

        void ISaveEnabledStore.ResetSaveEnabled() => m_MemoryService.WriteBool(MemoryBank.Session, m_Address, m_Default);
    }
}