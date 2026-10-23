#if ENABLE_RPG_FRAMEWORK_DEBUG
namespace RPGFramework.Core.SaveData
{
    /// <summary>The playthrough as a save file's bytes, and a playthrough begun from them, for the debug room's scenarios.</summary>
    public interface IDebugSaveDataService
    {
        /// <summary>The playthrough as a save file holds it, without writing one.</summary>
        byte[] CaptureSave();

        /// <summary>
        /// Begins a playthrough from a save file's bytes, arriving as a new game does — at the module's own way in, not
        /// where the player stood — under a save file name no file has yet. False, beginning nothing, for bytes that are
        /// damaged or from a newer version.
        /// </summary>
        bool TryBeginSave(byte[] save);
    }
}
#endif
