using System.Collections.Generic;
using RPGFramework.Core.Data;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Hashing;

namespace RPGFramework.Core.Tests.Saves
{
    internal static class SaveFiles
    {
        internal const string FILE_NAME = "save000.sav";

        /// <summary>A save service over <paramref name="map" />, its banks as big as the map needs, keeping its files in <paramref name="storage" />.</summary>
        internal static ISaveDataService Service(IVariableMap map, ISaveStorage storage, out IMemoryService memory)
        {
            MemoryService service = TestMemory.Make(64);

            memory = service;

            ISaveDataService saves = new SaveDataService(service, service, map, storage);

            return saves;
        }

        /// <summary>A save file holding a persistent bank and, unless it is null, the layout it was written in.</summary>
        internal static byte[] Holding(byte[] persistent, byte[] layout)
        {
            Dictionary<ulong, SectionBlob> sections = new Dictionary<ulong, SectionBlob>
                                                      {
                                                          [Fnv1a64.Hash(FrameworkSaveSections.PERSISTENT_MEMORY)] = new SectionBlob(Versions.PERSISTENT_MEMORY, persistent)
                                                      };

            if (layout != null)
            {
                sections[Fnv1a64.Hash(FrameworkSaveSections.PERSISTENT_MEMORY_LAYOUT)] = new SectionBlob(PersistentLayout.FORMAT_VERSION, layout);
            }

            byte[] file = SectionFile.Write(sections);

            return file;
        }

        /// <summary>The second version of the map, with the variable a save says whether it was loaded from.</summary>
        internal static TestVariableMap Version2WithLoadedFromSave()
        {
            TestVariableMap map = SaveLayouts.Version2().Add(CoreVariables.LOADED_FROM_SAVE, MemoryBank.Session, VariableWidth.Bool, 0, 0);

            return map;
        }
    }
}
