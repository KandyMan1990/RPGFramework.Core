using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGFramework.Core.Dialogue
{
    /// <summary>
    /// The named styles dialogue can use, such as <c>{Location}secret cave{/Location}</c>.<br /><br />
    /// A writer names what a thing is; this asset decides how that looks, so changing how every location is
    /// shown is one edit here rather than one per line of dialogue.
    /// </summary>
    [CreateAssetMenu(menuName = "RPG Framework/Dialogue/Dialogue Text Styles", fileName = "Dialogue Text Styles")]
    public sealed class DialogueTextStyles : ScriptableObject, IDialogueTextStyles
    {
        [SerializeField] private List<DialogueTextStyle> m_Styles = new List<DialogueTextStyle>();

        public IReadOnlyList<DialogueTextStyle> Styles => m_Styles;

        public bool TryGet(string name, out DialogueTextStyle style)
        {
            foreach (DialogueTextStyle candidate in m_Styles)
            {
                if (candidate.Name == name)
                {
                    style = candidate;
                    return true;
                }
            }

            style = null;
            return false;
        }
    }

    /// <summary>
    /// Looks a style up by the name a writer typed. The parser takes this rather than the asset, so it does
    /// not depend on how Unity compares an asset with null.
    /// </summary>
    public interface IDialogueTextStyles
    {
        bool TryGet(string name, out DialogueTextStyle style);
    }

    [Serializable]
    public sealed class DialogueTextStyle
    {
        [Tooltip("What writers type, as {Name}text{/Name}. Case matters.")]
        [SerializeField] private string m_Name;

        [SerializeField] private Color m_Colour = Color.white;

        [Tooltip("Flash the text, to draw the eye to something such as a new objective.")]
        [SerializeField] private bool m_Blink;

        public string Name   => m_Name;
        public Color  Colour => m_Colour;
        public bool   Blink  => m_Blink;

        public DialogueTextStyle(string name, Color colour, bool blink)
        {
            m_Name   = name;
            m_Colour = colour;
            m_Blink  = blink;
        }
    }
}