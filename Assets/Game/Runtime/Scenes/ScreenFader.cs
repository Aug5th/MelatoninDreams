using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameKit
{
    /// <summary>
    /// Black overlay drawn above all UI (sortingOrder 999). It runs on unscaledDeltaTime
    /// so it still fades while the game is paused (Time.timeScale = 0).
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        CanvasGroup _group;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; }

        void Awake()
        {
            Instance = this;

            var go = new GameObject("Screen Fader", typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            _group = go.GetComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;

            var fillGo = new GameObject("Fill", typeof(Image));
            fillGo.transform.SetParent(go.transform, false);
            fillGo.GetComponent<Image>().color = Color.black;

            var rt = (RectTransform)fillGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public IEnumerator FadeTo(float alpha, float duration)
        {
            float start = _group.alpha;
            float t = 0f;

            _group.blocksRaycasts = true;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(start, alpha, Mathf.Clamp01(t / duration));
                yield return null;
            }

            _group.alpha = alpha;
            _group.blocksRaycasts = alpha > 0.001f;
        }

        public IEnumerator FadeOut(float duration = -1f)
        {
            yield return FadeTo(1f, duration < 0f ? GameKitConfig.FadeDuration : duration);
        }

        public IEnumerator FadeIn(float duration = -1f)
        {
            yield return FadeTo(0f, duration < 0f ? GameKitConfig.FadeDuration : duration);
        }
    }
}
