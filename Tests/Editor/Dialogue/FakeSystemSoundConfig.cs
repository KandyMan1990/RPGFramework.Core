using RPGFramework.Core.Audio;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>A question's three sounds, each a hash of its own, or all none.</summary>
    internal sealed class FakeSystemSoundConfig : ISystemSoundConfig
    {
        internal const ulong CURSOR  = 1;
        internal const ulong CONFIRM = 2;
        internal const ulong CANCEL  = 3;

        private readonly ulong m_Cursor;
        private readonly ulong m_Confirm;
        private readonly ulong m_Cancel;

        internal FakeSystemSoundConfig() : this(CURSOR, CONFIRM, CANCEL)
        {
        }

        private FakeSystemSoundConfig(ulong cursor, ulong confirm, ulong cancel)
        {
            m_Cursor  = cursor;
            m_Confirm = confirm;
            m_Cancel  = cancel;
        }

        internal static FakeSystemSoundConfig Silent() => new FakeSystemSoundConfig(0, 0, 0);

        ulong ISystemSoundConfig.Cursor   => m_Cursor;
        ulong ISystemSoundConfig.Confirm  => m_Confirm;
        ulong ISystemSoundConfig.Cancel   => m_Cancel;
        ulong ISystemSoundConfig.Error    => 0;
        ulong ISystemSoundConfig.NewGame  => 0;
        ulong ISystemSoundConfig.LoadGame => 0;
        ulong ISystemSoundConfig.SaveGame => 0;
    }
}
