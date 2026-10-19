using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenALayoutIsWritten
    {
        // The second version has nine variables and four record fields.
        [Test]
        public void ThenItIsSixBytesThenTenAVariableAndFiveAField()
        {
            byte[] layout = PersistentLayout.Write(SaveLayouts.Version2());

            Assert.AreEqual(6 + 9 * 10 + 4 * 5, layout.Length);
        }
    }
}
