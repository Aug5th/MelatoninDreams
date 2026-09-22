using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI
{
    public class MainMenuScreen : UIScreen
    {
        Text _best;

        protected override void Build()
        {
            UIFactory.MakeFullscreenImage(transform, "Background", Theme.Background);

            var column = UIFactory.MakeColumn(transform, "Menu", 16f, 520f);
            column.anchorMin = new Vector2(0.5f, 0.5f);
            column.anchorMax = new Vector2(0.5f, 0.5f);
            column.pivot = new Vector2(0.5f, 0.5f);
            column.anchoredPosition = new Vector2(0f, -30f);

            UIFactory.MakeLabel(column, "Title", GameKitConfig.GameTitle, Theme.FontTitle, Theme.Accent);
            UIFactory.MakeLabel(column, "Subtitle", GameKitConfig.GameSubtitle, Theme.FontSmall, Theme.TextDim);
            UISkin.EnsureLoaded();
            var bestRow = UIFactory.MakeRow(column, "Best", 44f);
            bestRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            if (UISkin.Star != null)
            {
                var star = UIFactory.MakeIcon(bestRow, "Star", UISkin.Star, Theme.Accent, 32f);
                UIFactory.SetWidth(star, 32f);
            }

            _best = UIFactory.MakeLabel(bestRow, "Best Label", "", Theme.FontSmall, Theme.TextDim,
                                        TextAnchor.MiddleLeft);
            UIFactory.SetWidth(_best, 240f);

            UIFactory.MakeSpacer(column, 24f);

            UIFactory.MakeButton(column, "PLAY", () => GameManager.Instance.StartGame(), 520f, 84f, true);
            UIFactory.MakeButton(column, "SETTINGS", () => UI.Push<SettingsScreen>());
            UIFactory.MakeButton(column, "QUIT", Quit);

            var hint = UIFactory.MakeLabel(transform, "Hint",
                "Esc / P = pause    -    build: " + Application.version,
                Theme.FontSmall, Theme.TextDim, TextAnchor.LowerCenter);
            var hintRt = (RectTransform)hint.transform;
            hintRt.anchorMin = new Vector2(0.5f, 0f);
            hintRt.anchorMax = new Vector2(0.5f, 0f);
            hintRt.pivot = new Vector2(0.5f, 0f);
            hintRt.anchoredPosition = new Vector2(0f, 36f);
        }

        public override void OnShow()
        {
            int best = SaveSystem.Save.highScore;
            _best.text = best > 0 ? "BEST  " + best : "NO SCORE YET";
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
