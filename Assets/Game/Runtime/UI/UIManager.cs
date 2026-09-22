using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

namespace GameKit.UI
{
    /// <summary>
    /// Creates the Canvas and EventSystem, owns the screen stack, and wires GameState to the UI.
    /// To change which screen a state shows, edit only OnStateChanged below.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public Canvas Canvas { get; private set; }
        public RectTransform Root { get; private set; }

        /// <summary>Layer beneath all UI - where the demo mini-game draws its objects.</summary>
        public RectTransform GameLayer { get; private set; }

        readonly Dictionary<Type, UIScreen> _screens = new Dictionary<Type, UIScreen>();
        readonly List<UIScreen> _stack = new List<UIScreen>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; }

        void Awake()
        {
            Instance = this;
            BuildCanvas();
            EnsureEventSystem();
        }

        void OnEnable()  { GameManager.StateChanged += OnStateChanged; }
        void OnDisable() { GameManager.StateChanged -= OnStateChanged; }

        void BuildCanvas()
        {
            var go = new GameObject("GameKit Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);

            Canvas = go.GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 100;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = GameKitConfig.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Root = (RectTransform)go.transform;
            GameLayer = UIFactory.Stretch(Root, "Game Layer");
        }

        static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;

            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            DontDestroyOnLoad(go);
        }

        // ---------- Stack ----------

        public T Get<T>() where T : UIScreen
        {
            UIScreen screen;
            if (_screens.TryGetValue(typeof(T), out screen) && screen != null) return (T)screen;

            var rt = UIFactory.Stretch(Root, typeof(T).Name);
            var created = rt.gameObject.AddComponent<T>();
            created.Bind(this);
            created.EnsureBuilt();
            rt.gameObject.SetActive(false);

            _screens[typeof(T)] = created;
            return created;
        }

        /// <summary>Closes the whole stack, then opens a new screen as the base.</summary>
        public T Show<T>() where T : UIScreen
        {
            for (int i = _stack.Count - 1; i >= 0; i--) _stack[i].Hide();
            _stack.Clear();
            return Push<T>();
        }

        /// <summary>Pushes another screen on top of the current one.</summary>
        public T Push<T>() where T : UIScreen
        {
            var screen = Get<T>();
            if (_stack.Contains(screen)) return screen;

            if (_stack.Count > 0)
            {
                var below = _stack[_stack.Count - 1];
                if (screen.IsOverlay) below.SetInteractable(false);
                else below.Hide();
            }

            _stack.Add(screen);
            screen.Show();
            return screen;
        }

        public void Pop()
        {
            if (_stack.Count == 0) return;

            var top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            top.Hide();

            if (_stack.Count == 0) return;

            var below = _stack[_stack.Count - 1];
            if (!below.gameObject.activeSelf) below.Show();
            else below.SetInteractable(true);
        }

        /// <summary>Closes screens above until T is on top.</summary>
        public void PopTo<T>() where T : UIScreen
        {
            int guard = 0;
            while (_stack.Count > 1 && !(_stack[_stack.Count - 1] is T) && guard++ < 16) Pop();
        }

        public UIScreen Top
        {
            get { return _stack.Count > 0 ? _stack[_stack.Count - 1] : null; }
        }

        // ---------- Wiring UI to GameState ----------

        void OnStateChanged(GameState previous, GameState next)
        {
            var audio = AudioManager.Instance;

            switch (next)
            {
                case GameState.MainMenu:
                    Show<MainMenuScreen>();
                    if (audio != null) audio.PlayMusic(audio.MenuMusic);
                    break;

                case GameState.Loading:
                    Show<LoadingScreen>();
                    break;

                case GameState.Playing:
                    if (previous == GameState.Paused)
                    {
                        PopTo<HudScreen>();
                    }
                    else
                    {
                        Show<HudScreen>();
                        if (audio != null) audio.PlayMusic(audio.GameMusic);
                    }
                    break;

                case GameState.Paused:
                    Push<PauseScreen>();
                    break;

                case GameState.GameOver:
                    Push<GameOverScreen>();
                    if (audio != null) audio.PlaySfx(audio.GameOverSfx);
                    break;
            }
        }
    }
}
