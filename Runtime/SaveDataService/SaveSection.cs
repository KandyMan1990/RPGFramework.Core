using System.Runtime.InteropServices;

namespace RPGFramework.Core.SaveDataService
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SaveSection<T> where T : unmanaged
    {
        public uint  Version;
        public T     Data;
    }
}