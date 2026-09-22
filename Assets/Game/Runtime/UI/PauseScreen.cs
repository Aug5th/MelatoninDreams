using UnityEngine;

namespace GameKit.UI
{
    public class PauseScreen : UIScreen
    {
        public override bool IsOverlay { get { return true; } }

        protected override void Build()
        {
            UIFactory.MakeFullscreenImage(transform, "Scrim", Theme.Scrim);

            var panel = UIFactory.MakePanel(transform, "Panel", new Vector2(640f, 520f));
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;

            var column = UIFactory.MakeColumn(panelRt, "Menu", 16f, 480f);
            column.anchorMin = new Vector2(0.5f, 0.5f);
            column.anchorMax = new Vector2(0.5f, 0.5f);
            column.pivot = new Vector2(0.5f, 0.5f);
            column.anchoredPosition = Vector2.zero;

            UIFactory.MakeLabel(column, "Header", "PAUSED", Theme.FontHeader, Theme.Text);
            UIFactory.MakeSpacer(column, 20f);

            UIFactory.MakeButton(column, "RESUME", () => GameManager.Instance.Resume(), 480f, 78f, true);
            UIFactory.MakeButton(column, "SETTINGS", () => UI.Push<SettingsScreen>(), 480f);
            UIFactory.MakeButton(column, "MAIN MENU", () => GameManager.Instance.ToMainMenu(), 480f);
        }
    }
}
