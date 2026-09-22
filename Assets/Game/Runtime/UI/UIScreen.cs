using System.Collections;
using UnityEngine;

namespace GameKit.UI
{
    /// <summary>
    /// A single UI screen. Subclass this, build the layout in Build(),
    /// then call UIManager.Show/Push. No prefab needed.
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        protected UIManager UI;

        public RectTransform Rect { get; private set; }
        public CanvasGroup Group { get; private set; }

        /// <summary>True = drawn on top of the screen below (pause, settings, game over).</summary>
        public virtual bool IsOverlay { get { return false; } }

        /// <summary>True = this screen swallows the Esc key, so GameManager will not pause/resume.</summary>
        public virtual bool ConsumesPauseKey { get { return false; } }

        /// <summary>Called when the player presses Esc and ConsumesPauseKey is true.</summary>
        public virtual void OnPauseKey() { }

        bool _built;

        public void Bind(UIManager manager)
        {
            UI = manager;
            Rect = (RectTransform)transform;
            Group = gameObject.GetComponent<CanvasGroup>();
            if (Group == null) Group = gameObject.AddComponent<CanvasGroup>();
        }

        public void EnsureBuilt()
        {
            if (_built) return;
            _built = true;
            Build();
        }

        protected abstract void Build();

        public virtual void OnShow() { }
        public virtual void OnHide() { }

        public void Show()
        {
            EnsureBuilt();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            SetInteractable(true);
            OnShow();
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }

        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            OnHide();
            gameObject.SetActive(false);
        }

        public void SetInteractable(bool value)
        {
            Group.interactable = value;
            Group.blocksRaycasts = value;
        }

        IEnumerator FadeIn()
        {
            const float duration = 0.14f;
            float t = 0f;
            Group.alpha = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                Group.alpha = Mathf.Clamp01(t / duration);
                yield return null;
            }
            Group.alpha = 1f;
        }
    }
}
