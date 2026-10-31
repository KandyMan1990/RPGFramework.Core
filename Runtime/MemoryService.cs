using System;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core
{
    internal sealed class MemoryService : IMemoryService, IMemoryBankAccess
    {
        private readonly byte[]         m_Persistent;
        private readonly byte[]         m_Session;
        private readonly byte[]         m_Settings;
        private readonly IMemoryService m_This;

        internal MemoryService(IMemoryServiceArgs args)
        {
            m_Persistent = new byte[args.PersistentBytes];
            m_Session    = new byte[args.SessionBytes];
            m_Settings   = new byte[args.SettingsBytes];
            m_This       = this;
        }

        int IMemoryBankAccess.GetByteCount(MemoryBank bank) => GetBank(bank).Length;

        byte[] IMemoryBankAccess.Copy(MemoryBank bank)
        {
            byte[] source = GetBank(bank);
            byte[] copy   = new byte[source.Length];

            Buffer.BlockCopy(source, 0, copy, 0, source.Length);

            return copy;
        }

        void IMemoryBankAccess.Restore(MemoryBank bank, byte[] source)
        {
            byte[] target = GetBank(bank);

            Array.Clear(target, 0, target.Length);

            if (source == null)
            {
                return;
            }

            Buffer.BlockCopy(source, 0, target, 0, Math.Min(source.Length, target.Length));
        }

        void IMemoryBankAccess.Clear(MemoryBank bank)
        {
            byte[] target = GetBank(bank);

            Array.Clear(target, 0, target.Length);
        }

        byte IMemoryService.ReadByte(MemoryBank bank, ushort address)
        {
            return GetBank(bank)[address];
        }

        void IMemoryService.WriteByte(MemoryBank bank, ushort address, byte value)
        {
            GetBank(bank)[address] = value;
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
                       MemoryBank.Settings   => m_Settings,
                       _                     => throw new ArgumentOutOfRangeException(nameof(bank), bank, null)
                   };
        }
    }
}