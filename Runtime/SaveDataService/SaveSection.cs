using System.Runtime.InteropServices;

namespace RPGFramework.Core.SaveDataService
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SaveSection<T> where T : unmanaged
    {
        public uint Version;
        public T    Data;

        public SaveSection(uint version, T data)
        {
            Version = version;
            Data    = data;
        }
    }
}