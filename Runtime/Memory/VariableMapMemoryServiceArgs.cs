using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// Sizes the memory banks from a <see cref="VariableMapAsset" /> rather than from hand-entered numbers,
    /// so the banks are always exactly big enough for what has been declared.<br /><br />
    /// Bind this as <see cref="IMemoryServiceArgs" /> in the global installer to keep the authored map and
    /// the runtime allocation in step. Growing the map between releases is safe: a save written by an older
    /// build restores into the larger bank with the new bytes zeroed.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Core/Variable Map Memory Service Args", fileName = "VariableMapMemoryServiceArgs")]
    public sealed class VariableMapMemoryServiceArgs : ScriptableObject, IMemoryServiceArgs
    {
        [SerializeField]
        [Tooltip("The map that declares what lives in each bank")]
        private VariableMapAsset m_VariableMap;

        [SerializeField]
        [Tooltip("Spare bytes added to the global bank beyond what the map needs. Global is saved, so this bloats every save file — leave at 0 unless something writes global memory without declaring it")]
        private int m_AdditionalGlobalBytes;

        [SerializeField]
        [Tooltip("Spare bytes added to the session bank beyond what the map needs. Session is never saved, so this is cheap")]
        private int m_AdditionalSessionBytes;

        [SerializeField]
        [Tooltip("Spare bytes added to the temp bank beyond what the map needs. Temp is script scratch and is never saved, so leave headroom here rather than in Global")]
        private int m_AdditionalTempBytes = 64;

        int IMemoryServiceArgs.GlobalBytes
        {
            get
            {
                int globalBytes = GetBankBytes(MemoryBank.Global, m_AdditionalGlobalBytes);

                return globalBytes;
            }
        }

        int IMemoryServiceArgs.SessionBytes
        {
            get
            {
                int sessionBytes = GetBankBytes(MemoryBank.Session, m_AdditionalSessionBytes);

                return sessionBytes;
            }
        }

        int IMemoryServiceArgs.TempBytes
        {
            get
            {
                int tempBytes = GetBankBytes(MemoryBank.Temp, m_AdditionalTempBytes);

                return tempBytes;
            }
        }

        private int GetBankBytes(MemoryBank bank, int additionalBytes)
        {
            if (m_VariableMap == null)
            {
                Debug.LogError($"{nameof(VariableMapMemoryServiceArgs)}::{nameof(GetBankBytes)} No {nameof(VariableMapAsset)} assigned, so bank [{bank}] will be sized from the spare bytes alone");

                return additionalBytes;
            }

            int bankBytes = m_VariableMap.GetRequiredBytes(bank) + additionalBytes;

            return bankBytes;
        }
    }
}