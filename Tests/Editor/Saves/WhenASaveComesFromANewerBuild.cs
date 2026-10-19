using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASaveComesFromANewerBuild
    {
        [Test]
        public void ThenAVariableIdTheMapNeverGaveSaysSo()
        {
            Assert.IsTrue(PersistentMigration.IsFromNewerBuild(SaveLayouts.Of(SaveLayouts.WithAVariableFromLater()), SaveLayouts.Version2()));
        }

        [Test]
        public void ThenAFieldIdTheRecordNeverGaveSaysSo()
        {
            VariableRecordField[] fields =
            {
                SaveLayouts.Field("level", VariableWidth.Byte, 1, 0, 2),
                SaveLayouts.Field("later", VariableWidth.Byte, 1, 0, 5)
            };

            TestVariableMap moreFields = new TestVariableMap().AddRecord("party", MemoryBank.Persistent, 0, 1, fields, 6);

            Assert.IsTrue(PersistentMigration.IsFromNewerBuild(SaveLayouts.Of(moreFields), SaveLayouts.Version2()));
        }

        [Test]
        public void ThenANewerDataVersionSaysSo()
        {
            byte[] layout = PersistentLayout.Write(SaveLayouts.Version2());

            layout[0] = 2;

            Assert.IsTrue(PersistentMigration.IsFromNewerBuild(SaveLayouts.Read(layout), SaveLayouts.Version2()));
        }
    }
}
