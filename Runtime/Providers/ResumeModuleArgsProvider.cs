namespace RPGFramework.Core.Providers
{
    public interface IResumeModuleArgsProvider
    {
        byte GetModuleToResume { get; }
        void SetModuleToResume(byte moduleId);
    }
    
    internal sealed class ResumeModuleArgsProvider : IResumeModuleArgsProvider
    {
        private byte m_ModuleId;

        byte IResumeModuleArgsProvider.GetModuleToResume => m_ModuleId;

        void IResumeModuleArgsProvider.SetModuleToResume(byte moduleId)
        {
            m_ModuleId = moduleId;
        }
    }
}