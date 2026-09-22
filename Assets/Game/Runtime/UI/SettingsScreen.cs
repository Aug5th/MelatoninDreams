using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI
{
    public class SettingsScreen : UIScreen
    {
        public override bool IsOverlay { get { return true; } }
        public override bool ConsumesPauseKey { get { return true; } }

        Slider _master, _music, _sfx;
        Text _resetLabel;

        protected override void Build()
        {
            UIFactory.MakeFullscreenImage(transform, "Scrim", Theme.Scrim);

            // Height 880: 11 children (646px) + 10 gaps of 12px + 80px padding = 846px.
            // At 720 the bottom two buttons spill outside the panel.
            var panel = UIFactory.MakePanel(transform, "Panel", new Vector2(980f, 880f));
            var panelRt = (RectTransform)panel.transform;
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;

            var content = UIFactory.Stretch(panelRt, "Content");
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(56, 56, 40, 40);
            vlg.spacing = 12f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            UIFactory.MakeLabel(content, "Header", "SETTINGS", Theme.FontHeader, Theme.Text);
            UIFactory.MakeSpacer(content, 16f);

            var s = SaveSystem.Settings;

            _master = UIFactory.MakeSliderRow(content, "Master", s.masterVolume, v =>
            {
                AudioManager.Instance.MasterVolume = v;
            });

            _music = UIFactory.MakeSliderRow(content, "Music", s.musicVolume, v =>
            {
                AudioManager.Instance.MusicVolume = v;
            });

            _sfx = UIFactory.MakeSliderRow(content, "SFX", s.sfxVolume, v =>
            {
                AudioManager.Instance.SfxVolume = v;
                AudioManager.Instance.PlaySfx(AudioManager.Instance.ClickSfx);
            });

            UIFactory.MakeToggleRow(content, "Fullscreen", s.fullscreen, v =>
            {
                s.fullscreen = v;
#if !UNITY_WEBGL
                Screen.fullScreen = v;
#endif
            });

            UIFactory.MakeToggleRow(content, "Screen shake", s.screenShake, v => s.screenShake = v);

            var names = QualitySettings.names;
            int qualityIndex = s.qualityLevel >= 0 ? Mathf.Clamp(s.qualityLevel, 0, names.Length - 1)
                                                   : QualitySettings.GetQualityLevel();
            UIFactory.MakeCycleRow(content, "Quality", names, qualityIndex, i =>
            {
                s.qualityLevel = i;
                QualitySettings.SetQualityLevel(i, true);
            });

            UIFactory.MakeSpacer(content, 18f);

            var resetBtn = UIFactory.MakeButton(content, "RESET SAVE DATA", () =>
            {
                SaveSystem.ResetSave();
                if (_resetLabel != null) _resetLabel.text = "SAVE DATA CLEARED";
            });
            _resetLabel = resetBtn.GetComponentInChildren<Text>();

            UIFactory.MakeButton(content, "BACK", Close);
        }

        public override void OnShow()
        {
            var s = SaveSystem.Settings;
            _master.SetValueWithoutNotify(s.masterVolume);
            _music.SetValueWithoutNotify(s.musicVolume);
            _sfx.SetValueWithoutNotify(s.sfxVolume);
            if (_resetLabel != null) _resetLabel.text = "RESET SAVE DATA";
        }

        public override void OnPauseKey() { Close(); }

        void Close()
        {
            SaveSystem.SaveSettings();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBack();
            UI.Pop();
        }
    }
}
