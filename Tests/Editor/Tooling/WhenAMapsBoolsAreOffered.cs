using System;
using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Memory;
using UnityEngine;

namespace RPGFramework.Core.Tests.Tooling
{
    /// <summary>A new map's Bools offered by name, as an entry's <c>when</c> lists them: Core's required variables.</summary>
    internal sealed class WhenAMapsBoolsAreOffered
    {
        private VariableMapAsset m_Map;
        private string[]         m_Names;

        [SetUp]
        public void SetUp()
        {
            m_Map   = ScriptableObject.CreateInstance<VariableMapAsset>();
            m_Names = VariableNames.Of(m_Map, VariableWidth.Bool);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(m_Map);
        }

        [Test]
        public void ThenABoolIsOffered()
        {
            Assert.Contains(CoreVariables.SAVE_ENABLED, m_Names);
        }

        [Test]
        public void ThenAnotherWidthIsNot()
        {
            Assert.AreEqual(-1, Array.IndexOf(m_Names, CoreVariables.PLAY_TIME));
        }

        [Test]
        public void ThenTheyAreInNameOrder()
        {
            string[] sorted = (string[])m_Names.Clone();

            Array.Sort(sorted, string.CompareOrdinal);

            CollectionAssert.AreEqual(sorted, m_Names);
        }
    }
}
