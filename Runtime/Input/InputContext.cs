using UnityEngine;

namespace RPGFramework.Core.Input
{
    public enum ControlSlot
    {
        Primary,
        Secondary,
        Tertiary,
        Quaternary,
        ShoulderLeft,
        ShoulderRight,
        TriggerLeft,
        TriggerRight,
        Start,
        Select
    }

    public interface IInputContext
    {
        bool Handle(ControlSlot slot);
        void HandleMove(Vector2 move);
    }
}