using RPGFramework.Core.Memory;

namespace RPGFramework.Core.Editor
{
    /// <summary>A variable map checked before what reads it is exported or built.</summary>
    public interface IVariableMapValidator
    {
        /// <summary>
        /// What is wrong with the map — names, ids, overlaps, required variables missing or misdeclared; none when it is
        /// sound.
        /// </summary>
        string[] Validate(VariableMapAsset map);
    }

    /// <summary><see cref="IVariableMapValidator" /> as the Variable Map inspector and the build check validate.</summary>
    public sealed class VariableMapValidator : IVariableMapValidator
    {
        string[] IVariableMapValidator.Validate(VariableMapAsset map)
        {
            return map.Validate();
        }
    }
}
