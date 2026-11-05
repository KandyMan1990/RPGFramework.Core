using System.Collections.Generic;
using System.IO;
using RPGFramework.Core.Input;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// The input adapter's inspector: its map and movement action picked from its actions, as Unity's
    /// <c>PlayerInput</c> picks a default map, and what the module in its scene reads that the map lacks, with a button
    /// adding the missing actions, unbound, as Unity's actions editor makes a new one.
    /// </summary>
    [CustomEditor(typeof(InputAdapter))]
    internal sealed class InputAdapterEditor : UnityEditor.Editor
    {
        private const string NO_MOVEMENT = "(none)";

        private DropdownField m_Map;
        private DropdownField m_Movement;
        private HelpBox       m_Problems;
        private Button        m_AddMissing;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            m_Map        = new DropdownField("Map") { tooltip = "The map this module's input comes from: every button action in it is routed by name" };
            m_Movement   = new DropdownField("Movement") { tooltip = "The direction the player moves with; none in a module where nothing moves" };
            m_Problems   = new HelpBox(string.Empty, HelpBoxMessageType.Error);
            m_AddMissing = new Button(AddMissing) { text = "Add Missing Actions" };

            m_Map.RegisterValueChangedCallback(e => SetString("m_Map", e.newValue));
            m_Movement.RegisterValueChangedCallback(e => SetString("m_Movement", e.newValue == NO_MOVEMENT ? string.Empty : e.newValue));

            root.Add(new PropertyField(serializedObject.FindProperty("m_Actions")));
            root.Add(m_Map);
            root.Add(m_Movement);
            root.Add(m_Problems);
            root.Add(m_AddMissing);

            root.TrackSerializedObjectValue(serializedObject, _ => Refresh());

            Refresh();

            return root;
        }

        private InputActionAsset Actions => (InputActionAsset)serializedObject.FindProperty("m_Actions").objectReferenceValue;

        private string MapName => serializedObject.FindProperty("m_Map").stringValue;

        private void SetString(string property, string value)
        {
            serializedObject.FindProperty(property).stringValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private void Refresh()
        {
            serializedObject.Update();

            InputActionAsset actions  = Actions;
            InputActionMap   map      = actions != null ? actions.FindActionMap(MapName) : null;
            List<string>     maps     = new List<string>();
            List<string>     movement = new List<string> { NO_MOVEMENT };

            for (int i = 0; actions != null && i < actions.actionMaps.Count; i++)
            {
                maps.Add(actions.actionMaps[i].name);
            }

            for (int i = 0; map != null && i < map.actions.Count; i++)
            {
                if (map.actions[i].type != InputActionType.Button)
                {
                    movement.Add(map.actions[i].name);
                }
            }

            string current = serializedObject.FindProperty("m_Movement").stringValue;

            m_Map.choices = maps;
            m_Map.SetValueWithoutNotify(MapName);
            m_Map.SetEnabled(actions != null);
            m_Movement.choices = movement;
            m_Movement.SetValueWithoutNotify(string.IsNullOrEmpty(current) ? NO_MOVEMENT : current);
            m_Movement.SetEnabled(map != null);

            InputAdapter       adapter  = (InputAdapter)target;
            IRequiredActions[] modules  = InputActionsCheck.ModulesIn(adapter.gameObject.scene);
            List<string>       problems = new List<string>();

            InputActionsCheck.CheckAdapter(adapter, modules, problems);

            m_Problems.text = string.Join("\n", problems);
            m_Problems.style.display = problems.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            bool canAdd = map != null && InputActionsCheck.Missing(map, modules).Count > 0;
            m_AddMissing.style.display = canAdd ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // A copy read from the file is changed and written back, as the actions editor saves, so the live actions —
        // enabled while playing, and UI Toolkit's — are never edited in place.
        private void AddMissing()
        {
            string             path    = AssetDatabase.GetAssetPath(Actions);
            IRequiredActions[] modules = InputActionsCheck.ModulesIn(((InputAdapter)target).gameObject.scene);
            InputActionAsset   copy    = InputActionAsset.FromJson(File.ReadAllText(path));

            try
            {
                InputActionMap       map     = copy.FindActionMap(MapName);
                List<RequiredAction> missing = InputActionsCheck.Missing(map, modules);

                for (int i = 0; i < missing.Count; i++)
                {
                    bool direction = missing[i].Kind == RequiredActionKind.Direction;

                    map.AddAction(missing[i].Name, direction ? InputActionType.Value : InputActionType.Button, expectedControlLayout: direction ? "Vector2" : "Button");
                }

                File.WriteAllText(path, copy.ToJson());
            }
            finally
            {
                DestroyImmediate(copy);
            }

            AssetDatabase.ImportAsset(path);

            Refresh();
        }
    }
}
