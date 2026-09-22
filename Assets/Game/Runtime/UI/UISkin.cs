using UnityEngine;

namespace GameKit.UI
{
    /// <summary>
    /// Loads the UI art set from Assets/Arts/UI/Resources/GameKitUI.
    /// For any missing file UIFactory falls back to a code-generated sprite for that slot,
    /// so deleting assets never breaks the interface.
    ///
    /// To change the art: overwrite the file of the same name in that folder. No code change needed.
    /// </summary>
    public static class UISkin
    {
        public const string ResourceRoot = "GameKitUI/";

        // Frames / backgrounds
        public static Sprite Panel;
        public static Sprite PanelFlat;

        // Buttons
        public static Sprite Button;
        public static Sprite ButtonPrimary;
        public static Sprite ButtonSquare;

        // Slider
        public static Sprite SliderTrack;
        public static Sprite SliderFill;
        public static Sprite SliderHandle;

        // Toggle (checkbox style)
        public static Sprite ToggleOff;
        public static Sprite ToggleOn;

        // Progress bar
        public static Sprite ProgressTrack;
        public static Sprite ProgressFill;

        // Icon
        public static Sprite IconPlay;
        public static Sprite IconPause;
        public static Sprite IconRestart;
        public static Sprite IconClose;
        public static Sprite IconCheck;
        public static Sprite ArrowLeft;
        public static Sprite ArrowRight;

        // Decoration
        public static Sprite Divider;
        public static Sprite Star;

        public static Font Font;

        /// <summary>True once at least the button sprite was found - used to pick the right palette.</summary>
        public static bool Loaded { get; private set; }

        static bool _tried;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _tried = false;
            Loaded = false;
            Panel = PanelFlat = Button = ButtonPrimary = ButtonSquare = null;
            SliderTrack = SliderFill = SliderHandle = null;
            ToggleOff = ToggleOn = ProgressTrack = ProgressFill = null;
            IconPlay = IconPause = IconRestart = IconClose = IconCheck = null;
            ArrowLeft = ArrowRight = Divider = Star = null;
            Font = null;
        }

        public static void EnsureLoaded()
        {
            if (_tried) return;
            _tried = true;

            Panel         = Load("panel");
            PanelFlat     = Load("panel_flat");
            Button        = Load("button");
            ButtonPrimary = Load("button_primary");
            ButtonSquare  = Load("button_square");

            SliderTrack   = Load("slider_track");
            SliderFill    = Load("slider_fill");
            SliderHandle  = Load("slider_handle");

            ToggleOff     = Load("toggle_off");
            ToggleOn      = Load("toggle_on");

            ProgressTrack = Load("progress_track");
            ProgressFill  = Load("progress_fill");

            IconPlay      = Load("icon_play");
            IconPause     = Load("icon_pause");
            IconRestart   = Load("icon_restart");
            IconClose     = Load("icon_close");
            IconCheck     = Load("icon_check");
            ArrowLeft     = Load("arrow_left");
            ArrowRight    = Load("arrow_right");

            Divider       = Load("divider");
            Star          = Load("star");

            Font = Resources.Load<Font>(ResourceRoot + "font_ui");

            Loaded = Button != null;

            if (!Loaded)
            {
                Debug.Log("[GameKit] Could not load the skin from Resources/" + ResourceRoot +
                          " - falling back to code-generated sprites. " +
                          "Check that the PNG files are imported as Sprite (2D and UI).");
            }
        }

        static Sprite Load(string fileName)
        {
            return Resources.Load<Sprite>(ResourceRoot + fileName);
        }
    }
}
