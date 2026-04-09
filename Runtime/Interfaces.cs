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

    /// <summary>
    /// This service is used to read and write data to persistant and temporary memory.<br /><br />
    /// <see cref="MemoryBank.Global" /> will be written to and loaded from the save file.<br /><br />
    /// <see cref="MemoryBank.Session" /> will be temporary storage for use during the game session.<br /><br />
    /// When creating the args object for the service, remember not to under allocate or overallocate too much memory.<br /><br />
    /// Since global is stored in the save file, over allocating will bloat the save file.
    /// </summary>
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

    /// <summary>
    /// This args object is used to determine how big the global and session memory arrays are in bytes.<br /><br />
    /// Make sure they are big enough to handle the game but not so big that you overallocate memory for the game.
    /// </summary>
    public interface IMemoryServiceArgs
    {
        /// <summary>
        /// How many bytes will be available in the global array (new byte[GlobalBytes])
        /// </summary>
        int GlobalBytes { get; }

        /// <summary>
        /// How many bytes will be available in the session array (new byte[SessionBytes])
        /// </summary>
        int SessionBytes { get; }
    }
}