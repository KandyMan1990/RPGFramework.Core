namespace RPGFramework.Core.SaveDataService
{
    internal readonly struct SectionBlob
    {
        public readonly uint   Version;
        public readonly byte[] Data;

        public SectionBlob(uint version, byte[] data)
        {
            Version = version;
            Data    = data;
        }
    }
}