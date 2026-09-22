using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI
{
    /// <summary>
    /// In-game HUD. It only reads data from GameManager through events and knows
    /// nothing about gameplay, so swapping the demo for a real game just works.
    /// </summary>
    public class HudScreen : UIScreen
    {
        Text _score;
        Text _lives;

        protected override void Build()
        {
            var bar = UIFactory.NewRect(transform, "Top Bar");
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.sizeDelta = new Vector2(0f, 110f);
            bar.anchoredPosition = Vector2.zero;

            _score = UIFactory.MakeLabel(bar, "Score", "0", Theme.FontHeader, Theme.Text, TextAnchor.MiddleLeft);
            var scoreRt = (RectTransform)_score.transform;
            scoreRt.anchorMin = new Vector2(0f, 0.5f);
            scoreRt.anchorMax = new Vector2(0f, 0.5f);
            scoreRt.pivot = new Vector2(0f, 0.5f);
            scoreRt.anchoredPosition = new Vector2(48f, 0f);

            _lives = UIFactory.MakeLabel(bar, "Lives", "", Theme.FontHeader, Theme.Danger, TextAnchor.MiddleRight);
            var livesRt = (RectTransform)_lives.transform;
            livesRt.anchorMin = new Vector2(1f, 0.5f);
            livesRt.anchorMax = new Vector2(1f, 0.5f);
            livesRt.pivot = new Vector2(1f, 0.5f);
            livesRt.anchoredPosition = new Vector2(-180f, 0f);

            UISkin.EnsureLoaded();
            var pause = UIFactory.MakeIconButton(bar, "II", UISkin.IconPause,
                                                 () => GameManager.Instance.Pause(), 72f);
            var pauseRt = (RectTransform)pause.transform;
            pauseRt.anchorMin = new Vector2(1f, 0.5f);
            pauseRt.anchorMax = new Vector2(1f, 0.5f);
            pauseRt.pivot = new Vector2(1f, 0.5f);
            pauseRt.anchoredPosition = new Vector2(-48f, 0f);
            pauseRt.sizeDelta = new Vector2(72f, 72f);
        }

        public override void OnShow()
        {
            GameManager.ScoreChanged += SetScore;
            GameManager.LivesChanged += SetLives;
            SetScore(GameManager.Instance.Score);
            SetLives(GameManager.Instance.Lives);
        }

        public override void OnHide()
        {
            GameManager.ScoreChanged -= SetScore;
            GameManager.LivesChanged -= SetLives;
        }

        void SetScore(int value) { _score.text = value.ToString(); }

        void SetLives(int value)
        {
            var sb = new System.Text.StringBuilder(value * 2);
            for (int i = 0; i < value; i++) sb.Append("* ");
            _lives.text = sb.ToString().TrimEnd();
        }
    }
}
