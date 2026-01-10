using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using RPGFramework.Core.Helpers;
using UnityEngine;

namespace RPGFramework.Core.GlobalConfig
{
    public interface IGlobalConfig
    {
        bool TryGetSection<T>(string key, uint expectedVersion, out T value, out uint storedVersion) where T : unmanaged;
        void SetSection<T>(string    key, uint version,         T     value) where T : unmanaged;
    }

    internal sealed class ConfigSection
    {
        public uint   Version;
        public byte[] Data;
    }

    public sealed class GlobalConfig : IGlobalConfig
    {
        private const string FILENAME = "global.config";

        private readonly string                           m_Path;
        private          Dictionary<ulong, ConfigSection> m_Sections;

        public GlobalConfig()
        {
            m_Path = Path.Combine(Application.persistentDataPath, FILENAME);

            Load();
        }

        bool IGlobalConfig.TryGetSection<T>(string key, uint expectedVersion, out T value, out uint storedVersion)
        {
            ulong keyHash = GetKeyHash(key);
            if (m_Sections.TryGetValue(keyHash, out ConfigSection section))
            {
                storedVersion = section.Version;
                value         = MemoryMarshal.Read<T>(section.Data);
                return true;
            }

            storedVersion = 0;
            value         = default;

            return false;
        }

        unsafe void IGlobalConfig.SetSection<T>(string key, uint version, T value)
        {
            ulong  keyHash = GetKeyHash(key);
            byte[] buffer  = new byte[sizeof(T)];
            MemoryMarshal.Write(buffer, ref value);

            m_Sections[keyHash] = new ConfigSection
                                  {
                                          Version = version,
                                          Data    = buffer
                                  };

            Save();
        }

        private void Load()
        {
            m_Sections = new Dictionary<ulong, ConfigSection>();

            if (!File.Exists(m_Path))
            {
                return;
            }

            using BinaryReader reader = new BinaryReader(File.OpenRead(m_Path));

            int sectionCount = reader.ReadInt32();
            for (int i = 0; i < sectionCount; i++)
            {
                ulong  keyHash = reader.ReadUInt64();
                uint   version = reader.ReadUInt32();
                int    size    = reader.ReadInt32();
                byte[] data    = reader.ReadBytes(size);

                m_Sections[keyHash] = new ConfigSection
                                      {
                                              Version = version,
                                              Data    = data
                                      };
            }
        }

        private void Save()
        {
            using BinaryWriter writer = new BinaryWriter(File.Create(m_Path));

            writer.Write(m_Sections.Count);
            foreach (KeyValuePair<ulong, ConfigSection> pair in m_Sections)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value.Version);
                writer.Write(pair.Value.Data.Length);
                writer.Write(pair.Value.Data);
            }
        }

        private static ulong GetKeyHash(string key) => Fnv1a64.Hash(key);
    }
}