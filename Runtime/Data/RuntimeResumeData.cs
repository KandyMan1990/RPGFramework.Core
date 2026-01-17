using System.Runtime.InteropServices;

namespace RPGFramework.Core.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RuntimeResumeData
    {
        public byte ModuleId;
        public int  Arg0;
        public int  Arg1;
        public int  Arg2;
        public int  Arg3;
    }
}