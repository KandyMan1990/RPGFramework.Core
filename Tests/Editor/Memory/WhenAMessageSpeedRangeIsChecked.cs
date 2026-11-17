using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenAMessageSpeedRangeIsChecked
    {
        [Test]
        public void ThenASlowestOfZeroIsRefused()
        {
            Assert.AreEqual(1, Problems(0f, 52.5f).Length);
        }

        [Test]
        public void ThenASlowestAboveTheFastestIsRefused()
        {
            Assert.AreEqual(1, Problems(60f, 52.5f).Length);
        }

        [Test]
        public void ThenEqualEndsAreAccepted()
        {
            Assert.IsEmpty(Problems(30f, 30f));
        }

        private static string[] Problems(float slowest, float fastest)
        {
            TestVariableMap map = new TestVariableMap().Add(CoreVariables.MESSAGE_SPEED_SLOWEST, MemoryBank.Session, VariableWidth.Float, 0, VariableDefaults.FromFloat(slowest))
                                                       .Add(CoreVariables.MESSAGE_SPEED_FASTEST, MemoryBank.Session, VariableWidth.Float, 4, VariableDefaults.FromFloat(fastest));

            return Validation.Problems(map);
        }
    }
}
