using System;

namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// Where the save and settings files are kept: the player's data folder on a desktop, and on a console the
    /// platform's own save storage, which item 117 will let a platform supply. Files are named, side by side, and
    /// written whole.
    /// </summary>
    internal interface ISaveStorage
    {
        /// <summary>The name of every file it holds, saves or not.</summary>
        string[] GetFileNames();

        bool Exists(string fileName);

        byte[] Read(string fileName);

        /// <summary>Writes the file whole: a write cut short leaves the file as it was.</summary>
        void Write(string fileName, byte[] bytes);

        void Delete(string fileName);

        DateTime GetLastWriteTime(string fileName);
    }
}
