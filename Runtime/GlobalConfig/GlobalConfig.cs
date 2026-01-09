using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RPGFramework.Core.GlobalConfig
{
    public interface IGlobalConfig
    {
        /// <summary>
        /// Loads a config file from the system using the provided struct for the format
        /// </summary>
        /// <typeparam name="T">Must be a blittable type</typeparam>
        /// <returns>A config file of type T</returns>
        bool TryGet<T>(out T value) where T : unmanaged;
        /// <summary>
        /// Saves a config file to the system
        /// </summary>
        /// <param name="value">The data to be saved to disk</param>
        /// <typeparam name="T">Must be a blittable type</typeparam>
        /// <returns></returns>
        void Set<T>(T value) where T : unmanaged;
    }

    public class GlobalConfig : IGlobalConfig
    {
        private const string FILENAME = "global.config";

        private readonly string m_Path = Path.Combine(Application.persistentDataPath, FILENAME);

        private byte[] m_Data;

        bool IGlobalConfig.TryGet<T>(out T value)
        {
            if (m_Data == null)
            {
                if (!File.Exists(m_Path))
                {
                    value = default;
                    return false;
                }

                m_Data = File.ReadAllBytes(m_Path);
            }

            value = MemoryMarshal.Read<T>(m_Data);
            return true;
        }

        unsafe void IGlobalConfig.Set<T>(T value)
        {
            m_Data = new byte[sizeof(T)];
            MemoryMarshal.Write(m_Data, ref value);

            File.WriteAllBytes(m_Path, m_Data);
        }
    }
}