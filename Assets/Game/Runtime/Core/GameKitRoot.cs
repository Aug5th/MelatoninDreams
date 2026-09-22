using UnityEngine;
using UnityEngine.SceneManagement;
using GameKit.UI;

namespace GameKit
{
    /// <summary>
    /// The one object that survives across scenes. It builds every manager in code,
    /// so even an empty project runs: press Play and the Main Menu appears.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameKitRoot : MonoBehaviour
    {
        public static GameKitRoot Instance { get; private set; }

        Camera _fallbackCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (GameKitConfig.AutoBootstrap) Boot();
        }

        public static GameKitRoot Boot()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("[GameKit]");
            DontDestroyOnLoad(go);
            return go.AddComponent<GameKitRoot>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SaveSystem.LoadAll();
            ApplyStartupSettings();
            EnsureCamera();

            // Order matters: AddComponent runs Awake immediately.
            gameObject.AddComponent<GameManager>();
            gameObject.AddComponent<AudioManager>();
            gameObject.AddComponent<SceneLoader>();
            gameObject.AddComponent<ScreenFader>();
            gameObject.AddComponent<UIManager>();
            gameObject.AddComponent<DemoGame>();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Start()
        {
            GameManager.Instance.ChangeState(GameState.MainMenu);
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }

        static void ApplyStartupSettings()
        {
            var s = SaveSystem.Settings;
            if (s.qualityLevel >= 0 && s.qualityLevel < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(s.qualityLevel, true);
#if !UNITY_WEBGL
            if (Screen.fullScreen != s.fullscreen) Screen.fullScreen = s.fullscreen;
#endif
            Application.targetFrameRate = 60;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) { EnsureCamera(); }

        /// <summary>An empty scene has no camera or AudioListener - create fallbacks so UI and audio still work.</summary>
        void EnsureCamera()
        {
            if (Camera.main != null)
            {
                if (_fallbackCamera != null && Camera.main != _fallbackCamera)
                {
                    Destroy(_fallbackCamera.gameObject);
                    _fallbackCamera = null;
                }
                EnsureListener();
                return;
            }

            if (_fallbackCamera != null) return;

            var go = new GameObject("GameKit Fallback Camera");
            go.tag = "MainCamera";
            DontDestroyOnLoad(go);
            _fallbackCamera = go.AddComponent<Camera>();
            _fallbackCamera.clearFlags = CameraClearFlags.SolidColor;
            _fallbackCamera.backgroundColor = Theme.Background;
            _fallbackCamera.orthographic = true;
            _fallbackCamera.orthographicSize = 5f;
            EnsureListener();
        }

        void EnsureListener()
        {
#if UNITY_2023_1_OR_NEWER
            var listener = Object.FindFirstObjectByType<AudioListener>();
#else
            var listener = Object.FindObjectOfType<AudioListener>();
#endif
            if (listener != null) return;
            var cam = Camera.main;
            if (cam != null) cam.gameObject.AddComponent<AudioListener>();
        }

        void OnApplicationQuit() { SaveSystem.SaveAll(); }

        void OnApplicationPause(bool paused) { if (paused) SaveSystem.SaveAll(); }
    }
}
