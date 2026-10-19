using System;
using System.IO;
using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    /// <summary>The one test of the disk itself: the folder Core keeps files in, here a temporary one.</summary>
    internal sealed class WhenAFileIsWrittenToTheSaveFolder
    {
        private string       m_Folder;
        private ISaveStorage m_Storage;

        [SetUp]
        public void SetUp()
        {
            m_Folder  = Path.Combine(Path.GetTempPath(), "RPGFramework.Core.Tests." + Guid.NewGuid().ToString("N"));
            m_Storage = new FileSaveStorage(m_Folder);

            Directory.CreateDirectory(m_Folder);

            m_Storage.Write(SaveFiles.FILE_NAME, new byte[] { 1, 2, 3 });
            m_Storage.Write(SaveFiles.FILE_NAME, new byte[] { 4, 5 });
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(m_Folder, true);
        }

        [Test]
        public void ThenItReadsBackAsLastWritten()
        {
            CollectionAssert.AreEqual(new byte[] { 4, 5 }, m_Storage.Read(SaveFiles.FILE_NAME));
        }

        [Test]
        public void ThenWritingOverItLeavesNoOtherFile()
        {
            CollectionAssert.AreEqual(new[] { SaveFiles.FILE_NAME }, m_Storage.GetFileNames());
        }

        [Test]
        public void ThenItExistsUntilItIsDeleted()
        {
            Assert.IsTrue(m_Storage.Exists(SaveFiles.FILE_NAME));

            m_Storage.Delete(SaveFiles.FILE_NAME);

            Assert.IsFalse(m_Storage.Exists(SaveFiles.FILE_NAME));
        }
    }
}
