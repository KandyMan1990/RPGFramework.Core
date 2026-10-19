using System.Collections.Generic;
using System.Threading.Tasks;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    /// <summary>A host whose loads and unloads complete at once, writing what it is asked to do, and its modules', to a log.</summary>
    internal sealed class FakeModuleHost : IModuleHost
    {
        private readonly List<string> m_Log;

        internal FakeModuleHost()
        {
            m_Log = new List<string>();
        }

        internal IReadOnlyList<string> Log => m_Log;

        internal void ClearLog()
        {
            m_Log.Clear();
        }

        Task<StackedModule> IModuleHost.LoadAsync(byte moduleId, bool alone)
        {
            m_Log.Add($"load {moduleId} {(alone ? "alone" : "beside")}");

            StackedModule stacked = new StackedModule(moduleId, new FakeModule(moduleId, m_Log), default, null);

            return Task.FromResult(stacked);
        }

        Task IModuleHost.UnloadAsync(StackedModule module, bool unloadScene)
        {
            m_Log.Add($"unload {module.ModuleId} {(unloadScene ? "scene" : "container")}");

            return Task.CompletedTask;
        }

        void IModuleHost.MakeCurrent(StackedModule module)
        {
            m_Log.Add($"current {module.ModuleId}");
        }

        Task IModuleHost.CoverAsync(ModuleChange change)
        {
            m_Log.Add("cover");

            return Task.CompletedTask;
        }

        Task IModuleHost.RevealAsync(ModuleChange change)
        {
            m_Log.Add("reveal");

            return Task.CompletedTask;
        }

        private sealed class FakeModule : IModule
        {
            private readonly byte         m_ModuleId;
            private readonly List<string> m_Log;

            internal FakeModule(byte moduleId, List<string> log)
            {
                m_ModuleId = moduleId;
                m_Log      = log;
            }

            Task IModule.OnEnterAsync(byte entry)
            {
                return Record($"enter {entry}");
            }

            Task IModule.OnExitAsync()
            {
                return Record("exit");
            }

            Task IModule.OnSuspendAsync()
            {
                return Record("suspend");
            }

            Task IModule.OnResumeAsync()
            {
                return Record("resume");
            }

            void IModule.OnRevealed()
            {
                Record("revealed");
            }

            private Task Record(string call)
            {
                m_Log.Add($"{m_ModuleId} {call}");

                return Task.CompletedTask;
            }
        }
    }
}
