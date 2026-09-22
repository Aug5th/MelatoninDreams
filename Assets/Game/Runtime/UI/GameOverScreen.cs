using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI
{
    public class GameOverScreen : UIScreen
    {
        public override bool IsOverlay { get { return true; } }

        Text _score;
        Text _best;
        Text _record;

        protected override void Build()
        {
            UIFactory.MakeFullscreenImage(transform, "Scrim", Theme.Scrim);

            var panel = UIFactory.MakePanel(transform, "Panel", new Vector2(700f, 620f));
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;

            var column = UIFactory.MakeColumn(panelRt, "Content", 12f, 540f);
            column.anchorMin = new Vector2(0.5f, 0.5f);
            column.anchorMax = new Vector2(0.5f, 0.5f);
            column.pivot = new Vector2(0.5f, 0.5f);
            column.anchoredPosition = Vector2.zero;

            UIFactory.MakeLabel(column, "Header", "GAME OVER", Theme.FontHeader, Theme.Danger);
            _record = UIFactory.MakeLabel(column, "Record", "NEW RECORD!", Theme.FontSmall, Theme.Accent);
            UIFactory.MakeSpacer(column, 10f);

            _score = UIFactory.MakeLabel(column, "Score", "0", Theme.FontTitle, Theme.Text);
            _best  = UIFactory.MakeLabel(column, "Best", "BEST 0", Theme.FontSmall, Theme.TextDim);

            UIFactory.MakeSpacer(column, 26f);

            UIFactory.MakeButton(column, "RETRY", () => GameManager.Instance.StartGame(), 540f, 78f, true);
            UIFactory.MakeButton(column, "MAIN MENU", () => GameManager.Instance.ToMainMenu(), 540f);
        }

        public override void OnShow()
        {
            var gm = GameManager.Instance;
            _score.text = gm.Score.ToString();
            _best.text = "BEST  " + SaveSystem.Save.highScore;
            _record.gameObject.SetActive(gm.IsNewRecord);
        }
    }
}
