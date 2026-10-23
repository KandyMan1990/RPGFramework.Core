using System.Threading.Tasks;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core
{
    /// <summary>
    /// Ticked every frame while registered with <see cref="PlayerLoop.UpdateManager" />, so a module owns its own
    /// update and stops it by unregistering.
    /// </summary>
    public interface IUpdatable
    {
        /// <summary>Called once a frame, in the order things registered.</summary>
        void Update();
    }

    /// <summary>
    /// Ticked every physics step while registered with <see cref="PlayerLoop.UpdateManager" />.
    /// </summary>
    public interface IFixedUpdatable
    {
        /// <summary>Called once a physics step, in the order things registered.</summary>
        void FixedUpdate();
    }

    /// <summary>
    /// Switches between the game's modules: enters the first, and carries out each change the game's module router
    /// decides on as a module reports what happened.
    /// </summary>
    public interface ICoreModule
    {
        /// <summary>
        /// Enters the game's first module, the one <see cref="CoreModuleFactory.Create" /> was given.
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Tells Core what happened in the module on top — an outcome its shared types define — and carries out the
        /// change the game's <see cref="IModuleRouter" /> decides on.
        /// </summary>
        Task RequestModuleChangeAsync(byte outcome);
    }

    /// <summary>
    /// The scene a module runs in, for finding its own objects: other modules' scenes can be loaded beside it.
    /// </summary>
    public interface IModuleScene
    {
        /// <summary>The first component of the type in the scene, or null.</summary>
        T Find<T>() where T : Component;
    }

    /// <summary>
    /// The game's variables, read and written by address in a bank: <see cref="MemoryBank.Persistent" />, which a save
    /// holds, and <see cref="MemoryBank.Session" />, which lasts until the game closes. Each value is held lowest byte
    /// first, at the offset the variable map gives it.
    /// </summary>
    public interface IMemoryService
    {
        /// <summary>The byte at the address in the bank.</summary>
        byte ReadByte(MemoryBank bank, ushort address);

        /// <summary>Writes a byte at the address in the bank.</summary>
        void WriteByte(MemoryBank bank, ushort address, byte value);

        /// <summary>The byte at the address as a bool: anything but zero is true.</summary>
        bool ReadBool(MemoryBank bank, ushort address);

        /// <summary>Writes a bool at the address as one byte, 1 or 0.</summary>
        void WriteBool(MemoryBank bank, ushort address, bool value);

        /// <summary>The two bytes from the address as a ushort, lowest first.</summary>
        ushort ReadUshort(MemoryBank bank, ushort address);

        /// <summary>Writes a ushort's two bytes from the address, lowest first.</summary>
        void WriteUshort(MemoryBank bank, ushort address, ushort value);

        /// <summary>The four bytes from the address as an int, lowest first.</summary>
        int ReadInt(MemoryBank bank, ushort address);

        /// <summary>Writes an int's four bytes from the address, lowest first.</summary>
        void WriteInt(MemoryBank bank, ushort address, int value);

        /// <summary>The four bytes from the address as a float's bits, lowest first.</summary>
        float ReadFloat(MemoryBank bank, ushort address);

        /// <summary>Writes a float's four bytes from the address, lowest first.</summary>
        void WriteFloat(MemoryBank bank, ushort address, float value);

        /// <summary>The eight bytes from the address as a ulong, lowest first, as a name hash is kept.</summary>
        ulong ReadUlong(MemoryBank bank, ushort address);

        /// <summary>Writes a ulong's eight bytes from the address, lowest first.</summary>
        void WriteUlong(MemoryBank bank, ushort address, ulong value);
    }

    /// <summary>How big the persistent and session banks are, in bytes: as much as the variable map lays out.</summary>
    public interface IMemoryServiceArgs
    {
        /// <summary>The persistent bank's size, which every save holds whole.</summary>
        int PersistentBytes { get; }

        /// <summary>The session bank's size.</summary>
        int SessionBytes { get; }
    }

    /// <summary>
    /// How big each running script's <see cref="Memory.TempMemory" /> is.
    /// </summary>
    public interface ITempMemoryArgs
    {
        /// <summary>
        /// How many bytes one script's temp memory holds. Each running script has its own, so this only has to
        /// fit the working values one script holds. It is never saved.
        /// </summary>
        int TempBytes { get; }
    }
}