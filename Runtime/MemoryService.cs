using System;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core
{
    internal sealed class MemoryService : IMemoryService, IMemoryBankAccess
    {
        private readonly byte[]         m_Persistent;
        private readonly byte[]         m_Session;
        private readonly IMemoryService m_This;

        internal MemoryService(IMemoryServiceArgs args)
        {
            m_Persistent = new byte[args.PersistentBytes];
            m_Session    = new byte[args.SessionBytes];
            m_This       = this;
        }

        int IMemoryBankAccess.PersistentByteCount => m_Persistent.Length;

        byte[] IMemoryBankAccess.CopyPersistent()
        {
            byte[] copy = new byte[m_Persistent.Length];

            Buffer.BlockCopy(m_Persistent, 0, copy, 0, m_Persistent.Length);

            return copy;
        }

        void IMemoryBankAccess.RestorePersistent(byte[] source)
        {
            Array.Clear(m_Persistent, 0, m_Persistent.Length);

            if (source == null)
            {
                return;
            }

            int copyLength = Math.Min(source.Length, m_Persistent.Length);

            Buffer.BlockCopy(source, 0, m_Persistent, 0, copyLength);
        }

        void IMemoryBankAccess.ClearPersistent()
        {
            Array.Clear(m_Persistent, 0, m_Persistent.Length);
        }

        void IMemoryBankAccess.ClearSession()
        {
            Array.Clear(m_Session, 0, m_Session.Length);
        }

        byte IMemoryService.ReadByte(MemoryBank bank, ushort address)
        {
            return bank switch
                   {
                       MemoryBank.Persistent => m_Persistent[address],
                       MemoryBank.Session    => m_Session[address],
                       _                     => throw new ArgumentOutOfRangeException(nameof(bank), bank, null)
                   };
        }

        void IMemoryService.WriteByte(MemoryBank bank, ushort address, byte value)
        {
            switch (bank)
            {
                case MemoryBank.Persistent:
                    m_Persistent[address] = value;
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
            return MemoryEncoding.ReadUshort(GetBank(bank), address);
        }

        void IMemoryService.WriteUshort(MemoryBank bank, ushort address, ushort value)
        {
            MemoryEncoding.WriteUshort(GetBank(bank), address, value);
        }

        int IMemoryService.ReadInt(MemoryBank bank, ushort address)
        {
            return MemoryEncoding.ReadInt(GetBank(bank), address);
        }

        void IMemoryService.WriteInt(MemoryBank bank, ushort address, int value)
        {
            MemoryEncoding.WriteInt(GetBank(bank), address, value);
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
            return MemoryEncoding.ReadUlong(GetBank(bank), address);
        }

        void IMemoryService.WriteUlong(MemoryBank bank, ushort address, ulong value)
        {
            MemoryEncoding.WriteUlong(GetBank(bank), address, value);
        }

        private byte[] GetBank(MemoryBank bank)
        {
            return bank switch
                   {
                       MemoryBank.Persistent => m_Persistent,
                       MemoryBank.Session    => m_Session,
                       _                     => throw new ArgumentOutOfRangeException(nameof(bank), bank, null)
                   };
        }
    }
}