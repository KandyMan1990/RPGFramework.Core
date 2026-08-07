namespace RPGFramework.Core.Store
{
    public interface IChangeModuleStore
    {
        byte GetModuleId { get; }
        void SetModuleId(byte moduleId);
    }

    internal sealed class ChangeModuleStore : IChangeModuleStore
    {
        private byte m_ModuleId;

        byte IChangeModuleStore.GetModuleId => m_ModuleId;

        void IChangeModuleStore.SetModuleId(byte moduleId) => m_ModuleId = moduleId;
    }
}