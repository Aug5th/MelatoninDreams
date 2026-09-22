using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameKit.EditorTools
{
    /// <summary>
    /// Applies import settings to every PNG under Assets/Arts/UI: Sprite type,
    /// no mipmaps, and the right 9-slice border per asset kind.
    ///
    /// So when you overwrite an art file with the same name, Unity re-applies the
    /// border automatically - no need to open the Sprite Editor.
    /// </summary>
    public class UIAssetPostprocessor : AssetPostprocessor
    {
        public const string UIFolder = "/Arts/UI/";

        /// <summary>Vector4 in Unity's order: (left, bottom, right, top).</summary>
        static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
        {
            // Frames
            { "panel",          new Vector4(10f, 14f, 10f, 10f) },
            { "panel_flat",     new Vector4(6f,  6f,  6f,  6f)  },

            // Buttons
            { "button",         new Vector4(6f, 10f, 6f, 10f) },
            { "button_primary", new Vector4(6f, 10f, 6f, 10f) },
            { "button_square",  new Vector4(6f, 10f, 6f, 10f) },

            // Slider / loading bar track and fill
            { "slider_track",   new Vector4(6f, 6f, 6f, 6f) },
            { "slider_fill",    new Vector4(6f, 6f, 6f, 6f) },
            { "progress_track", new Vector4(6f, 6f, 6f, 6f) },
            { "progress_fill",  new Vector4(6f, 6f, 6f, 6f) },

            // Checkbox
            { "toggle_off",     new Vector4(6f, 6f, 6f, 6f) },

            // Rule
            { "divider",        new Vector4(8f, 0f, 8f, 0f) },

            // Everything else (icon, arrow, star, toggle_on, slider_handle)
            // uses a zero border and is drawn Simple.
        };

        static bool IsUIAsset(string path)
        {
            return path.Replace('\\', '/').IndexOf(UIFolder, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void OnPreprocessTexture()
        {
            if (!IsUIAsset(assetPath)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 512;
            importer.spritePixelsPerUnit = 100f;

            var key = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
            Vector4 border;
            if (!Borders.TryGetValue(key, out border)) border = Vector4.zero;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteBorder = border;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
        }

        [MenuItem("Tools/GameKit/Reimport UI Skin", false, 10)]
        public static void ReimportUISkin()
        {
            const string folder = "Assets/Arts/UI";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning("[GameKit] Folder not found: " + folder);
                return;
            }

            AssetDatabase.ImportAsset(folder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            Debug.Log("[GameKit] Reimported all art under " + folder);
        }
    }
}
