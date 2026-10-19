using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal static class MapWithAGap
    {
        /// <summary>
        /// Three ints, the middle one deleted, then a short, a byte and a long allocated in Persistent, and an int in
        /// Session: "a" at 0, "s" at 4, "t" at 6, "c" at 8 and "l" at 16, and "x" at 0 in its own bank.
        /// </summary>
        internal static VariableMapAsset Make()
        {
            VariableMapAsset map = MapAuthoring.Empty();

            map.Allocate("a", MemoryBank.Persistent, VariableWidth.Int, 1, string.Empty, 0);
            map.Allocate("b", MemoryBank.Persistent, VariableWidth.Int, 1, string.Empty, 0);
            map.Allocate("c", MemoryBank.Persistent, VariableWidth.Int, 1, string.Empty, 0);

            MapAuthoring.Delete(map, "b");

            map.Allocate("s", MemoryBank.Persistent, VariableWidth.Short, 1, string.Empty, 0);
            map.Allocate("t", MemoryBank.Persistent, VariableWidth.Byte, 1, string.Empty, 0);
            map.Allocate("l", MemoryBank.Persistent, VariableWidth.Long, 1, string.Empty, 0);
            map.Allocate("x", MemoryBank.Session, VariableWidth.Int, 1, string.Empty, 0);

            return map;
        }

        internal static int OffsetOf(VariableMapAsset map, string name)
        {
            int offset = MapAuthoring.Named(map, name).Offset;

            return offset;
        }
    }
}
