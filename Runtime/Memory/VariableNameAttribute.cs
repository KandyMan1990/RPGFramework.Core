using System.Diagnostics;
using UnityEngine;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// Marks a <c>string</c> as the name of a single variable of one width in the project's variable map, outside Temp,
    /// which the inspector and the UI Builder offer as a list, empty for none. Left out of a player build: only the editor
    /// reads it.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public sealed class VariableNameAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        internal VariableWidth Width { get; }
#endif

        public VariableNameAttribute(VariableWidth width)
        {
#if UNITY_EDITOR
            Width = width;
#endif
        }
    }
}
