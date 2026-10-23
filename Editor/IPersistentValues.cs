using System.Collections.Generic;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// The persistent variables' values as editor tooling sets them outside a playthrough: where each value is and
    /// what a new game starts it at, a bank of them, and a save file holding one, as the game writes and reads it.
    /// </summary>
    public interface IPersistentValues
    {
        /// <summary>Every persistent value, in the map's order: a variable, an array's elements, or records' fields.</summary>
        IReadOnlyList<PersistentValue> Values { get; }

        /// <summary>The persistent bank as a new game starts it, every value at its default.</summary>
        byte[] CreateDefaultBank();

        /// <summary>The value in <paramref name="bank" />, as the bytes it occupies, lowest first.</summary>
        ulong Read(byte[] bank, PersistentValue value);

        void Write(byte[] bank, PersistentValue value, ulong bits);

        /// <summary>A save file holding <paramref name="bank" />, as the game writes one.</summary>
        byte[] WriteSave(byte[] bank);

        /// <summary>The bank a save file holds, laid out as the map lays it out now; false for one that cannot be loaded.</summary>
        bool TryReadSave(byte[] save, out byte[] bank);
    }
}
