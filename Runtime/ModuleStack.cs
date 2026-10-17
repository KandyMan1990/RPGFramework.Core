using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.SharedTypes;
using RPGFramework.DI;
using UnityEngine.SceneManagement;

namespace RPGFramework.Core
{
    /// <summary>
    /// Loads and unloads modules' scenes and containers, for <see cref="ModuleStack" />, which decides when.
    /// </summary>
    internal interface IModuleHost
    {
        /// <summary>
        /// Loads the module's scene — alone, unloading every other, or beside them — builds its container, makes it the
        /// current resolver, and resolves the module.
        /// </summary>
        Task<StackedModule> LoadAsync(byte moduleId, bool alone);

        /// <summary>Disposes the module's container, and unloads its scene unless a load alone already has.</summary>
        Task UnloadAsync(StackedModule module, bool unloadScene);

        /// <summary>Makes the module's container the current resolver again, once the one over it has gone.</summary>
        void MakeCurrent(StackedModule module);

        /// <summary>Covers the screen as the change asks, before it touches any module.</summary>
        Task CoverAsync(ModuleChange change);

        /// <summary>Reveals the screen as the change asks, once it has entered or resumed its module.</summary>
        Task RevealAsync(ModuleChange change);
    }

    /// <summary>A module on the stack, with the scene and container its host loaded it into.</summary>
    internal sealed class StackedModule
    {
        internal readonly byte         ModuleId;
        internal readonly IModule      Module;
        internal readonly Scene        Scene;
        internal readonly IDIContainer Container;

        internal StackedModule(byte moduleId, IModule module, Scene scene, IDIContainer container)
        {
            ModuleId  = moduleId;
            Module    = module;
            Scene     = scene;
            Container = container;
        }
    }

    /// <summary>
    /// The modules loaded. Carries out a change in a fixed order, under a covered screen: a module suspended is told
    /// before anything over it exists, and resumed only once what was over it has gone; the module on top is told it is
    /// on screen once it has been revealed.
    /// </summary>
    internal sealed class ModuleStack
    {
        private readonly IModuleHost          m_Host;
        private readonly Stack<StackedModule> m_Modules;

        internal ModuleStack(IModuleHost host)
        {
            m_Host    = host;
            m_Modules = new Stack<StackedModule>();
        }

        /// <summary>The modules loaded, the top one first.</summary>
        internal IReadOnlyCollection<StackedModule> Modules => m_Modules;

        internal byte TopModuleId
        {
            get
            {
                StackedModule top      = Top();
                byte          moduleId = top.ModuleId;

                return moduleId;
            }
        }

        internal async Task ApplyAsync(ModuleChange change)
        {
            if (change.Kind == ModuleChangeKind.Close && m_Modules.Count < 2)
            {
                throw new InvalidOperationException($"{nameof(ModuleStack)}::{nameof(ApplyAsync)} Nothing is under module [{TopModuleId}] to go back to");
            }

            await m_Host.CoverAsync(change);

            Task applying = change.Kind switch
                            {
                                ModuleChangeKind.Over    => OverAsync(change),
                                ModuleChangeKind.Close   => CloseAsync(),
                                ModuleChangeKind.Replace => m_Modules.Count > 1 ? ReplaceTopAsync(change) : ClearAsync(change),
                                ModuleChangeKind.Clear   => ClearAsync(change),
                                _                        => throw new ArgumentOutOfRangeException(nameof(change), change.Kind, null)
                            };

            await applying;

            await m_Host.RevealAsync(change);

            Top().Module.OnRevealed();
        }

        private async Task OverAsync(ModuleChange change)
        {
            await Top().Module.OnSuspendAsync();

            StackedModule opened = await m_Host.LoadAsync(change.ModuleId, false);
            m_Modules.Push(opened);

            await opened.Module.OnEnterAsync(change.Entry);
        }

        private async Task CloseAsync()
        {
            StackedModule closed = Top();

            await closed.Module.OnExitAsync();

            m_Modules.Pop();
            await m_Host.UnloadAsync(closed, true);

            StackedModule resumed = Top();

            m_Host.MakeCurrent(resumed);
            await resumed.Module.OnResumeAsync();
        }

        private async Task ReplaceTopAsync(ModuleChange change)
        {
            StackedModule replaced = Top();

            await replaced.Module.OnExitAsync();

            m_Modules.Pop();
            await m_Host.UnloadAsync(replaced, true);

            StackedModule entered = await m_Host.LoadAsync(change.ModuleId, false);
            m_Modules.Push(entered);

            await entered.Module.OnEnterAsync(change.Entry);
        }

        private async Task ClearAsync(ModuleChange change)
        {
            foreach (StackedModule module in m_Modules)
            {
                await module.Module.OnExitAsync();
            }

            StackedModule entered = await m_Host.LoadAsync(change.ModuleId, true);

            while (m_Modules.Count > 0)
            {
                await m_Host.UnloadAsync(m_Modules.Pop(), false);
            }

            m_Modules.Push(entered);

            await entered.Module.OnEnterAsync(change.Entry);
        }

        private StackedModule Top()
        {
            if (m_Modules.Count == 0)
            {
                throw new InvalidOperationException($"{nameof(ModuleStack)}::{nameof(Top)} No module is loaded");
            }

            StackedModule top = m_Modules.Peek();

            return top;
        }
    }
}