using System;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core
{
    internal sealed class MemoryService : IMemoryService
    {
        private readonly byte[]         m_Global;
        private readonly byte[]         m_Session;
        private readonly IMemoryService m_This;

        private object m_TempModuleData;

        internal MemoryService(IMemoryServiceArgs args)
        {
            m_Global  = new byte[args.GlobalBytes];
            m_Session = new byte[args.SessionBytes];
            m_This    = this;
        }

        byte IMemoryService.ReadByte(MemoryBank bank, ushort address)
        {
            return bank switch
                   {
                           MemoryBank.Global  => m_Global[address],
                           MemoryBank.Session => m_Session[address],
                           _                  => throw new InvalidOperationException()
                   };
        }

        void IMemoryService.WriteByte(MemoryBank bank, ushort address, byte value)
        {
            switch (bank)
            {
                case MemoryBank.Global:
                    m_Global[address] = value;
                    break;
                case MemoryBank.Session:
                    m_Session[address] = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bank), bank, null);
            }
        }

        bool IMemoryService.ReadBool(MemoryBank bank, ushort address)
        {
            return m_This.ReadByte(bank, address) != 0;
        }

        void IMemoryService.WriteBool(MemoryBank bank, ushort address, bool value)
        {
            m_This.WriteByte(bank, address, value ? (byte)1 : (byte)0);
        }

        ushort IMemoryService.ReadUshort(MemoryBank bank, ushort address)
        {
            byte[] mem = GetBank(bank);

            return (ushort)(mem[address] | mem[address + 1] << 8);
        }

        void IMemoryService.WriteUshort(MemoryBank bank, ushort address, ushort value)
        {
            byte[] mem = GetBank(bank);

            mem[address]     = (byte)(value & 0xFF);
            mem[address + 1] = (byte)(value >> 8);
        }

        int IMemoryService.ReadInt(MemoryBank bank, ushort address)
        {
            byte[] mem = GetBank(bank);

            return mem[address]           |
                   mem[address + 1] << 8  |
                   mem[address + 2] << 16 |
                   mem[address + 3] << 24;
        }

        void IMemoryService.WriteInt(MemoryBank bank, ushort address, int value)
        {
            byte[] mem = GetBank(bank);

            mem[address]     = (byte)(value       & 0xFF);
            mem[address + 1] = (byte)(value >> 8  & 0xFF);
            mem[address + 2] = (byte)(value >> 16 & 0xFF);
            mem[address + 3] = (byte)(value >> 24 & 0xFF);
        }

        float IMemoryService.ReadFloat(MemoryBank bank, ushort address)
        {
            int raw = m_This.ReadInt(bank, address);
            return BitConverter.Int32BitsToSingle(raw);
        }

        void IMemoryService.WriteFloat(MemoryBank bank, ushort address, float value)
        {
            int raw = BitConverter.SingleToInt32Bits(value);
            m_This.WriteInt(bank, address, raw);
        }

        ulong IMemoryService.ReadUlong(MemoryBank bank, ushort address)
        {
            byte[] mem = GetBank(bank);

            return mem[address]                  |
                   (ulong)mem[address + 1] << 8  |
                   (ulong)mem[address + 2] << 16 |
                   (ulong)mem[address + 3] << 24 |
                   (ulong)mem[address + 4] << 32 |
                   (ulong)mem[address + 5] << 40 |
                   (ulong)mem[address + 6] << 48 |
                   (ulong)mem[address + 7] << 56;
        }

        void IMemoryService.WriteUlong(MemoryBank bank, ushort address, ulong value)
        {
            byte[] mem = GetBank(bank);

            mem[address]     = (byte)(value       & 0xFF);
            mem[address + 1] = (byte)(value >> 8  & 0xFF);
            mem[address + 2] = (byte)(value >> 16 & 0xFF);
            mem[address + 3] = (byte)(value >> 24 & 0xFF);
            mem[address + 4] = (byte)(value >> 32 & 0xFF);
            mem[address + 5] = (byte)(value >> 40 & 0xFF);
            mem[address + 6] = (byte)(value >> 48 & 0xFF);
            mem[address + 7] = (byte)(value >> 56 & 0xFF);
        }

        T IMemoryService.GetTempModuleData<T>()
        {
            if (m_TempModuleData == null)
            {
                throw new NullReferenceException($"{nameof(IMemoryService)}::{nameof(IMemoryService.GetTempModuleData)} Temporary data not set");
            }

            object data = m_TempModuleData;
            m_TempModuleData = null;

            return (T)data;
        }

        void IMemoryService.SetTempModuleData(object data)
        {
            m_TempModuleData = data;
        }

        private byte[] GetBank(MemoryBank bank)
        {
            return bank switch
                   {

                           MemoryBank.Global  => m_Global,
                           MemoryBank.Session => m_Session,
                           _                  => throw new ArgumentOutOfRangeException(nameof(bank), bank, null)
                   };
        }
    }
}