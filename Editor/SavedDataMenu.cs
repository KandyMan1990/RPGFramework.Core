using System;
using System.Collections.Generic;
using System.IO;
using RPGFramework.Core.SaveData;
using UnityEditor;
using UnityEngine;
using SettingsService = RPGFramework.Core.Settings.SettingsService;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// Deletes the game's settings and saves from this machine, so the next play is a first launch — which is the
    /// settings file not existing — with no saves.
    /// </summary>
    internal static class SavedDataMenu
    {
        private const string MENU_PATH = "RPG Framework/Saved Data/Delete Settings and Saves";

        [MenuItem(MENU_PATH)]
        private static void DeleteSettingsAndSaves()
        {
            string   folder = Application.persistentDataPath;
            string[] files  = FindFiles(folder);

            if (files.Length == 0)
            {
                EditorUtility.DisplayDialog("Nothing to delete", $"There are no settings or saves in {folder}.", "OK");
                return;
            }

            string[] names = new string[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                names[i] = Path.GetFileName(files[i]);
            }

            string message = $"{files.Length} file(s) in {folder}:\n\n{string.Join("\n", names)}\n\nThe next play is a first launch, with no saves.";

            if (!EditorUtility.DisplayDialog("Delete settings and saves?", message, "Delete", "Cancel"))
            {
                return;
            }

            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }

            Debug.Log($"{nameof(SavedDataMenu)}::{nameof(DeleteSettingsAndSaves)} Deleted {string.Join(", ", names)} from [{folder}]");
        }

        // Not while playing: the settings and save services hold what they read, and would write it back.
        [MenuItem(MENU_PATH, true)]
        private static bool CanDeleteSettingsAndSaves()
        {
            bool canDelete = !EditorApplication.isPlaying;

            return canDelete;
        }

        // The files themselves and any a write left half done beside them, as name.tmp.
        private static string[] FindFiles(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return Array.Empty<string>();
            }

            List<string> files = new List<string>();

            files.AddRange(Directory.GetFiles(folder, SettingsService.SETTINGS_FILE_NAME + "*"));
            files.AddRange(Directory.GetFiles(folder, SaveDataService.SAVE_FILE_SEARCH_PATTERN + "*"));

            string[] found = files.ToArray();

            return found;
        }
    }
}
