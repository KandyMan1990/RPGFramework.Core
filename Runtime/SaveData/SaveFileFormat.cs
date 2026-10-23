using System;
using System.Collections.Generic;
using RPGFramework.Core.Data;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Hashing;

namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// How a save file holds the persistent bank under one variable map: written with the map's layout beside it, and
    /// read back laid out as the map lays it out now, moved value by value when it was written under another.
    /// </summary>
    internal sealed class SaveFileFormat
    {
        private readonly IVariableMap m_VariableMap;
        private readonly int          m_BankBytes;
        private readonly ulong        m_MemorySectionId;
        private readonly ulong        m_LayoutSectionId;
        private readonly byte[]       m_Layout;

        internal SaveFileFormat(IVariableMap variableMap, int bankBytes)
        {
            m_VariableMap     = variableMap;
            m_BankBytes       = bankBytes;
            m_MemorySectionId = Fnv1a64.Hash(FrameworkSaveSections.PERSISTENT_MEMORY);
            m_LayoutSectionId = Fnv1a64.Hash(FrameworkSaveSections.PERSISTENT_MEMORY_LAYOUT);
            m_Layout          = PersistentLayout.Write(variableMap);
        }

        /// <summary>The sections as a save file, with <paramref name="bank" /> and the map's layout put among them.</summary>
        internal byte[] Write(Dictionary<ulong, SectionBlob> sections, byte[] bank)
        {
            sections[m_MemorySectionId] = new SectionBlob(Versions.PERSISTENT_MEMORY,      bank);
            sections[m_LayoutSectionId] = new SectionBlob(PersistentLayout.FORMAT_VERSION, m_Layout);

            return SectionFile.Write(sections);
        }

        /// <summary>
        /// A save's sections, and its persistent bank laid out as the map lays it out. A save that is damaged or from a
        /// newer version cannot be loaded, and its bank holds the map's defaults, so a preview of it still reads.
        /// </summary>
        internal SaveState Read(byte[] save, Dictionary<ulong, SectionBlob> sections, out byte[] bank)
        {
            SectionFileStatus file = SectionFile.Read(save, sections);

            if (file != SectionFileStatus.Intact)
            {
                bank = DefaultBank();

                return file == SectionFileStatus.FromNewerVersion ? SaveState.FromNewerVersion : SaveState.Damaged;
            }

            // A save written before any variables existed has no persistent memory, so every variable reads its
            // default.
            byte[] persistent = sections.TryGetValue(m_MemorySectionId, out SectionBlob memory) ? memory.Data : Array.Empty<byte>();
            byte[] layout     = sections.TryGetValue(m_LayoutSectionId, out SectionBlob saved) ? saved.Data : null;

            return ToCurrentLayout(persistent, layout, out bank);
        }

        /// <summary>The persistent bank as a new game starts it, every variable at its default.</summary>
        internal byte[] DefaultBank()
        {
            byte[] bank = new byte[m_BankBytes];

            VariableDefaults.Write(bank, m_VariableMap, MemoryBank.Persistent);

            return bank;
        }

        /// <summary>
        /// As saved when the layouts match, otherwise moved value by value. A save with no layout was written before
        /// saves recorded one, when the map could only ever grow at the end, so it is read by the map's own layout as
        /// far as its bytes reach. A layout that cannot be read, or a bank shorter than its layout describes, is
        /// damage.
        /// </summary>
        private SaveState ToCurrentLayout(byte[] persistent, byte[] layout, out byte[] bank)
        {
            if (layout == null)
            {
                bank = PersistentMigration.Migrate(persistent, PersistentLayout.Within(m_VariableMap, persistent.Length), m_VariableMap, m_BankBytes);

                return SaveState.Loadable;
            }

            if (!PersistentLayout.TryRead(layout, out PersistentLayout savedLayout) || persistent.Length < savedLayout.RequiredBytes)
            {
                bank = DefaultBank();

                return SaveState.Damaged;
            }

            if (layout.AsSpan().SequenceEqual(m_Layout))
            {
                bank = persistent;

                return SaveState.Loadable;
            }

            bank = PersistentMigration.Migrate(persistent, savedLayout, m_VariableMap, m_BankBytes);

            return PersistentMigration.IsFromNewerBuild(savedLayout, m_VariableMap) ? SaveState.FromNewerVersion : SaveState.Loadable;
        }
    }
}
