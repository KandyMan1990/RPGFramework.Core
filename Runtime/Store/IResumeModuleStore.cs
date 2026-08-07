namespace RPGFramework.Core.Store
{
    public interface IResumeModuleStore
    {
        byte GetModuleId { get; }
        void SetModuleId(byte moduleId);
    }
    
    internal sealed class ResumeModuleStore : IResumeModuleStore
    {
        private byte m_ModuleId;

        byte IResumeModuleStore.GetModuleId => m_ModuleId;

        void IResumeModuleStore.SetModuleId(byte moduleId) => m_ModuleId = moduleId;
    }
}