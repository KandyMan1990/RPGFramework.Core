namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// The game's own save sections. Where a game begins and resumes is not the game's to write: every
    /// variable starts at its default when a save begins, and the module and field a game is in are
    /// variables — see <see cref="RPGFramework.Core.Memory.CoreVariables" />.
    /// </summary>
    public interface ISaveFactory
    {
        /// <summary>
        /// Populate a brand new save with every section the game expects, at its starting values. Variables
        /// already hold their defaults.
        /// </summary>
        void CreateDefaultSave(ISaveDataService saveDataService);

        /// <summary>
        /// Called once an existing save has been read, before the module change, for the game to read back
        /// anything it keeps in its own sections. Persistent memory has already been restored.
        /// </summary>
        void OnSaveLoaded(ISaveDataService saveDataService);
    }
}