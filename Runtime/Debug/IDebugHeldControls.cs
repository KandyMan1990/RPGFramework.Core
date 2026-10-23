#if ENABLE_RPG_FRAMEWORK_DEBUG
namespace RPGFramework.Core.Input
{
    /// <summary>Which control slots are held down now, for the debug room's way back from a field.</summary>
    public interface IDebugHeldControls
    {
        /// <summary>True while the slot's control is held; false for a slot with no action, or while input is off.</summary>
        bool IsHeld(ControlSlot slot);
    }
}
#endif
