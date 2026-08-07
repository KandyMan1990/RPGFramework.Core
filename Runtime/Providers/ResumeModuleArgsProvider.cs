namespace RPGFramework.Core.Providers
{
    public interface IResumeModuleArgsProvider
    {
        byte GetModuleIdToResume { get; }
        void SetModuleIdToResume(byte moduleId);
    }
    
    internal sealed class ResumeModuleArgsProvider : IResumeModuleArgsProvider
    {
        private byte m_ModuleId;

        byte IResumeModuleArgsProvider.GetModuleIdToResume => m_ModuleId;

        void IResumeModuleArgsProvider.SetModuleIdToResume(byte moduleId)
        {
            m_ModuleId = moduleId;
        }
    }
}