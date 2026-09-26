using UnityEngine;

namespace GameKit
{
    /// <summary>
    /// Everything you need to tweak for your game lives here. Do not edit the systems themselves.
    /// </summary>
    public static class GameKitConfig
    {
        /// <summary>Boot the kit automatically in every scene. Turn off once you own a Bootstrap scene.</summary>
        public const bool AutoBootstrap = true;

        /// <summary>
        /// Leave empty to run the built-in demo mini-game.
        /// Set it to your real gameplay scene name (remember to add it to Build Settings)
        /// and the Play button will load that scene through SceneLoader.
        /// </summary>
        public const string GameSceneName = "GameScene";

        public const string GameTitle = "GAME TITLE";
        public const string GameSubtitle = "a game jam prototype";

        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        public const float FadeDuration = 0.35f;
        public const float MinLoadingTime = 0.6f;
        public const int StartingLives = 3;
    }

    /// <summary>Shared colour palette and font sizes for the whole UI.</summary>
    public static class Theme
    {
        public static readonly Color Background   = Hex("#12141C");
        public static readonly Color Panel        = Hex("#1B1F2B");
        public static readonly Color Scrim        = new Color(0f, 0f, 0f, 0.74f);
        public static readonly Color Accent       = Hex("#4CC2FF");
        public static readonly Color Danger       = Hex("#FF6B6B");
        public static readonly Color Text         = Hex("#EAEEF5");
        public static readonly Color TextDim      = Hex("#8A93A8");
        public static readonly Color ButtonNormal = Hex("#2A3042");
        public static readonly Color ButtonHover  = Hex("#3A4360");
        public static readonly Color ButtonPress  = Hex("#4CC2FF");
        public static readonly Color TrackColor   = Hex("#2A3042");

        // Used when a skin is present in Assets/Arts/UI: the sprites already carry
        // their own colour, so we only brighten or darken them slightly.
        public static readonly Color SkinTint       = Color.white;
        public static readonly Color SkinHover      = new Color(1.10f, 1.10f, 1.10f, 1f);
        public static readonly Color SkinPress      = new Color(0.84f, 0.84f, 0.84f, 1f);
        public static readonly Color SkinDisabled   = new Color(0.55f, 0.55f, 0.60f, 1f);
        /// <summary>Label colour on the skin's light-coloured buttons.</summary>
        public static readonly Color TextOnLight    = Hex("#1B1F2B");

        public const int FontTitle  = 96;
        public const int FontHeader = 48;
        public const int FontBody   = 30;
        public const int FontSmall  = 24;

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
