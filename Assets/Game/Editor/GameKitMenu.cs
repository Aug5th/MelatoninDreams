using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameKit.EditorTools
{
    public static class GameKitMenu
    {
        const string BootstrapPath = "Assets/Scenes/Bootstrap.unity";

        [MenuItem("Tools/GameKit/Create Bootstrap Scene", false, 0)]
        public static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            new GameObject("[GameKit]").AddComponent<GameKitRoot>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, BootstrapPath);
            AssetDatabase.Refresh();

            AddSceneToBuildSettings(BootstrapPath, true);

            Debug.Log("[GameKit] Created " + BootstrapPath +
                      " and made it the first scene. You can now turn off GameKitConfig.AutoBootstrap.");
        }

        [MenuItem("Tools/GameKit/Open Save Folder", false, 20)]
        public static void OpenSaveFolder()
        {
            var path = Application.persistentDataPath;
            Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem("Tools/GameKit/Delete Save Data", false, 21)]
        public static void DeleteSaveData()
        {
            if (!EditorUtility.DisplayDialog("GameKit",
                    "Delete save.json and settings.json from persistentDataPath?", "Delete", "Cancel"))
                return;

            int removed = 0;
            foreach (var file in new[] { "save.json", "settings.json" })
            {
                var path = Path.Combine(Application.persistentDataPath, file);
                if (!File.Exists(path)) continue;
                File.Delete(path);
                removed++;
            }

            PlayerPrefs.DeleteKey("save.json");
            PlayerPrefs.DeleteKey("settings.json");
            PlayerPrefs.Save();

            Debug.Log("[GameKit] Deleted " + removed + " save file(s).");
        }

        static void AddSceneToBuildSettings(string path, bool first)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path)) return;

            var entry = new EditorBuildSettingsScene(path, true);
            if (first) scenes.Insert(0, entry);
            else scenes.Add(entry);

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
