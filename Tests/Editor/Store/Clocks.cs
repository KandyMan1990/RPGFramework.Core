using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Store
{
    internal static class Clocks
    {
        /// <summary>The play time and the countdown, as a map lays them out.</summary>
        internal static TestVariableMap Map()
        {
            TestVariableMap map = new TestVariableMap().Add(CoreVariables.PLAY_TIME,       MemoryBank.Persistent, VariableWidth.UInt, 0, 0)
                                                       .Add(CoreVariables.COUNTDOWN,       MemoryBank.Persistent, VariableWidth.UInt, 4, 0)
                                                       .Add(CoreVariables.COUNTDOWN_SHOWN, MemoryBank.Persistent, VariableWidth.Bool, 8, 0)
                                                       .Add(CoreVariables.COUNTDOWN_X,     MemoryBank.Persistent, VariableWidth.Int,  12, 0)
                                                       .Add(CoreVariables.COUNTDOWN_Y,     MemoryBank.Persistent, VariableWidth.Int,  16, 0);

            return map;
        }
    }
}
