using UnityEngine;

namespace SamanthaTrainer.Payload.UI
{
    // Cached fonts, textures and GUIStyles for the menu.
    //
    // Everything here is built once and reused. Allocating GUIStyles or textures inside
    // OnGUI runs them every frame for every element, which is both wasteful and the reason
    // text ends up rendering inconsistently.
    public static class MenuStyle
    {
        private static Font _font;
        private static Font _fontBold;
        private static Texture2D _white;
        private static Texture2D _gradient;
        private static Color32 _gradientTop, _gradientBottom;

        private static GUIStyle _label;

        // Condensed grotesques first, to get near the look of the original menu.
        // Bahnschrift ships with Windows 10; the rest are progressively safer fallbacks.
        private static readonly string[] FontCandidates =
        {
            "Bahnschrift SemiBold Condensed",
            "Bahnschrift SemiCondensed",
            "Bahnschrift",
            "Segoe UI Semibold",
            "Segoe UI",
            "Tahoma",
            "Verdana",
            "Arial"
        };

        public static Font Font
        {
            get
            {
                if (_font == null) _font = LoadFont(FontStyle.Normal);
                return _font;
            }
        }

        public static Font FontBold
        {
            get
            {
                if (_fontBold == null) _fontBold = LoadFont(FontStyle.Bold);
                return _fontBold;
            }
        }

        private static Font LoadFont(FontStyle style)
        {
            string[] installed = Font.GetOSInstalledFontNames();

            foreach (var candidate in FontCandidates)
            {
                bool present = false;
                foreach (var name in installed)
                {
                    if (!string.Equals(name, candidate, System.StringComparison.OrdinalIgnoreCase)) continue;
                    present = true;
                    break;
                }
                if (!present) continue;

                // 32 is just the atlas baking size; GUIStyle.fontSize scales dynamic fonts freely.
                var font = Font.CreateDynamicFontFromOSFont(candidate, 32);
                if (font != null)
                {
                    font.hideFlags = HideFlags.HideAndDontSave;
                    return font;
                }
            }

            return GUI.skin.label.font;
        }

        public static Texture2D White
        {
            get
            {
                if (_white == null)
                {
                    _white = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    _white.SetPixel(0, 0, Color.white);
                    _white.Apply();
                }
                return _white;
            }
        }

        // A 1xN vertical ramp, stretched by GUI.DrawTexture to fill the banner.
        public static Texture2D Gradient(Color32 top, Color32 bottom)
        {
            bool stale = _gradient == null
                      || !Same(_gradientTop, top)
                      || !Same(_gradientBottom, bottom);

            if (stale)
            {
                const int steps = 64;
                if (_gradient == null)
                {
                    _gradient = new Texture2D(1, steps, TextureFormat.RGBA32, false)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Bilinear
                    };
                }

                for (int i = 0; i < steps; i++)
                {
                    // Row 0 is the bottom of a Unity texture, so invert for a top-down ramp.
                    float t = 1f - i / (float)(steps - 1);
                    _gradient.SetPixel(0, i, Color.Lerp(bottom, top, 1f - t));
                }
                _gradient.Apply();

                _gradientTop = top;
                _gradientBottom = bottom;
            }

            return _gradient;
        }

        private static bool Same(Color32 a, Color32 b)
            => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;

        // Shared label style; callers set size/colour/alignment per draw.
        public static GUIStyle Label
        {
            get
            {
                if (_label == null)
                {
                    _label = new GUIStyle
                    {
                        wordWrap = false,
                        clipping = TextClipping.Clip,
                        richText = false
                    };
                }
                return _label;
            }
        }

        // ─── Drawing primitives ───────────────────────────────────────────────────
        public static void Fill(Rect rect, Color color)
        {
            if (rect.width <= 0f || rect.height <= 0f) return;
            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, White);
            GUI.color = prev;
        }

        public static void FillGradient(Rect rect, Color32 top, Color32 bottom)
        {
            if (rect.width <= 0f || rect.height <= 0f) return;
            var prev = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, Gradient(top, bottom), ScaleMode.StretchToFill);
            GUI.color = prev;
        }

        // Text with a 1px drop shadow, so it stays legible over the game.
        public static void Text(Rect rect, string text, int size, Color color,
                                TextAnchor anchor = TextAnchor.MiddleLeft, bool bold = false)
        {
            if (string.IsNullOrEmpty(text)) return;

            var style = Label;
            style.font = bold ? FontBold : Font;
            style.fontSize = size;
            style.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            style.alignment = anchor;

            style.normal.textColor = new Color(0f, 0f, 0f, color.a * 0.6f);
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);

            style.normal.textColor = color;
            GUI.Label(rect, text, style);
        }
    }
}
