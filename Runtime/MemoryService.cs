using System;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core
{
    internal sealed class MemoryService : IMemoryService, IMemoryBankAccess
    {
        private readonly byte[]         m_Global;
        private readonly byte[]         m_Session;
        private readonly byte[]         m_Temp;
        private readonly IMemoryService m_This;

        internal MemoryService(IMemoryServiceArgs args)
        {
            m_Global  = new byte[args.GlobalBytes];
            m_Session = new byte[args.SessionBytes];
            m_Temp    = new byte[args.TempBytes];
            m_This    = this;
        }

        int IMemoryBankAccess.GlobalByteCount => m_Global.Length;

        void IMemoryService.ClearTemp()
        {
            Array.Clear(m_Temp, 0, m_Temp.Length);
        }

        byte[] IMemoryBankAccess.CopyGlobal()
        {
            byte[] copy = new byte[m_Global.Length];

            Buffer.BlockCopy(m_Global, 0, copy, 0, m_Global.Length);

            return copy;
        }

        void IMemoryBankAccess.RestoreGlobal(byte[] source)
        {
            Array.Clear(m_Global, 0, m_Global.Length);

            if (source == null)
            {
                return;
            }

            int copyLength = Math.Min(source.Length, m_Global.Length);

            Buffer.BlockCopy(source, 0, m_Global, 0, copyLength);
        }

        void IMemoryBankAccess.ClearGlobal()
        {
            Array.Clear(m_Global, 0, m_Global.Length);
        }

        void IMemoryBankAccess.ClearSession()
        {
            Array.Clear(m_Session, 0, m_Session.Length);
        }

        byte IMemoryService.ReadByte(MemoryBank bank, ushort address)
        {
            return bank switch
                   {
                       MemoryBank.Global  => m_Global[address],
                       MemoryBank.Session => m_Session[address],
                       MemoryBank.Temp    => m_Temp[address],
                       _                  => throw new ArgumentOutOfRangeException(nameof(bank), bank, null)
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
                case MemoryBank.Temp:
                    m_Temp[address] = value;
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

        private byte[] GetBank(MemoryBank bank)
        {
            return bank switch
                   {
                       MemoryBank.Global  => m_Global,
                       MemoryBank.Session => m_Session,
                       MemoryBank.Temp    => m_Temp,
                       _                  => throw new ArgumentOutOfRangeException(nameof(bank), bank, null)
                   };
        }
    }
}