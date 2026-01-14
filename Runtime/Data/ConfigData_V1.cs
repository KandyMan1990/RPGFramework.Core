using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RPGFramework.Core.Data
{

    /// <summary>
    /// Example config data using the settings in Menu.ConfigMenu.  If a custom ConfigMenu is created with more options, just clone this struct with the additional properties added and use it instead
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ConfigData_V1
    {
        public fixed byte  Language[8];
        public       float MusicVolume;
        public       float SfxVolume;
        public       float BattleMessageSpeed;
        public       float FieldMessageSpeed;
        // TODO: control bindings

        public string GetLanguage()
        {
            fixed (byte* ptr = Language)
            {
                int length = 0;
                while (length < 8 && ptr[length] != 0)
                {
                    length++;
                }

                return Encoding.UTF8.GetString(ptr, length);
            }
        }

        public void SetLanguage(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The language string is too long.");
            }

            for (int i = 0; i < bytes.Length; i++)
            {
                Language[i] = bytes[i];
            }

            for (int i = bytes.Length; i < 8; i++)
            {
                Language[i] = 0;
            }
        }
    }
}