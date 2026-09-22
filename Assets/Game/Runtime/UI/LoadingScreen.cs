using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI
{
    public class LoadingScreen : UIScreen
    {
        RectTransform _fill;
        Text _percent;

        protected override void Build()
        {
            UIFactory.MakeFullscreenImage(transform, "Background", Theme.Background);

            var column = UIFactory.MakeColumn(transform, "Content", 18f, 720f);
            column.anchorMin = new Vector2(0.5f, 0.5f);
            column.anchorMax = new Vector2(0.5f, 0.5f);
            column.pivot = new Vector2(0.5f, 0.5f);
            column.anchoredPosition = Vector2.zero;

            UIFactory.MakeLabel(column, "Header", "LOADING", Theme.FontHeader, Theme.Text);

            _fill = UIFactory.MakeProgressBar(column, 720f, 28f);

            _percent = UIFactory.MakeLabel(column, "Percent", "0%", Theme.FontSmall, Theme.TextDim);
        }

        void Update()
        {
            float p = SceneLoader.Instance != null ? SceneLoader.Instance.Progress : 0f;
            _fill.anchorMax = new Vector2(p, 1f);
            _percent.text = Mathf.RoundToInt(p * 100f) + "%";
        }
    }
}
