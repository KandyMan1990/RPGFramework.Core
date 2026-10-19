using System;
using System.IO;

namespace RPGFramework.Core.SaveData
{
    /// <summary>The save and settings files in a folder: the player's data folder, as Core binds it.</summary>
    internal sealed class FileSaveStorage : ISaveStorage
    {
        private const string TEMPORARY_SUFFIX = ".tmp";

        private readonly string m_Folder;

        internal FileSaveStorage(string folder)
        {
            m_Folder = folder;
        }

        string[] ISaveStorage.GetFileNames()
        {
            FileInfo[] files = new DirectoryInfo(m_Folder).GetFiles();
            string[]   names = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                names[i] = files[i].Name;
            }

            return names;
        }

        bool ISaveStorage.Exists(string fileName)
        {
            bool exists = File.Exists(PathOf(fileName));

            return exists;
        }

        byte[] ISaveStorage.Read(string fileName)
        {
            byte[] bytes = File.ReadAllBytes(PathOf(fileName));

            return bytes;
        }

        // Written beside the file and moved over it, so a write cut short leaves the previous file as it was.
        void ISaveStorage.Write(string fileName, byte[] bytes)
        {
            string path      = PathOf(fileName);
            string temporary = path + TEMPORARY_SUFFIX;

            File.WriteAllBytes(temporary, bytes);

            if (File.Exists(path))
            {
                File.Replace(temporary, path, null);
            }
            else
            {
                File.Move(temporary, path);
            }
        }

        void ISaveStorage.Delete(string fileName)
        {
            File.Delete(PathOf(fileName));
        }

        DateTime ISaveStorage.GetLastWriteTime(string fileName)
        {
            DateTime written = File.GetLastWriteTime(PathOf(fileName));

            return written;
        }

        private string PathOf(string fileName)
        {
            string path = Path.Combine(m_Folder, fileName);

            return path;
        }
    }
}
