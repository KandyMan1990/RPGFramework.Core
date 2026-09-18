using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.UI;
using UnityEditor;

namespace RPGFramework.Core.Editor.Dialogue
{
    public static class DialogueEditorUtility
    {
        /// <summary>
        /// The styles dialogue uses in play: those on the project's dialogue window UI provider, or failing that the
        /// first styles asset in the project. Null when there are none.
        /// </summary>
        public static DialogueTextStyles FindProjectTextStyles()
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(DialogueWindowUiProvider)}"))
            {
                IDialogueWindowUiProvider provider = AssetDatabase.LoadAssetAtPath<DialogueWindowUiProvider>(AssetDatabase.GUIDToAssetPath(guid));

                if (provider?.TextStyles != null)
                {
                    DialogueTextStyles providerStyles = provider.TextStyles;

                    return providerStyles;
                }
            }

            string[]           found  = AssetDatabase.FindAssets($"t:{nameof(DialogueTextStyles)}");
            DialogueTextStyles styles = found.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<DialogueTextStyles>(AssetDatabase.GUIDToAssetPath(found[0]));

            return styles;
        }
    }
}
