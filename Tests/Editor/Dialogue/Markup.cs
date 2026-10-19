using System.Collections.Generic;
using RPGFramework.Core.Dialogue;
using UnityEditor;
using UnityEngine;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal static class Markup
    {
        internal static DialogueText Parse(string source, out List<string> problems, int[] variables = null)
        {
            DialogueTextStyles styles = Styles();

            problems = new List<string>();

            DialogueText text = DialogueMarkup.Parse(source, variables, styles, problems);

            Object.DestroyImmediate(styles);

            return text;
        }

        internal static string[] Validate(string source)
        {
            DialogueTextStyles styles   = Styles();
            string[]           problems = DialogueMarkup.Validate(source, styles);

            Object.DestroyImmediate(styles);

            return problems;
        }

        internal static bool Same(Color a, Color b)
        {
            bool same = Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f && Mathf.Abs(a.a - b.a) < 0.01f;

            return same;
        }

        /// <summary>A location in yellow, and a task in red that blinks, authored as the styles' inspector authors them.</summary>
        private static DialogueTextStyles Styles()
        {
            DialogueTextStyles styles = ScriptableObject.CreateInstance<DialogueTextStyles>();
            SerializedObject   asset  = new SerializedObject(styles);
            SerializedProperty list   = asset.FindProperty("m_Styles");

            list.arraySize = 2;

            Author(list.GetArrayElementAtIndex(0), "Location", Color.yellow, false);
            Author(list.GetArrayElementAtIndex(1), "Task", Color.red, true);

            asset.ApplyModifiedPropertiesWithoutUndo();

            return styles;
        }

        private static void Author(SerializedProperty style, string name, Color colour, bool blink)
        {
            style.FindPropertyRelative("m_Name").stringValue  = name;
            style.FindPropertyRelative("m_Colour").colorValue = colour;
            style.FindPropertyRelative("m_Blink").boolValue   = blink;
        }
    }
}
