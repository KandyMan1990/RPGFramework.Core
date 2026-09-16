namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// One running script's scratch memory, addressed the way the variable map lays out its temp variables.
    /// It starts zeroed and belongs to that script alone, so a script interrupted by another gets its values
    /// back untouched.
    /// </summary>
    public sealed class TempMemory
    {
        private readonly byte[] m_Bytes;

        public TempMemory(int byteCount)
        {
            m_Bytes = new byte[byteCount];
        }

        public byte ReadByte(ushort address)
        {
            byte value = m_Bytes[address];

            return value;
        }

        public void WriteByte(ushort address, byte value)
        {
            m_Bytes[address] = value;
        }

        public bool ReadBool(ushort address)
        {
            bool value = m_Bytes[address] != 0;

            return value;
        }

        public void WriteBool(ushort address, bool value)
        {
            m_Bytes[address] = value ? (byte)1 : (byte)0;
        }

        public ushort ReadUshort(ushort address)
        {
            ushort value = MemoryEncoding.ReadUshort(m_Bytes, address);

            return value;
        }

        public void WriteUshort(ushort address, ushort value)
        {
            MemoryEncoding.WriteUshort(m_Bytes, address, value);
        }

        public int ReadInt(ushort address)
        {
            int value = MemoryEncoding.ReadInt(m_Bytes, address);

            return value;
        }

        public void WriteInt(ushort address, int value)
        {
            MemoryEncoding.WriteInt(m_Bytes, address, value);
        }

        public float ReadFloat(ushort address)
        {
            float value = System.BitConverter.Int32BitsToSingle(MemoryEncoding.ReadInt(m_Bytes, address));

            return value;
        }

        public void WriteFloat(ushort address, float value)
        {
            MemoryEncoding.WriteInt(m_Bytes, address, System.BitConverter.SingleToInt32Bits(value));
        }

        public ulong ReadUlong(ushort address)
        {
            ulong value = MemoryEncoding.ReadUlong(m_Bytes, address);

            return value;
        }

        public void WriteUlong(ushort address, ulong value)
        {
            MemoryEncoding.WriteUlong(m_Bytes, address, value);
        }
    }
}