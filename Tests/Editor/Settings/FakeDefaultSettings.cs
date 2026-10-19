using RPGFramework.Core.SaveData;
using RPGFramework.Core.Settings;

namespace RPGFramework.Core.Tests.Settings
{
    /// <summary>A game's defaults: two ints at versions 1 and 2, a long and a byte.</summary>
    internal sealed class FakeDefaultSettings : IDefaultSettings
    {
        internal const string A = "Game.A";
        internal const string B = "Game.B";
        internal const string C = "Game.C";
        internal const string D = "Game.D";

        void IDefaultSettings.Populate(ISettingsService settingsService)
        {
            settingsService.SetSection(A, new SaveSection<int>(1, 0));
            settingsService.SetSection(B, new SaveSection<int>(2, 0));
            settingsService.SetSection(C, new SaveSection<long>(1, 0));
            settingsService.SetSection(D, new SaveSection<byte>(1, 5));
        }
    }
}
