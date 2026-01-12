namespace RPGFramework.Core.SaveDataService
{
    public interface ISaveFactory
    {
        void CreateDefaultSave(ISaveDataService saveDataService);
    }
}