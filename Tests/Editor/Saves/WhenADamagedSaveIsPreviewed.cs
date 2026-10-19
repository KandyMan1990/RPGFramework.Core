using System;
using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenADamagedSaveIsPreviewed
    {
        [Test]
        public void ThenABankShorterThanItsLayoutIsDamage()
        {
            SavePreview preview = Preview(new byte[10], PersistentLayout.Write(SaveLayouts.Version2()));

            Assert.IsTrue(preview.IsDamaged);
        }

        [Test]
        public void ThenALayoutCutShortIsDamage()
        {
            SavePreview preview = Preview(new byte[36], LayoutCutShort());

            Assert.IsTrue(preview.IsDamaged);
        }

        [Test]
        public void ThenItsVariablesReadTheirDefaults()
        {
            SavePreview preview = Preview(new byte[36], LayoutCutShort());

            Assert.AreEqual(42, preview.Read<byte>("added"));
        }

        private static SavePreview Preview(byte[] persistent, byte[] layout)
        {
            ISaveStorage storage = new FakeSaveStorage();

            storage.Write(SaveFiles.FILE_NAME, SaveFiles.Holding(persistent, layout));

            SavePreview preview = SaveFiles.Service(SaveLayouts.Version2(), storage, out _).ReadPreview(SaveFiles.FILE_NAME);

            return preview;
        }

        private static byte[] LayoutCutShort()
        {
            byte[] layout = PersistentLayout.Write(SaveLayouts.Version2());

            Array.Resize(ref layout, layout.Length - 3);

            return layout;
        }
    }
}
