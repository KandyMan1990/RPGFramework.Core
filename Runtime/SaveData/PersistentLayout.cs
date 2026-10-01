using System.Collections.Generic;
using System.IO;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// Where each persistent variable sat in a save's bank — its id, offset, width and count, and a record's fields —
    /// written beside the bank so that a build whose map has changed since can still find every value. Fixed-size fields,
    /// little endian, as the rest of a save is.
    /// </summary>
    internal sealed class PersistentLayout
    {
        /// <summary>
        /// How a layout's bytes are arranged, which its section is written under.
        /// </summary>
        internal const uint FORMAT_VERSION = 1;

        /// <summary>
        /// What a save's values mean, for steps that change meaning rather than layout. 1 until the first step exists.
        /// </summary>
        internal const uint DATA_VERSION = 1;

        private const int HEADER_SIZE         = sizeof(uint) + sizeof(ushort);
        private const int VARIABLE_ENTRY_SIZE = sizeof(uint) + sizeof(ushort) + sizeof(ushort) + sizeof(byte) + sizeof(byte);
        private const int FIELD_ENTRY_SIZE    = sizeof(ushort) + sizeof(byte) + sizeof(ushort);

        internal readonly struct Field
        {
            internal readonly ushort        Id;
            internal readonly VariableWidth Width;
            internal readonly int           Count;

            internal int ByteCount => Width.GetByteCount() * Count;

            internal Field(ushort id, VariableWidth width, int count)
            {
                Id    = id;
                Width = width;
                Count = count;
            }
        }

        internal sealed class Entry
        {
            internal readonly uint          Id;
            internal readonly int           Offset;
            internal readonly int           Count;
            internal readonly VariableWidth Width;
            internal readonly Field[]       Fields;
            internal readonly int           ElementSize;

            internal bool IsRecord => Fields.Length > 0;

            internal Entry(uint id, int offset, int count, VariableWidth width, Field[] fields)
            {
                Id     = id;
                Offset = offset;
                Count  = count;
                Width  = width;
                Fields = fields;

                ElementSize = fields.Length > 0 ? 0 : width.GetByteCount();

                foreach (Field field in fields)
                {
                    ElementSize += field.ByteCount;
                }
            }

            internal bool TryGetField(ushort id, out Field field, out int fieldOffset)
            {
                fieldOffset = 0;

                foreach (Field candidate in Fields)
                {
                    if (candidate.Id == id)
                    {
                        field = candidate;
                        return true;
                    }

                    fieldOffset += candidate.ByteCount;
                }

                field = default;

                return false;
            }
        }

        private readonly Dictionary<uint, Entry> m_Entries;

        internal uint DataVersion { get; }

        /// <summary>
        /// How many bytes of bank the layout describes: what a save's bank section must hold.
        /// </summary>
        internal int RequiredBytes { get; }

        internal IEnumerable<Entry> Entries => m_Entries.Values;

        private PersistentLayout(uint dataVersion, Dictionary<uint, Entry> entries)
        {
            DataVersion = dataVersion;
            m_Entries   = entries;

            foreach (Entry entry in entries.Values)
            {
                int end = entry.Offset + entry.ElementSize * entry.Count;

                if (end > RequiredBytes)
                {
                    RequiredBytes = end;
                }
            }
        }

        internal bool TryGetEntry(uint id, out Entry entry)
        {
            bool found = m_Entries.TryGetValue(id, out entry);

            return found;
        }

        /// <summary>
        /// The layout of <paramref name="map" />'s persistent variables, as a save written now records it.
        /// </summary>
        internal static byte[] Write(IVariableMap map)
        {
            List<VariableDefinition> persistent = new List<VariableDefinition>();

            foreach (VariableDefinition variable in map.Variables)
            {
                if (variable.Bank == MemoryBank.Persistent)
                {
                    persistent.Add(variable);
                }
            }

            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(DATA_VERSION);
            writer.Write((ushort)persistent.Count);

            foreach (VariableDefinition variable in persistent)
            {
                writer.Write(variable.Id);
                writer.Write((ushort)variable.Offset);
                writer.Write((ushort)variable.Count);
                writer.Write((byte)variable.Width);
                writer.Write((byte)variable.Fields.Count);

                foreach (VariableRecordField field in variable.Fields)
                {
                    writer.Write(field.Id);
                    writer.Write((byte)field.Width);
                    writer.Write((ushort)field.Count);
                }
            }

            writer.Flush();

            byte[] bytes = stream.ToArray();

            return bytes;
        }

        /// <summary>
        /// False for bytes too short for what they declare — a layout that is not one.
        /// </summary>
        internal static bool TryRead(byte[] bytes, out PersistentLayout layout)
        {
            layout = null;

            if (bytes.Length < HEADER_SIZE)
            {
                return false;
            }

            using MemoryStream stream = new MemoryStream(bytes);
            using BinaryReader reader = new BinaryReader(stream);

            uint dataVersion   = reader.ReadUInt32();
            int  variableCount = reader.ReadUInt16();

            Dictionary<uint, Entry> entries = new Dictionary<uint, Entry>(variableCount);

            for (int i = 0; i < variableCount; i++)
            {
                if (stream.Length - stream.Position < VARIABLE_ENTRY_SIZE)
                {
                    return false;
                }

                uint          id         = reader.ReadUInt32();
                int           offset     = reader.ReadUInt16();
                int           count      = reader.ReadUInt16();
                VariableWidth width      = (VariableWidth)reader.ReadByte();
                int           fieldCount = reader.ReadByte();

                if (stream.Length - stream.Position < fieldCount * FIELD_ENTRY_SIZE)
                {
                    return false;
                }

                Field[] fields = new Field[fieldCount];

                for (int f = 0; f < fieldCount; f++)
                {
                    fields[f] = new Field(reader.ReadUInt16(), (VariableWidth)reader.ReadByte(), reader.ReadUInt16());
                }

                entries[id] = new Entry(id, offset, count, width, fields);
            }

            layout = new PersistentLayout(dataVersion, entries);

            return true;
        }

        /// <summary>
        /// For a save written before saves recorded their layout: <paramref name="map" />'s own, as far as the
        /// <paramref name="savedLength" /> bytes the save holds reach — what loading assumed of every save until then.
        /// </summary>
        internal static PersistentLayout Within(IVariableMap map, int savedLength)
        {
            Dictionary<uint, Entry> entries = new Dictionary<uint, Entry>();

            foreach (VariableDefinition variable in map.Variables)
            {
                if (variable.Bank != MemoryBank.Persistent || variable.EndOffset > savedLength)
                {
                    continue;
                }

                Field[] fields = new Field[variable.Fields.Count];

                for (int f = 0; f < fields.Length; f++)
                {
                    VariableRecordField field = variable.Fields[f];

                    fields[f] = new Field(field.Id, field.Width, field.Count);
                }

                entries[variable.Id] = new Entry(variable.Id, variable.Offset, variable.Count, variable.Width, fields);
            }

            PersistentLayout layout = new PersistentLayout(DATA_VERSION, entries);

            return layout;
        }
    }
}
