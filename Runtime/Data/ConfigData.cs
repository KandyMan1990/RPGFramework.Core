using System.Runtime.InteropServices;
using System.Text;

namespace RPGFramework.Core.Data
{
    /// <summary>
    /// Example config data using the settings in Menu.ConfigMenu.  If a custom ConfigMenu is created with more options, just clone this struct with the additional properties added and use it instead
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ConfigData
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

                return Encoding.ASCII.GetString(ptr, length);
            }
        }

        public void SetLanguage(string value)
        {
            for (int i = 0; i < 8; i++)
            {
                Language[i] = i < value.Length ? (byte)value[i] : (byte)0;
            }
        }
    }
}