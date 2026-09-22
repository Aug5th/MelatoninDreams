using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameKit
{
    /// <summary>
    /// Async scene loading: fade to black -> loading screen -> load -> fade back in.
    /// MinLoadingTime keeps the progress bar from snapping on small scenes.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public bool  IsLoading { get; private set; }
        public float Progress  { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; }

        void Awake() { Instance = this; }

        public void Load(string sceneName, Action onComplete = null)
        {
            if (IsLoading) return;

            if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning("[SceneLoader] Scene '" + sceneName +
                                 "' is not in Build Settings. Skipping the load step.");
                if (onComplete != null) onComplete();
                return;
            }

            StartCoroutine(LoadRoutine(sceneName, onComplete));
        }

        public void Reload(Action onComplete = null)
        {
            Load(SceneManager.GetActiveScene().name, onComplete);
        }

        IEnumerator LoadRoutine(string sceneName, Action onComplete)
        {
            IsLoading = true;
            Progress = 0f;

            yield return ScreenFader.Instance.FadeTo(1f, GameKitConfig.FadeDuration);

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            float elapsed = 0f;
            while (op.progress < 0.9f || elapsed < GameKitConfig.MinLoadingTime)
            {
                elapsed += Time.unscaledDeltaTime;
                Progress = Mathf.Clamp01(Mathf.Min(op.progress / 0.9f,
                                                   elapsed / GameKitConfig.MinLoadingTime));
                yield return null;
            }

            Progress = 1f;
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;

            if (onComplete != null) onComplete();

            yield return ScreenFader.Instance.FadeTo(0f, GameKitConfig.FadeDuration);
            IsLoading = false;
        }
    }
}
