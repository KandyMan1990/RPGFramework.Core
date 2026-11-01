using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Store
{
    /// <summary>
    /// The countdown: seconds a script sets, counted down beside the play time whenever they are above zero, in every
    /// module, shown or not, and saved with the game. Shown, the field and a battle carrying it put it on screen in the
    /// same place, and the menu's clock shows it in place of the play time; at zero it ends the game unless the battle
    /// mode lets it run out.
    /// </summary>
    public interface ICountdownStore
    {
        /// <summary>The seconds left.</summary>
        uint Seconds { get; }

        /// <summary>Sets the seconds left, which start counting down at once; zero stops it.</summary>
        void SetSeconds(uint seconds);

        /// <summary>Whether the countdown is on screen.</summary>
        bool IsShown { get; }

        /// <summary>Shows the countdown or hides it. Hidden, it still counts down.</summary>
        void SetShown(bool shown);

        /// <summary>Where it is shown, across, in UI units.</summary>
        int X { get; }

        /// <summary>Where it is shown, down, in UI units.</summary>
        int Y { get; }

        /// <summary>Puts it there, in every module that shows it, until put somewhere else.</summary>
        void SetPosition(int x, int y);
    }

    /// <summary>
    /// Kept in <see cref="CoreVariables.COUNTDOWN" />, <see cref="CoreVariables.COUNTDOWN_SHOWN" /> and its position's
    /// two, so a save holds it; counted down by the play time's counter.
    /// </summary>
    internal sealed class CountdownStore : ICountdownStore
    {
        private readonly IMemoryService m_MemoryService;
        private readonly ushort         m_SecondsAddress;
        private readonly ushort         m_ShownAddress;
        private readonly ushort         m_XAddress;
        private readonly ushort         m_YAddress;

        public CountdownStore(IMemoryService memoryService, IVariableMap variableMap)
        {
            variableMap.TryGetVariable(CoreVariables.COUNTDOWN,       out VariableDefinition seconds);
            variableMap.TryGetVariable(CoreVariables.COUNTDOWN_SHOWN, out VariableDefinition shown);
            variableMap.TryGetVariable(CoreVariables.COUNTDOWN_X,     out VariableDefinition x);
            variableMap.TryGetVariable(CoreVariables.COUNTDOWN_Y,     out VariableDefinition y);

            m_MemoryService  = memoryService;
            m_SecondsAddress = (ushort)seconds.Offset;
            m_ShownAddress   = (ushort)shown.Offset;
            m_XAddress       = (ushort)x.Offset;
            m_YAddress       = (ushort)y.Offset;
        }

        uint ICountdownStore.Seconds => (uint)m_MemoryService.ReadInt(MemoryBank.Persistent, m_SecondsAddress);

        void ICountdownStore.SetSeconds(uint seconds) => m_MemoryService.WriteInt(MemoryBank.Persistent, m_SecondsAddress, (int)seconds);

        bool ICountdownStore.IsShown => m_MemoryService.ReadBool(MemoryBank.Persistent, m_ShownAddress);

        void ICountdownStore.SetShown(bool shown) => m_MemoryService.WriteBool(MemoryBank.Persistent, m_ShownAddress, shown);

        int ICountdownStore.X => m_MemoryService.ReadInt(MemoryBank.Persistent, m_XAddress);

        int ICountdownStore.Y => m_MemoryService.ReadInt(MemoryBank.Persistent, m_YAddress);

        void ICountdownStore.SetPosition(int x, int y)
        {
            m_MemoryService.WriteInt(MemoryBank.Persistent, m_XAddress, x);
            m_MemoryService.WriteInt(MemoryBank.Persistent, m_YAddress, y);
        }
    }
}
