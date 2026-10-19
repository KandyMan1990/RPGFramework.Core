using System;
using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASaveIsLoadedUnderTheNextMap
    {
        private byte[] m_Image;

        [SetUp]
        public void SetUp()
        {
            m_Image = PersistentMigration.Migrate(SaveLayouts.SavedUnderVersion1(), SaveLayouts.Of(SaveLayouts.Version1()), SaveLayouts.Version2(), 36);
        }

        [Test]
        public void ThenARenamedMovedVariableKeepsItsValue()
        {
            Assert.AreEqual(70000u, BitConverter.ToUInt32(m_Image, 32));
        }

        [Test]
        public void ThenAnUnchangedVariableKeepsItsValue()
        {
            Assert.AreEqual(1, m_Image[0]);
        }

        [Test]
        public void ThenAWidenedVariableKeepsItsValue()
        {
            Assert.AreEqual(200, BitConverter.ToUInt16(m_Image, 2));
        }

        [Test]
        public void ThenALongerArrayKeepsItsElements()
        {
            Assert.AreEqual(10, BitConverter.ToUInt16(m_Image, 4));
            Assert.AreEqual(30, BitConverter.ToUInt16(m_Image, 8));
        }

        [Test]
        public void ThenALongerArraysNewElementsTakeTheDefault()
        {
            Assert.AreEqual(7, BitConverter.ToUInt16(m_Image, 10));
            Assert.AreEqual(7, BitConverter.ToUInt16(m_Image, 12));
        }

        [Test]
        public void ThenANarrowedVariableIsClamped()
        {
            Assert.AreEqual(short.MinValue, BitConverter.ToInt16(m_Image, 14));
        }

        [Test]
        public void ThenAFloatMadeAnIntegerRoundsTowardZero()
        {
            Assert.AreEqual(3, BitConverter.ToInt32(m_Image, 16));
        }

        [Test]
        public void ThenARecordsFieldsFollowTheirIdsThroughAReorderAndAWidening()
        {
            Assert.AreEqual(5, m_Image[20]);
            Assert.AreEqual(100u, BitConverter.ToUInt32(m_Image, 21));
        }

        [Test]
        public void ThenAFieldAddedToARecordTakesItsDefault()
        {
            Assert.AreEqual(50, BitConverter.ToUInt16(m_Image, 25));
        }

        [Test]
        public void ThenAValueThatBecameARecordTakesTheRecordsDefault()
        {
            Assert.AreEqual(9, m_Image[28]);
        }

        [Test]
        public void ThenAVariableAddedSinceTakesItsDefault()
        {
            Assert.AreEqual(42, m_Image[29]);
        }

        [Test]
        public void ThenTheSaveIsNotFromANewerBuild()
        {
            Assert.IsFalse(PersistentMigration.IsFromNewerBuild(SaveLayouts.Of(SaveLayouts.Version1()), SaveLayouts.Version2()));
        }
    }
}
