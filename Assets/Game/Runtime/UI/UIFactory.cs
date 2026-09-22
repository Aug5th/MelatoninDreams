using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameKit.UI
{
    /// <summary>
    /// Builds UI in code. If a skin is found in Assets/Arts/UI it uses the real sprites,
    /// otherwise it falls back to rounded sprites generated at runtime - both paths work.
    /// Screens only ever call the helpers here; they never assemble raw UI themselves.
    /// </summary>
    public static class UIFactory
    {
        static Font _font;
        static Sprite _rounded;
        static Sprite _circle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { _font = null; _rounded = null; _circle = null; }

        public static Font UIFont
        {
            get
            {
                if (_font != null) return _font;
                UISkin.EnsureLoaded();
                _font = UISkin.Font != null ? UISkin.Font : LoadBuiltinFont();
                return _font;
            }
        }

        public static Sprite RoundedSprite
        {
            get { if (_rounded == null) _rounded = MakeRounded(48, 14); return _rounded; }
        }

        public static Sprite CircleSprite
        {
            get { if (_circle == null) _circle = MakeCircle(96); return _circle; }
        }

        /// <summary>The skin's sprite, or the fallback when the skin is missing that file.</summary>
        static Sprite Skinned(Sprite fromSkin, Sprite fallback)
        {
            UISkin.EnsureLoaded();
            return fromSkin != null ? fromSkin : fallback;
        }

        static bool HasSkin
        {
            get { UISkin.EnsureLoaded(); return UISkin.Loaded; }
        }

        static Font LoadBuiltinFont()
        {
            var f = TryBuiltin("LegacyRuntime.ttf");
            if (f != null) return f;
            f = TryBuiltin("Arial.ttf");
            if (f != null) return f;
            return Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        static Font TryBuiltin(string resourceName)
        {
            try { return Resources.GetBuiltinResource<Font>(resourceName); }
            catch { return null; }
        }

        // ---------- Fallback sprites generated at runtime ----------

        static Sprite MakeRounded(int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(Mathf.Max(radius - (x + 0.5f), (x + 0.5f) - (size - radius)), 0f);
                    float dy = Mathf.Max(Mathf.Max(radius - (y + 0.5f), (y + 0.5f) - (size - radius)), 0f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(radius - d + 0.5f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }

            tex.SetPixels32(px);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                                 SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        static Sprite MakeCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float r = size * 0.5f;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    float a = Mathf.Clamp01(r - d + 0.5f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }

            tex.SetPixels32(px);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        // ---------- Primitive ----------

        public static RectTransform NewRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform Stretch(Transform parent, string name)
        {
            var rt = NewRect(parent, name);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image MakeImage(Transform parent, string name, Color color,
                                      Sprite sprite = null, Image.Type type = Image.Type.Sliced)
        {
            var rt = NewRect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            if (sprite != null) { img.sprite = sprite; img.type = type; }
            return img;
        }

        public static Image MakeFullscreenImage(Transform parent, string name, Color color)
        {
            var rt = Stretch(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text MakeLabel(Transform parent, string name, string text, int fontSize,
                                     Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var rt = NewRect(parent, name);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = UIFont;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            rt.sizeDelta = new Vector2(600f, fontSize * 1.4f);
            return t;
        }

        /// <summary>Aspect-preserving icon that can be tinted.</summary>
        public static Image MakeIcon(Transform parent, string name, Sprite sprite, Color color, float size)
        {
            var img = MakeImage(parent, name, color, sprite, Image.Type.Simple);
            img.preserveAspect = true;
            img.raycastTarget = false;
            ((RectTransform)img.transform).sizeDelta = new Vector2(size, size);
            return img;
        }

        public static Image MakePanel(Transform parent, string name, Vector2 size)
        {
            var sprite = Skinned(UISkin.Panel, RoundedSprite);
            var img = MakeImage(parent, name, Theme.Panel, sprite);
            ((RectTransform)img.transform).sizeDelta = size;
            return img;
        }

        // ---------- Buttons ----------

        public static Button MakeButton(Transform parent, string label, Action onClick,
                                        float width = 460f, float height = 78f, bool primary = false)
        {
            var rt = NewRect(parent, "Button - " + label);
            rt.sizeDelta = new Vector2(width, height);

            var sprite = primary
                ? Skinned(UISkin.ButtonPrimary, RoundedSprite)
                : Skinned(UISkin.Button, RoundedSprite);

            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.colors = ButtonColors(primary);

            var labelColor = HasSkin
                ? (primary ? Color.white : Theme.TextOnLight)
                : (primary ? Theme.Background : Theme.Text);

            var text = MakeLabel(rt, "Label", label, Theme.FontBody, labelColor);
            var trt = (RectTransform)text.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;

            AddClick(btn, onClick);
            return btn;
        }

        /// <summary>Square icon-only button - used for pause, arrows and close.</summary>
        public static Button MakeIconButton(Transform parent, string name, Sprite icon, Action onClick,
                                            float size = 72f, bool primary = false)
        {
            var rt = NewRect(parent, "Button - " + name);
            rt.sizeDelta = new Vector2(size, size);

            var sprite = primary
                ? Skinned(UISkin.ButtonPrimary, RoundedSprite)
                : Skinned(UISkin.ButtonSquare, RoundedSprite);

            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.colors = ButtonColors(primary);

            var iconColor = HasSkin ? Theme.TextOnLight : Theme.Text;
            if (icon != null)
            {
                var iconImg = MakeIcon(rt, "Icon", icon, iconColor, size * 0.45f);
                var irt = (RectTransform)iconImg.transform;
                irt.anchorMin = new Vector2(0.5f, 0.5f);
                irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = Vector2.zero;
            }
            else
            {
                var text = MakeLabel(rt, "Label", name, Theme.FontBody, iconColor);
                var trt = (RectTransform)text.transform;
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = Vector2.zero;
                trt.offsetMax = Vector2.zero;
            }

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = size;
            le.minHeight = size;
            le.preferredWidth = size;

            AddClick(btn, onClick);
            return btn;
        }

        static ColorBlock ButtonColors(bool primary)
        {
            var colors = ColorBlock.defaultColorBlock;

            if (HasSkin)
            {
                colors.normalColor      = Theme.SkinTint;
                colors.highlightedColor = Theme.SkinHover;
                colors.pressedColor     = Theme.SkinPress;
                colors.selectedColor    = Theme.SkinTint;
                colors.disabledColor    = Theme.SkinDisabled;
            }
            else
            {
                colors.normalColor      = primary ? Theme.Accent : Theme.ButtonNormal;
                colors.highlightedColor = primary ? Theme.Accent * 1.15f : Theme.ButtonHover;
                colors.pressedColor     = primary ? Theme.Accent * 0.8f : Theme.ButtonPress;
                colors.selectedColor    = colors.normalColor;
                colors.disabledColor    = new Color(0.25f, 0.27f, 0.33f, 0.6f);
            }

            colors.fadeDuration = 0.08f;
            return colors;
        }

        static void AddClick(Button btn, Action onClick)
        {
            btn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (onClick != null) onClick();
            });
        }

        // ---------- Layout ----------

        public static RectTransform MakeColumn(Transform parent, string name, float spacing = 18f, float width = 520f)
        {
            var rt = NewRect(parent, name);
            rt.sizeDelta = new Vector2(width, 0f);

            var vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var fitter = rt.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            return rt;
        }

        public static RectTransform MakeRow(Transform parent, string name, float height = 66f)
        {
            var rt = NewRect(parent, name);
            rt.sizeDelta = new Vector2(0f, height);

            var hlg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;

            return rt;
        }

        public static LayoutElement SetWidth(Component target, float preferred, float flexible = 0f)
        {
            var le = target.gameObject.GetComponent<LayoutElement>();
            if (le == null) le = target.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = preferred;
            le.flexibleWidth = flexible;
            return le;
        }

        public static void MakeSpacer(Transform parent, float height)
        {
            var rt = NewRect(parent, "Spacer");
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
        }

        /// <summary>Faint horizontal rule that separates groups inside a panel.</summary>
        public static Image MakeDivider(Transform parent, float height = 6f)
        {
            var img = MakeImage(parent, "Divider", Theme.TrackColor,
                                Skinned(UISkin.Divider, RoundedSprite));
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
            img.raycastTarget = false;
            return img;
        }

        // ---------- Progress bar ----------

        /// <summary>Returns the fill RectTransform; set anchorMax.x to change progress.</summary>
        public static RectTransform MakeProgressBar(Transform parent, float width, float height)
        {
            var track = MakeImage(parent, "Progress Track", Theme.TrackColor,
                                  Skinned(UISkin.ProgressTrack, RoundedSprite));
            var trackRt = (RectTransform)track.transform;
            trackRt.sizeDelta = new Vector2(width, height);

            var le = track.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;

            var fill = MakeImage(trackRt, "Progress Fill", HasSkin ? Color.white : Theme.Accent,
                                 Skinned(UISkin.ProgressFill, RoundedSprite));
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fill.raycastTarget = false;

            return fillRt;
        }

        // ---------- Settings rows ----------

        public static Slider MakeSliderRow(Transform parent, string label, float value, Action<float> onChanged)
        {
            var row = MakeRow(parent, "Row - " + label);

            var caption = MakeLabel(row, "Caption", label, Theme.FontBody, Theme.Text, TextAnchor.MiddleLeft);
            SetWidth(caption, 300f);

            var sliderRt = NewRect(row, "Slider");
            SetWidth(sliderRt, 0f, 1f);
            var slider = sliderRt.gameObject.AddComponent<Slider>();

            float barHeight = HasSkin ? 28f : 14f;

            var bg = MakeImage(sliderRt, "Background", Theme.TrackColor,
                               Skinned(UISkin.SliderTrack, RoundedSprite));
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = new Vector2(0f, 0.5f);
            bgRt.anchorMax = new Vector2(1f, 0.5f);
            bgRt.sizeDelta = new Vector2(0f, barHeight);
            bgRt.anchoredPosition = Vector2.zero;

            var fillArea = NewRect(sliderRt, "Fill Area");
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.sizeDelta = new Vector2(-24f, barHeight);
            fillArea.anchoredPosition = Vector2.zero;

            var fill = MakeImage(fillArea, "Fill", HasSkin ? Color.white : Theme.Accent,
                                 Skinned(UISkin.SliderFill, RoundedSprite));
            var fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = new Vector2(24f, 0f);

            var handleArea = NewRect(sliderRt, "Handle Slide Area");
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(18f, 0f);
            handleArea.offsetMax = new Vector2(-18f, 0f);

            // Round handle: always drawn Simple, 9-slicing would distort the circle.
            float handleSize = HasSkin ? 40f : 30f;
            var handle = MakeImage(handleArea, "Handle", Color.white,
                                   Skinned(UISkin.SliderHandle, CircleSprite), Image.Type.Simple);
            handle.preserveAspect = true;
            var handleRt = (RectTransform)handle.transform;
            handleRt.anchorMin = new Vector2(0f, 0.5f);
            handleRt.anchorMax = new Vector2(0f, 0.5f);
            handleRt.sizeDelta = new Vector2(handleSize, handleSize);

            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(value);

            var readout = MakeLabel(row, "Value", Mathf.RoundToInt(value * 100f) + "%",
                                    Theme.FontSmall, Theme.TextDim, TextAnchor.MiddleRight);
            SetWidth(readout, 110f);

            slider.onValueChanged.AddListener(v =>
            {
                readout.text = Mathf.RoundToInt(v * 100f) + "%";
                if (onChanged != null) onChanged(v);
            });

            return slider;
        }

        public static Toggle MakeToggleRow(Transform parent, string label, bool value, Action<bool> onChanged)
        {
            var row = MakeRow(parent, "Row - " + label);

            var caption = MakeLabel(row, "Caption", label, Theme.FontBody, Theme.Text, TextAnchor.MiddleLeft);
            SetWidth(caption, 300f);

            var spacer = NewRect(row, "Spacer");
            SetWidth(spacer, 0f, 1f);

            const float boxSize = 52f;

            var box = MakeImage(row, "Toggle", HasSkin ? Color.white : Theme.TrackColor,
                                Skinned(UISkin.ToggleOff, RoundedSprite));
            var boxRt = (RectTransform)box.transform;
            boxRt.sizeDelta = new Vector2(boxSize, boxSize);

            var boxLe = SetWidth(box, boxSize);
            boxLe.preferredHeight = boxSize;
            boxLe.minHeight = boxSize;
            boxLe.flexibleHeight = 0f;

            var toggle = box.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.transition = Selectable.Transition.None;

            // Checkmark drawn Simple so the tick shape is not stretched.
            var check = MakeImage(boxRt, "Checkmark", HasSkin ? Color.white : Theme.Accent,
                                  Skinned(UISkin.ToggleOn, CircleSprite), Image.Type.Simple);
            check.preserveAspect = true;
            var checkRt = (RectTransform)check.transform;
            checkRt.anchorMin = Vector2.zero;
            checkRt.anchorMax = Vector2.one;
            float inset = UISkin.ToggleOn != null ? 0f : 12f;
            checkRt.offsetMin = new Vector2(inset, inset);
            checkRt.offsetMax = new Vector2(-inset, -inset);
            check.raycastTarget = false;

            // Toggle shows and hides this graphic based on its state.
            toggle.graphic = check;
            toggle.SetIsOnWithoutNotify(value);
            check.enabled = value;

            toggle.onValueChanged.AddListener(v =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                if (onChanged != null) onChanged(v);
            });

            return toggle;
        }

        public static void MakeCycleRow(Transform parent, string label, string[] options, int index,
                                        Action<int> onChanged)
        {
            var row = MakeRow(parent, "Row - " + label);

            var caption = MakeLabel(row, "Caption", label, Theme.FontBody, Theme.Text, TextAnchor.MiddleLeft);
            SetWidth(caption, 300f);

            var spacer = NewRect(row, "Spacer");
            SetWidth(spacer, 0f, 1f);

            int current = Mathf.Clamp(index, 0, Mathf.Max(0, options.Length - 1));
            Text valueLabel = null;

            Action<int> step = delta =>
            {
                if (options.Length == 0) return;
                current = (current + delta + options.Length) % options.Length;
                if (valueLabel != null) valueLabel.text = options[current];
                if (onChanged != null) onChanged(current);
            };

            UISkin.EnsureLoaded();

            var prev = UISkin.ArrowLeft != null
                ? MakeIconButton(row, "Prev", UISkin.ArrowLeft, () => step(-1), 54f)
                : MakeButton(row, "<", () => step(-1), 54f, 54f);
            SetWidth(prev, 54f);

            valueLabel = MakeLabel(row, "Value", options.Length > 0 ? options[current] : "-",
                                   Theme.FontSmall, Theme.Text, TextAnchor.MiddleCenter);
            SetWidth(valueLabel, 240f);

            var next = UISkin.ArrowRight != null
                ? MakeIconButton(row, "Next", UISkin.ArrowRight, () => step(1), 54f)
                : MakeButton(row, ">", () => step(1), 54f, 54f);
            SetWidth(next, 54f);
        }
    }
}
