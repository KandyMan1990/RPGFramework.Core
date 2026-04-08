namespace RPGFramework.Core.SaveData
{
    public interface ISaveFactory
    {
        void CreateDefaultSave(ISaveDataService saveDataService);
    }
}