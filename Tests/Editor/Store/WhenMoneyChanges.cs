using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    internal sealed class WhenMoneyChanges
    {
        private const ushort MONEY   = 4;
        private const ushort MAXIMUM = 8;

        private IMemoryService m_Memory;
        private IMoneyStore    m_Store;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map = new TestVariableMap().Add(CoreVariables.MONEY,         MemoryBank.Persistent, VariableWidth.UInt, MONEY,   0)
                                                       .Add(CoreVariables.MONEY_MAXIMUM, MemoryBank.Session,    VariableWidth.UInt, MAXIMUM, 0);

            m_Memory = TestMemory.Make(16);
            m_Store  = new MoneyStore(m_Memory, map);

            Money = 1000;
        }

        [Test]
        public void ThenAnAmountIsAdded()
        {
            m_Store.AddMoney(250);

            Assert.AreEqual(1250, Money);
        }

        [Test]
        public void ThenANegativeAmountIsTaken()
        {
            m_Store.AddMoney(-250);

            Assert.AreEqual(750, Money);
        }

        [Test]
        public void ThenItStopsAtNone()
        {
            m_Store.AddMoney(-1001);

            Assert.AreEqual(0, Money);
        }

        [Test]
        public void ThenItStopsAtTheGamesMaximum()
        {
            Maximum = 1200;

            m_Store.AddMoney(500);

            Assert.AreEqual(1200, Money);
        }

        [Test]
        public void ThenAMaximumOfZeroIsNoLimit()
        {
            m_Store.AddMoney(int.MaxValue);

            Assert.AreEqual(1000u + int.MaxValue, Money);
        }

        private uint Money
        {
            get => m_Memory.ReadUint(MemoryBank.Persistent, MONEY);
            set => m_Memory.WriteUint(MemoryBank.Persistent, MONEY, value);
        }

        private uint Maximum
        {
            set => m_Memory.WriteUint(MemoryBank.Session, MAXIMUM, value);
        }
    }
}
