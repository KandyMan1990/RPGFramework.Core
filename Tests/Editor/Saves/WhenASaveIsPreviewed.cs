using System;
using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASaveIsPreviewed
    {
        private SavePreview m_Preview;

        // The service lays a save out as the current map does before a preview reads it.
        [SetUp]
        public void SetUp()
        {
            TestVariableMap map = new TestVariableMap().Add("Count", MemoryBank.Persistent, VariableWidth.Int, 4, VariableDefaults.FromInteger(-7, VariableWidth.Int))
                                                       .Add("Speed", MemoryBank.Persistent, VariableWidth.Float, 8, VariableDefaults.FromFloat(2.5f))
                                                       .Add("Field", MemoryBank.Persistent, VariableWidth.ULong, 16, 0xDEADBEEFCAFEF00DUL);

            byte[] persistent = new byte[24];

            BitConverter.GetBytes(42).CopyTo(persistent, 4);
            BitConverter.GetBytes(2.5f).CopyTo(persistent, 8);

            m_Preview = new SavePreview("save000.sav", DateTime.MinValue, persistent, map, SaveState.FromNewerVersion);
        }

        [Test]
        public void ThenItReadsWhatTheSaveHolds()
        {
            Assert.AreEqual(42, m_Preview.Read<int>("Count"));
            Assert.AreEqual(2.5f, m_Preview.Read<float>("Speed"));
        }

        [Test]
        public void ThenItSaysItIsFromANewerVersion()
        {
            Assert.IsTrue(m_Preview.IsFromNewerVersion);
        }

        [Test]
        public void ThenItCannotBeLoaded()
        {
            Assert.IsFalse(m_Preview.CanLoad);
        }
    }
}
