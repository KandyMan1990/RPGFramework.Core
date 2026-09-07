namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// The game's save policy. The framework reads and writes the file; what a save <i>means</i> — which
    /// sections exist, and where a loaded one resumes — belongs to the game.
    /// </summary>
    public interface ISaveFactory
    {
        /// <summary>
        /// Populate a brand new save: every section the game expects, at its starting values, and the
        /// module and arguments a new game begins with.
        /// </summary>
        void CreateDefaultSave(ISaveDataService saveDataService);

        /// <summary>
        /// Called once an existing save has been read, before the module change. The game reads back
        /// whatever it needs from the loaded sections and from
        /// <see cref="RPGFramework.Core.SharedTypes.MemoryBank.Global" />, then sets the module to resume
        /// into and its arguments.
        /// </summary>
        void OnSaveLoaded(ISaveDataService saveDataService);
    }
}