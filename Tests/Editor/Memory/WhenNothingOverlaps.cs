using NUnit.Framework;
using RPGFramework.Core.Memory;
using UnityEngine;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenNothingOverlaps
    {
        [Test]
        public void ThenNothingMoves()
        {
            VariableMapAsset map   = MapWithAGap.Make();
            int              moved = map.RelocateOverlapping();

            Object.DestroyImmediate(map);

            Assert.AreEqual(0, moved);
        }
    }
}
