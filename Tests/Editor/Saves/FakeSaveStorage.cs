using System;
using System.Collections.Generic;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    /// <summary>Save storage held in memory, every file written at the same moment.</summary>
    internal sealed class FakeSaveStorage : ISaveStorage
    {
        private readonly Dictionary<string, byte[]> m_Files;

        internal FakeSaveStorage(params string[] fileNames)
        {
            m_Files = new Dictionary<string, byte[]>();

            for (int i = 0; i < fileNames.Length; i++)
            {
                m_Files[fileNames[i]] = Array.Empty<byte>();
            }
        }

        internal int Count => m_Files.Count;

        string[] ISaveStorage.GetFileNames()
        {
            string[] names = new string[m_Files.Count];

            m_Files.Keys.CopyTo(names, 0);

            return names;
        }

        bool ISaveStorage.Exists(string fileName)
        {
            bool exists = m_Files.ContainsKey(fileName);

            return exists;
        }

        byte[] ISaveStorage.Read(string fileName)
        {
            byte[] bytes = m_Files[fileName];

            return bytes;
        }

        void ISaveStorage.Write(string fileName, byte[] bytes)
        {
            m_Files[fileName] = bytes;
        }

        void ISaveStorage.Delete(string fileName)
        {
            m_Files.Remove(fileName);
        }

        DateTime ISaveStorage.GetLastWriteTime(string fileName)
        {
            DateTime written = DateTime.MinValue;

            return written;
        }
    }
}
