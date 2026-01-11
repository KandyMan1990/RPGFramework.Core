using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGFramework.Core.SaveDataService
{
    public interface ISaveDataService
    {
        object  CreateDefaultSaveFile();
        Task<T> LoadAsync<T>(string fileName) where T : unmanaged;
        Task    SaveAsync<T>(string fileName, T data) where T : unmanaged;
        object  GetCurrentSaveFile();
        void    SetCurrentSaveFile(object saveFile);
        // TODO: Task<string[]> GetListOfSaveFilesAsync();
    }

    public class SaveDataService : ISaveDataService
    {
        private readonly ISaveFactory m_SaveFactory;

        private object m_CurrentSaveFile;

        public SaveDataService(ISaveFactory saveFactory)
        {
            m_SaveFactory = saveFactory;
        }

        object ISaveDataService.CreateDefaultSaveFile() => m_SaveFactory.CreateDefaultSaveFile();

        async Task<T> ISaveDataService.LoadAsync<T>(string fileName)
        {
            string path  = Path.Combine(Application.persistentDataPath, fileName);
            byte[] bytes = await File.ReadAllBytesAsync(path);

            return MemoryMarshal.Read<T>(bytes);
        }

        Task ISaveDataService.SaveAsync<T>(string fileName, T data)
        {
            string path  = Path.Combine(Application.persistentDataPath, fileName);
            byte[] bytes = DataToBytes(data);

            return File.WriteAllBytesAsync(path, bytes);
        }

        object ISaveDataService.GetCurrentSaveFile() => m_CurrentSaveFile;

        void ISaveDataService.SetCurrentSaveFile(object saveFile) => m_CurrentSaveFile = saveFile;

        private static unsafe byte[] DataToBytes<T>(T data) where T : unmanaged
        {
            byte[] bytes = new byte[sizeof(T)];
            MemoryMarshal.Write(bytes, ref data);

            return bytes;
        }

    }
}