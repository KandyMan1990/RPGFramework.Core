using System.Collections.Generic;
using RPGFramework.Core.SaveData;
using RPGFramework.Hashing;

namespace RPGFramework.Core.Tests.Saves
{
    internal static class SectionFiles
    {
        internal const string FIRST = "Game.First";
        internal const string OTHER = "Game.Other";

        internal static readonly byte[] FIRST_DATA = { 1, 2, 3, 4, 5 };
        internal static readonly byte[] OTHER_DATA = { 8, 7, 6, 5, 4, 3, 2, 1 };

        /// <summary>A file of two sections of different versions and sizes.</summary>
        internal static byte[] TwoSections()
        {
            Dictionary<ulong, SectionBlob> sections = new Dictionary<ulong, SectionBlob>
                                                      {
                                                          [Fnv1a64.Hash(FIRST)] = new SectionBlob(3, FIRST_DATA),
                                                          [Fnv1a64.Hash(OTHER)] = new SectionBlob(7, OTHER_DATA)
                                                      };

            byte[] file = SectionFile.Write(sections);

            return file;
        }

        internal static SectionFileStatus Read(byte[] file, out Dictionary<ulong, SectionBlob> sections)
        {
            sections = new Dictionary<ulong, SectionBlob>();

            SectionFileStatus status = SectionFile.Read(file, sections);

            return status;
        }
    }
}
