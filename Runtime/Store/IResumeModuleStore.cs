namespace RPGFramework.Core.Store
{
    public interface IResumeModuleStore
    {
        byte ModuleId { get; }
        void SetModuleId(byte moduleId);
    }

    internal sealed class ResumeModuleStore : IResumeModuleStore
    {
        private byte m_ModuleId;

        byte IResumeModuleStore.ModuleId => m_ModuleId;

        void IResumeModuleStore.SetModuleId(byte moduleId) => m_ModuleId = moduleId;
    }
}