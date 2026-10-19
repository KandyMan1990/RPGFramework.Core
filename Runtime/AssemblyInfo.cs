using System.Runtime.CompilerServices;

// The editor authors the variable map, which the game only reads.
[assembly: InternalsVisibleTo("RPGFramework.Core.Editor")]
[assembly: InternalsVisibleTo("RPGFramework.Core.Tests")]