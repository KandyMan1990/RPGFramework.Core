using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// The track every battle plays, as its music asset's name hash, until it is set again — so no battle has to name
    /// it, and a stretch of the story can change it for every battle in it. Zero plays none.
    /// </summary>
    public interface IBattleMusicStore
    {
        /// <summary>The battle track's music asset name hash, or zero for none.</summary>
        ulong BattleMusic { get; }

        /// <summary>
        /// Sets the track every battle plays from now on, by its music asset's name hash; zero for none.
        /// </summary>
        void SetBattleMusic(ulong nameHash);
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.BATTLE_MUSIC" />, so a save holds it and a new game starts from its default.
    /// </summary>
    internal sealed class BattleMusicStore : IBattleMusicStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_Address;

        public BattleMusicStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.BATTLE_MUSIC, out VariableDefinition battleMusic);

            m_MemoryService = memoryService;
            m_Address       = (ushort)battleMusic.Offset;
        }

        ulong IBattleMusicStore.BattleMusic
        {
            get
            {
                ulong nameHash = m_MemoryService.ReadUlong(MemoryBank.Persistent, m_Address);

                return nameHash;
            }
        }

        void IBattleMusicStore.SetBattleMusic(ulong nameHash) => m_MemoryService.WriteUlong(MemoryBank.Persistent, m_Address, nameHash);
    }
}
