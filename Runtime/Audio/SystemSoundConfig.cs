using UnityEngine;

namespace RPGFramework.Core.Audio
{
    [CreateAssetMenu(menuName = "RPG Framework/Audio/System Sound Config", fileName = "System Sound Config")]
    public sealed class SystemSoundConfig : ScriptableObject, ISystemSoundConfig
    {
        [SerializeField]
        [SoundName]
        [Tooltip("The cursor moving to another choice")]
        private ulong m_Cursor;

        [SerializeField]
        [SoundName]
        [Tooltip("A choice taken")]
        private ulong m_Confirm;

        [SerializeField]
        [SoundName]
        [Tooltip("Backing out, or a question's cursor moved to its cancel answer")]
        private ulong m_Cancel;

        [SerializeField]
        [SoundName]
        [Tooltip("A choice refused: nothing to load, saving not allowed here, a binding refused")]
        private ulong m_Error;

        [SerializeField]
        [SoundName]
        [Tooltip("A new game begun")]
        private ulong m_NewGame;

        [SerializeField]
        [SoundName]
        [Tooltip("A saved game chosen to load")]
        private ulong m_LoadGame;

        [SerializeField]
        [SoundName]
        [Tooltip("The game saved")]
        private ulong m_SaveGame;

        ulong ISystemSoundConfig.Cursor   => m_Cursor;
        ulong ISystemSoundConfig.Confirm  => m_Confirm;
        ulong ISystemSoundConfig.Cancel   => m_Cancel;
        ulong ISystemSoundConfig.Error    => m_Error;
        ulong ISystemSoundConfig.NewGame  => m_NewGame;
        ulong ISystemSoundConfig.LoadGame => m_LoadGame;
        ulong ISystemSoundConfig.SaveGame => m_SaveGame;
    }
}
