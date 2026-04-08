using System;
using System.Threading.Tasks;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core
{
    public interface IUpdatable
    {
        void Update();
    }

    public interface IEntryPoint
    {
        Task StartGameAsync<T>(IModuleArgs args) where T : IModule;
    }

    public interface ICoreModule
    {
        Task LoadModuleAsync<T>(IModuleArgs args) where T : IModule;
        Task LoadModuleAsync(Type           type, IModuleArgs args);
        void ResetModule<TInterface, TConcrete>() where TConcrete : TInterface where TInterface : IModule;
        Task ResumeModuleAsync();
    }

    public interface IModuleResumeMap
    {
        Type        GetModuleType(byte moduleId);
        IModuleArgs CreateArgs(byte    moduleId, int arg0, int arg1, int arg2, int arg3);
    }

    public interface IMemoryService
    {
        byte   ReadByte(MemoryBank    bank, ushort address);
        void   WriteByte(MemoryBank   bank, ushort address, byte value);
        bool   ReadBool(MemoryBank    bank, ushort address);
        void   WriteBool(MemoryBank   bank, ushort address, bool value);
        ushort ReadUshort(MemoryBank  bank, ushort address);
        void   WriteUshort(MemoryBank bank, ushort address, ushort value);
        int    ReadInt(MemoryBank     bank, ushort address);
        void   WriteInt(MemoryBank    bank, ushort address, int value);
        float  ReadFloat(MemoryBank   bank, ushort address);
        void   WriteFloat(MemoryBank  bank, ushort address, float value);
        ulong  ReadUlong(MemoryBank   bank, ushort address);
        void   WriteUlong(MemoryBank  bank, ushort address, ulong value);
    }
}