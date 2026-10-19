using System;
using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenAMapHasMissingOrSharedIds
    {
        /// <summary>
        /// A variable with no id, one above the map's counter and a copy sharing it, and a record with no id whose second
        /// field shares its first's: five problems, the copy's counted once.
        /// </summary>
        [Test]
        public void ThenEachIsReported()
        {
            VariableRecordField first  = new VariableRecordField("x", VariableWidth.Byte, 1, 0);
            VariableRecordField copied = new VariableRecordField("y", VariableWidth.Byte, 1, 0);

            first.AssignId(1);
            copied.AssignId(1);

            VariableDefinition[] variables =
            {
                new VariableDefinition(0, "legacy", MemoryBank.Persistent, VariableWidth.Byte, 1, 0, string.Empty, 0),
                new VariableDefinition(5, "original", MemoryBank.Persistent, VariableWidth.Byte, 1, 1, string.Empty, 0),
                new VariableDefinition(5, "copy", MemoryBank.Persistent, VariableWidth.Byte, 1, 2, string.Empty, 0),
                new VariableDefinition(0, "record", MemoryBank.Persistent, new[] { first, copied }, 1, 20, string.Empty)
            };

            string[] problems = VariableMapValidation.Validate(variables, 0, Array.Empty<RequiredVariable>(), Array.Empty<IStartModule>());

            Assert.AreEqual(5, problems.Length, string.Join("; ", problems));
        }
    }
}
