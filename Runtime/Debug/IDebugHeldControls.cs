#if ENABLE_RPG_FRAMEWORK_DEBUG
namespace RPGFramework.Core.Input
{
    /// <summary>Which actions are held down now, for the debug room's way back from a field.</summary>
    public interface IDebugHeldControls
    {
        /// <summary>
        /// True while the action, named by the hash of its name, is held; false for one the module's map lacks, or while
        /// input is off.
        /// </summary>
        bool IsHeld(ulong action);
    }
}
#endif
