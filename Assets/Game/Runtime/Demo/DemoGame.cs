using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameKit.UI;

namespace GameKit
{
    /// <summary>
    /// Demo mini-game: click each circle before it disappears.
    /// Its only job is to prove the Menu -> Play -> Pause -> Game Over -> Save loop works end to end.
    /// Once you have real gameplay, set GameKitConfig.GameSceneName to your scene name
    /// and this script disables itself.
    /// </summary>
    public class DemoGame : MonoBehaviour
    {
        const float SpawnIntervalStart = 0.95f;
        const float SpawnIntervalEnd   = 0.38f;
        const float RampSeconds        = 45f;
        const float TargetLifetime     = 1.7f;

        readonly List<DemoTarget> _targets = new List<DemoTarget>();

        bool  _running;
        float _spawnTimer;
        float _elapsed;

        void OnEnable()  { GameManager.StateChanged += OnStateChanged; }
        void OnDisable() { GameManager.StateChanged -= OnStateChanged; }

        bool Enabled
        {
            get { return string.IsNullOrEmpty(GameKitConfig.GameSceneName); }
        }

        void OnStateChanged(GameState previous, GameState next)
        {
            if (!Enabled) return;

            if (next == GameState.Playing && previous != GameState.Paused) StartRun();
            else if (next != GameState.Playing && next != GameState.Paused) StopRun();
        }

        void StartRun()
        {
            ClearTargets();
            _running = true;
            _elapsed = 0f;
            _spawnTimer = 0.4f;
        }

        void StopRun()
        {
            _running = false;
            ClearTargets();
        }

        void ClearTargets()
        {
            for (int i = 0; i < _targets.Count; i++)
                if (_targets[i] != null) Destroy(_targets[i].gameObject);
            _targets.Clear();
        }

        void Update()
        {
            if (!_running) return;
            if (GameManager.Instance.State != GameState.Playing) return;

            _elapsed += Time.deltaTime;
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;

            Spawn();
            _spawnTimer = Mathf.Lerp(SpawnIntervalStart, SpawnIntervalEnd,
                                     Mathf.Clamp01(_elapsed / RampSeconds));
        }

        void Spawn()
        {
            var layer = UIManager.Instance.GameLayer;
            var size = layer.rect.size;
            const float margin = 160f;

            var rt = UIFactory.NewRect(layer, "Target");
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(150f, 150f);
            rt.anchoredPosition = new Vector2(
                UnityEngine.Random.Range(-size.x * 0.5f + margin, size.x * 0.5f - margin),
                UnityEngine.Random.Range(-size.y * 0.5f + margin, size.y * 0.5f - margin - 60f));

            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = UIFactory.CircleSprite;
            img.color = Theme.Accent;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;

            var target = rt.gameObject.AddComponent<DemoTarget>();
            target.Init(img, TargetLifetime, OnTargetFinished);
            btn.onClick.AddListener(target.Hit);

            _targets.Add(target);
        }

        void OnTargetFinished(DemoTarget target, bool clicked, float remaining)
        {
            _targets.Remove(target);
            Destroy(target.gameObject);

            var gm = GameManager.Instance;
            var audio = AudioManager.Instance;

            if (clicked)
            {
                int points = 10 + Mathf.RoundToInt(remaining * 15f);
                gm.AddScore(points);
                if (audio != null) audio.PlaySfx(audio.ScoreSfx, 1f, UnityEngine.Random.Range(0.95f, 1.15f));
            }
            else
            {
                if (audio != null) audio.PlaySfx(audio.HurtSfx);
                gm.LoseLife();
            }
        }
    }

    /// <summary>A single circle: it shrinks over time, and costs a life if it expires.</summary>
    public class DemoTarget : MonoBehaviour
    {
        Image _image;
        RectTransform _rect;
        Action<DemoTarget, bool, float> _onFinished;
        float _lifetime;
        float _age;
        bool  _done;

        public void Init(Image image, float lifetime, Action<DemoTarget, bool, float> onFinished)
        {
            _image = image;
            _rect = (RectTransform)transform;
            _lifetime = lifetime;
            _onFinished = onFinished;
        }

        void Update()
        {
            if (_done) return;

            _age += Time.deltaTime;
            float k = Mathf.Clamp01(_age / _lifetime);

            _rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.3f, k);
            _image.color = Color.Lerp(Theme.Accent, Theme.Danger, k);

            if (_age >= _lifetime) Finish(false);
        }

        public void Hit() { Finish(true); }

        void Finish(bool clicked)
        {
            if (_done) return;
            _done = true;
            float remaining = Mathf.Clamp01(1f - _age / _lifetime);
            if (_onFinished != null) _onFinished(this, clicked, remaining);
        }
    }
}
