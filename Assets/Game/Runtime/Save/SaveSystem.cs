using System;
using System.IO;
using UnityEngine;

namespace GameKit
{
    /// <summary>Player preferences edited on the Settings screen.</summary>
    [Serializable]
    public class SettingsData
    {
        public float masterVolume = 1f;
        public float musicVolume  = 0.6f;
        public float sfxVolume    = 0.9f;
        public bool  fullscreen   = true;
        public int   qualityLevel = -1;   // -1 = keep the project default
        public bool  screenShake  = true;
    }

    /// <summary>Game progress. Add new fields freely - JsonUtility ignores unknown ones.</summary>
    [Serializable]
    public class SaveData
    {
        public int    highScore;
        public int    totalRuns;
        public string lastPlayedUtc = "";
        public int    lastLevel;
    }

    /// <summary>
    /// Writes JSON to persistentDataPath (PlayerPrefs on WebGL builds for itch.io).
    /// Atomic write: writes a .tmp file first, then replaces it, so a crash cannot corrupt the save.
    /// </summary>
    public static class SaveSystem
    {
        const string SettingsFile = "settings.json";
        const string SaveFile     = "save.json";

        public static SettingsData Settings { get; private set; } = new SettingsData();
        public static SaveData     Save     { get; private set; } = new SaveData();

        public static event Action SettingsChanged;

        static bool _loaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _loaded = false;
            Settings = new SettingsData();
            Save = new SaveData();
            SettingsChanged = null;
        }

        public static void LoadAll()
        {
            if (_loaded) return;
            _loaded = true;
            Settings = Read(SettingsFile, new SettingsData());
            Save     = Read(SaveFile,     new SaveData());
        }

        public static void SaveSettings()
        {
            Write(SettingsFile, Settings);
            SettingsChanged?.Invoke();
        }

        public static void SaveGame() { Write(SaveFile, Save); }

        public static void SaveAll()
        {
            Write(SettingsFile, Settings);
            Write(SaveFile, Save);
        }

        public static void ResetSave()
        {
            Save = new SaveData();
            Write(SaveFile, Save);
        }

        public static void ResetSettings()
        {
            Settings = new SettingsData();
            SaveSettings();
        }

        public static string Location
        {
            get { return UsePlayerPrefs ? "PlayerPrefs" : Application.persistentDataPath; }
        }

        static bool UsePlayerPrefs
        {
            get { return Application.platform == RuntimePlatform.WebGLPlayer; }
        }

        static T Read<T>(string file, T fallback) where T : class
        {
            try
            {
                string json = null;
                if (UsePlayerPrefs)
                {
                    if (PlayerPrefs.HasKey(file)) json = PlayerPrefs.GetString(file);
                }
                else
                {
                    var path = Path.Combine(Application.persistentDataPath, file);
                    if (File.Exists(path)) json = File.ReadAllText(path);
                }

                if (string.IsNullOrEmpty(json)) return fallback;
                return JsonUtility.FromJson<T>(json) ?? fallback;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Could not read " + file + ": " + e.Message);
                return fallback;
            }
        }

        static void Write(string file, object data)
        {
            try
            {
                var json = JsonUtility.ToJson(data, true);

                if (UsePlayerPrefs)
                {
                    PlayerPrefs.SetString(file, json);
                    PlayerPrefs.Save();
                    return;
                }

                var path = Path.Combine(Application.persistentDataPath, file);
                var tmp  = path + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Could not write " + file + ": " + e.Message);
            }
        }
    }
}
