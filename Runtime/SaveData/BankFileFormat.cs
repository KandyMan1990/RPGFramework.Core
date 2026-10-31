using System;
using System.Collections.Generic;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Hashing;

namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// How a file holds one bank under one variable map — a save its persistent bank, the settings file its settings
    /// bank: written with the map's layout beside it, and read back laid out as the map lays it out now, moved value by
    /// value when it was written under another.
    /// </summary>
    internal sealed class BankFileFormat
    {
        /// <summary>
        /// How a bank's section holds its bytes: as the bank holds them.
        /// </summary>
        internal const uint MEMORY_VERSION = 1;

        private readonly IVariableMap m_VariableMap;
        private readonly MemoryBank   m_Bank;
        private readonly int          m_BankBytes;
        private readonly ulong        m_MemorySectionId;
        private readonly ulong        m_LayoutSectionId;
        private readonly byte[]       m_Layout;

        internal BankFileFormat(IVariableMap variableMap, MemoryBank bank, int bankBytes, string memorySection, string layoutSection)
        {
            m_VariableMap     = variableMap;
            m_Bank            = bank;
            m_BankBytes       = bankBytes;
            m_MemorySectionId = Fnv1a64.Hash(memorySection);
            m_LayoutSectionId = Fnv1a64.Hash(layoutSection);
            m_Layout          = BankLayout.Write(variableMap, bank);
        }

        /// <summary>The sections as a file, with <paramref name="bank" /> and the map's layout put among them.</summary>
        internal byte[] Write(Dictionary<ulong, SectionBlob> sections, byte[] bank)
        {
            sections[m_MemorySectionId] = new SectionBlob(MEMORY_VERSION,             bank);
            sections[m_LayoutSectionId] = new SectionBlob(BankLayout.FORMAT_VERSION, m_Layout);

            return SectionFile.Write(sections);
        }

        /// <summary>
        /// A file's sections, and its bank laid out as the map lays it out. A file that is damaged or from a newer
        /// version cannot be loaded, and its bank holds the map's defaults, so a preview of it still reads.
        /// </summary>
        internal SaveState Read(byte[] file, Dictionary<ulong, SectionBlob> sections, out byte[] bank)
        {
            SectionFileStatus status = SectionFile.Read(file, sections);

            if (status != SectionFileStatus.Intact)
            {
                bank = DefaultBank();

                return status == SectionFileStatus.FromNewerVersion ? SaveState.FromNewerVersion : SaveState.Damaged;
            }

            // A file written before any variables existed in the bank has no section for it, so every variable reads its
            // default.
            byte[] saved  = sections.TryGetValue(m_MemorySectionId, out SectionBlob memory) ? memory.Data : Array.Empty<byte>();
            byte[] layout = sections.TryGetValue(m_LayoutSectionId, out SectionBlob written) ? written.Data : null;

            return ToCurrentLayout(saved, layout, out bank);
        }

        /// <summary>The bank as a new game or a first launch starts it, every variable at its default.</summary>
        internal byte[] DefaultBank()
        {
            byte[] bank = new byte[m_BankBytes];

            VariableDefaults.Write(bank, m_VariableMap, m_Bank);

            return bank;
        }

        /// <summary>
        /// As saved when the layouts match, otherwise moved value by value. A file with no layout was written before
        /// files recorded one, when the map could only ever grow at the end, so it is read by the map's own layout as
        /// far as its bytes reach. A layout that cannot be read, or a bank shorter than its layout describes, is
        /// damage.
        /// </summary>
        private SaveState ToCurrentLayout(byte[] saved, byte[] layout, out byte[] bank)
        {
            if (layout == null)
            {
                bank = BankMigration.Migrate(saved, BankLayout.Within(m_VariableMap, m_Bank, saved.Length), m_VariableMap, m_Bank, m_BankBytes);

                return SaveState.Loadable;
            }

            if (!BankLayout.TryRead(layout, out BankLayout savedLayout) || saved.Length < savedLayout.RequiredBytes)
            {
                bank = DefaultBank();

                return SaveState.Damaged;
            }

            if (layout.AsSpan().SequenceEqual(m_Layout))
            {
                bank = saved;

                return SaveState.Loadable;
            }

            bank = BankMigration.Migrate(saved, savedLayout, m_VariableMap, m_Bank, m_BankBytes);

            return BankMigration.IsFromNewerBuild(savedLayout, m_VariableMap) ? SaveState.FromNewerVersion : SaveState.Loadable;
        }
    }
}
