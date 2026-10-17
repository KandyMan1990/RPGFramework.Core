using System.Diagnostics;
using UnityEngine;

namespace RPGFramework.Core.Audio
{
    /// <summary>
    /// Marks a <c>ulong</c> as a sound effect's name hash, which the inspector offers as a list of the sounds in the
    /// project's sound effect providers, <c>0</c> for none. Left out of a player build: only the editor reads it.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    internal sealed class SoundNameAttribute : PropertyAttribute
    {
    }
}