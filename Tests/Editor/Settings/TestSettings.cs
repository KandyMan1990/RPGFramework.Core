using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.Settings;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Settings
{
    /// <summary>Core's settings in a map, and a launch of the game reading the settings file.</summary>
    internal static class TestSettings
    {
        internal const MemoryBank S = MemoryBank.Settings;

        internal const ushort MUSIC_VOLUME = 8;

        /// <summary>Core's five settings, laid out as a map lays them out.</summary>
        internal static TestVariableMap Map()
        {
            TestVariableMap map = new TestVariableMap().Add(CoreVariables.LANGUAGE,             S, VariableWidth.ULong, 0,            0)
                                                       .Add(CoreVariables.MUSIC_VOLUME,         S, VariableWidth.Float, MUSIC_VOLUME, VariableDefaults.FromFloat(1f))
                                                       .Add(CoreVariables.SFX_VOLUME,           S, VariableWidth.Float, 12,           VariableDefaults.FromFloat(1f))
                                                       .Add(CoreVariables.BATTLE_MESSAGE_SPEED, S, VariableWidth.Float, 16,           VariableDefaults.FromFloat(0.5f))
                                                       .Add(CoreVariables.FIELD_MESSAGE_SPEED,  S, VariableWidth.Float, 20,           VariableDefaults.FromFloat(0.5f));

            return map;
        }

        /// <summary>A launch under <paramref name="map" />: the settings file in <paramref name="storage" /> read into new memory.</summary>
        internal static ISettingsService Launch(IVariableMap map, ISaveStorage storage, out IMemoryService memory)
        {
            MemoryService service = TestMemory.Make(16);

            SettingsService settings = new SettingsService(service, map, storage);

            ((ISettingsLoader)settings).Load();

            memory = service;

            return settings;
        }
    }
}
