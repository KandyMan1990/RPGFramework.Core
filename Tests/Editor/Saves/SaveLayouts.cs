using System;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    /// <summary>Two versions of a map, a save written under the first, and the layouts a save records.</summary>
    internal static class SaveLayouts
    {
        private const MemoryBank P = MemoryBank.Persistent;

        internal static VariableRecordField Field(string name, VariableWidth width, int count, ulong defaultValue, ushort id)
        {
            VariableRecordField field = new VariableRecordField(name, width, count, defaultValue);

            field.AssignId(id);

            return field;
        }

        internal static PersistentLayout Of(IVariableMap map)
        {
            PersistentLayout layout = Read(PersistentLayout.Write(map));

            return layout;
        }

        internal static PersistentLayout Read(byte[] bytes)
        {
            PersistentLayout.TryRead(bytes, out PersistentLayout layout);

            return layout;
        }

        internal static TestVariableMap Version1()
        {
            VariableRecordField[] party =
            {
                Field("hp", VariableWidth.UShort, 1, 0, 1),
                Field("level", VariableWidth.Byte, 1, 0, 2),
                Field("gone", VariableWidth.Byte, 1, 0, 3)
            };

            TestVariableMap map = new TestVariableMap().Add("gil", P, VariableWidth.UInt, 0, 0)
                                                       .Add("flag", P, VariableWidth.Bool, 4, 0)
                                                       .Add("deleted", P, VariableWidth.Byte, 5, 0)
                                                       .Add("counter", P, VariableWidth.Byte, 6, 0)
                                                       .Add("items", P, VariableWidth.UShort, 8, 0, 3)
                                                       .AddRecord("party", P, 14, 2, party)
                                                       .Add("narrow", P, VariableWidth.Int, 22, 0)
                                                       .Add("ratio", P, VariableWidth.Float, 26, 0)
                                                       .Add("kind", P, VariableWidth.Byte, 30, 0);

            return map;
        }

        internal static byte[] SavedUnderVersion1()
        {
            byte[] saved = new byte[31];

            BitConverter.GetBytes(70000u).CopyTo(saved, 0);
            saved[4] = 1;
            saved[5] = 99;
            saved[6] = 200;
            BitConverter.GetBytes((ushort)10).CopyTo(saved, 8);
            BitConverter.GetBytes((ushort)20).CopyTo(saved, 10);
            BitConverter.GetBytes((ushort)30).CopyTo(saved, 12);
            BitConverter.GetBytes((ushort)100).CopyTo(saved, 14);
            saved[16] = 5;
            saved[17] = 1;
            BitConverter.GetBytes((ushort)200).CopyTo(saved, 18);
            saved[20] = 6;
            saved[21] = 2;
            BitConverter.GetBytes(-70000).CopyTo(saved, 22);
            BitConverter.GetBytes(3.7f).CopyTo(saved, 26);
            saved[30] = 5;

            return saved;
        }

        /// <summary>
        /// The next version: gil renamed and moved, deleted gone, counter wider, items longer, party's fields reordered,
        /// widened, one dropped and one added and a record fewer, narrow and ratio narrower, kind now a record, and a
        /// variable added.
        /// </summary>
        internal static TestVariableMap Version2()
        {
            VariableRecordField[] party =
            {
                Field("level", VariableWidth.Byte, 1, 0, 2),
                Field("hp", VariableWidth.UInt, 1, 0, 1),
                Field("mp", VariableWidth.UShort, 1, 50, 4)
            };

            TestVariableMap map = new TestVariableMap().Add("flag", P, VariableWidth.Bool, 0, 0, id: 2)
                                                       .Add("counter", P, VariableWidth.UShort, 2, 0, id: 4)
                                                       .Add("items", P, VariableWidth.UShort, 4, 7, 5, 5)
                                                       .Add("narrow", P, VariableWidth.Short, 14, 0, id: 7)
                                                       .Add("ratio", P, VariableWidth.Int, 16, 0, id: 8)
                                                       .AddRecord("party", P, 20, 1, party, 6)
                                                       .AddRecord("kind", P, 28, 1, new[] { Field("x", VariableWidth.Byte, 1, 9, 1) }, 9)
                                                       .Add("added", P, VariableWidth.Byte, 29, 42, id: 10)
                                                       .Add("money", P, VariableWidth.UInt, 32, 0, id: 1);

            return map;
        }

        /// <summary>A map from a build after the second, with a variable it never gave.</summary>
        internal static TestVariableMap WithAVariableFromLater()
        {
            TestVariableMap map = new TestVariableMap().Add("flag", P, VariableWidth.Bool, 0, 0, id: 2)
                                                       .Add("later", P, VariableWidth.Byte, 1, 0, id: 20);

            return map;
        }
    }
}
