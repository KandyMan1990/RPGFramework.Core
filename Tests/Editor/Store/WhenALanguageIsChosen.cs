using NUnit.Framework;
using RPGFramework.Core.Store;
using RPGFramework.Core.Tests.Settings;

namespace RPGFramework.Core.Tests.Store
{
    internal sealed class WhenALanguageIsChosen
    {
        private ISettingsStore m_Store;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map = TestSettings.Map();

            m_Store = new SettingsStore(TestMemory.Make(16), map);
            m_Store.SetLanguage("de");
        }

        [Test]
        public void ThenOneIsChosen()
        {
            Assert.IsTrue(m_Store.IsLanguageChosen);
        }

        [Test]
        public void ThenItIsTheLanguage()
        {
            Assert.IsTrue(m_Store.IsLanguage("de"));
        }

        [Test]
        public void ThenAnotherIsNot()
        {
            Assert.IsFalse(m_Store.IsLanguage("en"));
        }
    }
}
