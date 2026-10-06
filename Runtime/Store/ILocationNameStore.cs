using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// The place the player is in, as the hash of its localisation key, so the menu can name it without knowing which
    /// module the player is in. Zero when the place has no name.
    /// </summary>
    public interface ILocationNameStore
    {
        ulong LocationName { get; }
        void  SetLocationName(ulong keyHash);
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.LOCATION_NAME" />, so each save holds the name of where it was made.
    /// </summary>
    internal sealed class LocationNameStore : ILocationNameStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_Address;

        public LocationNameStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.LOCATION_NAME, out VariableDefinition locationName);

            m_MemoryService = memoryService;
            m_Address       = (ushort)locationName.Offset;
        }

        ulong ILocationNameStore.LocationName
        {
            get
            {
                ulong keyHash = m_MemoryService.ReadUlong(MemoryBank.Persistent, m_Address);

                return keyHash;
            }
        }

        void ILocationNameStore.SetLocationName(ulong keyHash) => m_MemoryService.WriteUlong(MemoryBank.Persistent, m_Address, keyHash);
    }
}