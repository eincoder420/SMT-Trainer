using System.Collections.Generic;
using UnityEngine;

namespace SamanthaTrainer.Payload.UI
{
    // IMGUI reimplementation of the TML trainer's NativeUI-style menu.
    //
    // Laid out the way NativeUI actually stacks: banner, subtitle strip with the item
    // counter, item rows, then a description box. The payload runs inside the game's Mono
    // runtime, so this draws with IMGUI rather than the original's WPF/DirectX.
    public class NativeMenu
    {
        // Design sizes, in menu-space units. Everything is multiplied by Scale on draw so
        // the menu keeps its proportions from 1080p up to 4K.
        private const float WIDTH       = 420f;
        private const float BANNER_H    = 88f;
        private const float SUBTITLE_H  = 32f;
        private const float ITEM_H      = 34f;
        private const float DESC_H      = 30f;
        private const float PAD_X       = 14f;
        private const int   MAX_VISIBLE = 10;

        private const float ORIGIN_X = 42f;
        private const float ORIGIN_Y = 38f;

        private readonly Stack<MenuPage> _stack = new Stack<MenuPage>();

        private float _repeatAt;
        private const float REPEAT_DELAY = 0.35f;
        private const float REPEAT_RATE  = 0.06f;

        public bool Visible;
        public MenuPage Root { get; private set; }

        // Live character preview, shown beside pages that ask for one.
        public readonly PreviewPanel Preview = new PreviewPanel();

        public System.Func<Transform> CurrentPreviewTarget => Visible ? Current.PreviewTarget : null;

        public float CurrentPreviewZoom => Visible ? Current.PreviewZoom : 1f;

        public NativeMenu(MenuPage root)
        {
            Root = root;
            _stack.Push(root);
        }

        private MenuPage Current => _stack.Peek();

        // Menus designed against a 1080p canvas; scale up on taller displays.
        private static float Scale => Mathf.Max(1f, Screen.height / 1080f);

        // ─── Input ────────────────────────────────────────────────────────────────
        public void HandleInput()
        {
            if (!Visible) return;

            if (Pressed(KeyCode.UpArrow, KeyCode.Keypad8))        Move(-1);
            else if (Pressed(KeyCode.DownArrow, KeyCode.Keypad2)) Move(1);
            else if (Pressed(KeyCode.LeftArrow, KeyCode.Keypad4)) Adjust(-1);
            else if (Pressed(KeyCode.RightArrow, KeyCode.Keypad6))Adjust(1);
            else if (Pressed(KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Keypad5)) Activate();
            else if (Pressed(KeyCode.Backspace, KeyCode.Keypad0)) Back();

            if (Current.PreviewTarget != null) Preview.HandleInput();
        }

        private bool Pressed(params KeyCode[] keys)
        {
            bool down = false, held = false;
            foreach (var k in keys)
            {
                if (Input.GetKeyDown(k)) down = true;
                if (Input.GetKey(k))     held = true;
            }

            if (down) { _repeatAt = Time.unscaledTime + REPEAT_DELAY; return true; }
            if (held && Time.unscaledTime >= _repeatAt) { _repeatAt = Time.unscaledTime + REPEAT_RATE; return true; }
            return false;
        }

        private void Move(int delta)
        {
            var page = Current;
            if (page.Items.Count == 0) return;

            int i = page.Selected;
            for (int guard = 0; guard < page.Items.Count; guard++)
            {
                i = (i + delta + page.Items.Count) % page.Items.Count;
                if (page.Items[i].Type != MenuItemType.Separator) break;
            }
            page.Selected = i;

            if (page.Selected < page.Scroll) page.Scroll = page.Selected;
            if (page.Selected >= page.Scroll + MAX_VISIBLE) page.Scroll = page.Selected - MAX_VISIBLE + 1;
            page.Scroll = Mathf.Clamp(page.Scroll, 0, Mathf.Max(0, page.Items.Count - MAX_VISIBLE));
        }

        private MenuItem Selected()
        {
            var page = Current;
            if (page.Items.Count == 0) return null;
            if (page.Selected < 0 || page.Selected >= page.Items.Count) return null;
            return page.Items[page.Selected];
        }

        private void Adjust(int dir)
        {
            var item = Selected();
            if (item == null || !item.Enabled) return;

            switch (item.Type)
            {
                case MenuItemType.Slider:
                    item.SetValue(Mathf.Clamp(item.GetValue() + dir * item.Step, item.Min, item.Max));
                    break;

                case MenuItemType.List:
                {
                    var opts = item.GetOptions();
                    if (opts == null || opts.Count == 0) break;
                    item.SetIndex((item.GetIndex() + dir + opts.Count) % opts.Count);
                    break;
                }

                case MenuItemType.Toggle:
                    item.SetToggle(!item.GetToggle());
                    break;
            }
        }

        private void Activate()
        {
            var item = Selected();
            if (item == null || !item.Enabled) return;

            switch (item.Type)
            {
                case MenuItemType.Toggle:
                    item.SetToggle(!item.GetToggle());
                    break;
                case MenuItemType.Action:
                    item.OnActivate?.Invoke();
                    if (!string.IsNullOrEmpty(item.Confirmation)) Toast.Show(item.Confirmation);
                    break;
                case MenuItemType.Submenu:
                    if (item.Submenu != null)
                    {
                        item.Submenu.OnOpen?.Invoke(item.Submenu);
                        _stack.Push(item.Submenu);
                    }
                    break;
            }
        }

        private void Back()
        {
            if (_stack.Count > 1) _stack.Pop();
            else Visible = false;
        }

        // ─── Drawing ──────────────────────────────────────────────────────────────
        public void Draw()
        {
            if (!Visible) return;

            float s     = Scale;
            var theme   = MenuThemes.Current;
            var page    = Current;
            float width = WIDTH * s;
            float x     = ORIGIN_X * s;
            float y     = ORIGIN_Y * s;

            DrawBanner(theme, page, x, ref y, width, s);
            DrawSubtitle(theme, page, x, ref y, width, s);
            DrawItems(theme, page, x, ref y, width, s);
            DrawDescription(theme, x, y, width, s);

            if (page.PreviewTarget != null)
            {
                float gap = 10f * s;
                var rect = new Rect(x + width + gap, ORIGIN_Y * s + BANNER_H * s,
                                    270f * s, 380f * s);
                string empty = null;
                if (page.PreviewEmptyMessage != null)
                {
                    try { empty = page.PreviewEmptyMessage(); } catch { }
                }
                Preview.Draw(rect, theme, page.PreviewCaption, empty);
            }
        }

        private void DrawBanner(MenuTheme theme, MenuPage page, float x, ref float y, float w, float s)
        {
            var rect = new Rect(x, y, w, BANNER_H * s);
            MenuStyle.FillGradient(rect, theme.HdrTop, theme.HdrBot);

            // Accent hairline along the bottom of the banner.
            MenuStyle.Fill(new Rect(x, y + BANNER_H * s - 2f * s, w, 2f * s), theme.Accent);

            MenuStyle.Text(new Rect(x + PAD_X * s, y, w - PAD_X * 2f * s, BANNER_H * s),
                           page.Title, Mathf.RoundToInt(34f * s), Color.white,
                           TextAnchor.MiddleLeft, bold: true);

            y += BANNER_H * s;
        }

        private void DrawSubtitle(MenuTheme theme, MenuPage page, float x, ref float y, float w, float s)
        {
            var rect = new Rect(x, y, w, SUBTITLE_H * s);
            MenuStyle.Fill(rect, new Color32(0, 0, 0, 235));

            if (!string.IsNullOrEmpty(page.Subtitle))
                MenuStyle.Text(new Rect(x + PAD_X * s, y, w * 0.7f, SUBTITLE_H * s),
                               page.Subtitle.ToUpperInvariant(), Mathf.RoundToInt(13f * s),
                               theme.Accent, TextAnchor.MiddleLeft, bold: true);

            string counter = page.Items.Count == 0 ? "0 / 0" : $"{page.Selected + 1} / {page.Items.Count}";
            MenuStyle.Text(new Rect(x, y, w - PAD_X * s, SUBTITLE_H * s),
                           counter, Mathf.RoundToInt(13f * s), Color.white, TextAnchor.MiddleRight);

            y += SUBTITLE_H * s;
        }

        private void DrawItems(MenuTheme theme, MenuPage page, float x, ref float y, float w, float s)
        {
            int visible = Mathf.Min(MAX_VISIBLE, page.Items.Count);
            float rowH  = ITEM_H * s;

            for (int row = 0; row < visible; row++)
            {
                int index = page.Scroll + row;
                if (index >= page.Items.Count) break;

                var item    = page.Items[index];
                bool active = index == page.Selected;
                var rect    = new Rect(x, y + row * rowH, w, rowH);

                if (item.Type == MenuItemType.Separator)
                {
                    MenuStyle.Fill(rect, new Color32(0, 0, 0, 225));
                    MenuStyle.Text(new Rect(x + PAD_X * s, rect.y, w - PAD_X * 2f * s, rowH),
                                   item.Label, Mathf.RoundToInt(12f * s), theme.Accent2,
                                   TextAnchor.MiddleLeft, bold: true);
                    continue;
                }

                // Selected rows invert: bright accent bar with dark text, as NativeUI does.
                MenuStyle.Fill(rect, active ? (Color)theme.Accent : (Color)theme.BgColor);

                Color fg = !item.Enabled ? new Color(1f, 1f, 1f, 0.35f)
                         : active        ? new Color(0.08f, 0.06f, 0.12f, 1f)
                                         : new Color(0.92f, 0.90f, 0.96f, 1f);

                float labelW = w - PAD_X * 2f * s - 130f * s;
                MenuStyle.Text(new Rect(x + PAD_X * s, rect.y, labelW, rowH),
                               item.Label, Mathf.RoundToInt(16f * s), fg, TextAnchor.MiddleLeft);

                DrawBadge(theme, item, rect, active, s);
            }

            y += visible * rowH;

            // Scroll affordance when the page is longer than the window.
            if (page.Items.Count > MAX_VISIBLE)
            {
                var bar = new Rect(x, y, w, 3f * s);
                MenuStyle.Fill(bar, new Color32(0, 0, 0, 235));
                float t = page.Scroll / (float)(page.Items.Count - MAX_VISIBLE);
                float knobW = w * (MAX_VISIBLE / (float)page.Items.Count);
                MenuStyle.Fill(new Rect(x + (w - knobW) * t, y, knobW, 3f * s), theme.Accent);
                y += 3f * s;
            }
        }

        private void DrawBadge(MenuTheme theme, MenuItem item, Rect row, bool active, float s)
        {
            float right = row.x + row.width - PAD_X * s;
            Color fg = active ? new Color(0.08f, 0.06f, 0.12f, 1f) : (Color)theme.Value;

            switch (item.Type)
            {
                case MenuItemType.Toggle:
                {
                    bool on = item.GetToggle();
                    float bw = 40f * s, bh = 18f * s;
                    var box = new Rect(right - bw, row.y + (row.height - bh) * 0.5f, bw, bh);

                    MenuStyle.Fill(box, on
                        ? (active ? new Color(0.08f, 0.06f, 0.12f, 1f) : (Color)theme.Accent)
                        : new Color(1f, 1f, 1f, active ? 0.25f : 0.12f));

                    MenuStyle.Text(box, on ? "ON" : "OFF", Mathf.RoundToInt(11f * s),
                                   on ? (active ? (Color)theme.Accent : Color.black)
                                      : new Color(1f, 1f, 1f, 0.7f),
                                   TextAnchor.MiddleCenter, bold: true);
                    break;
                }

                case MenuItemType.Slider:
                {
                    float v = item.GetValue();
                    float t = Mathf.Approximately(item.Max, item.Min)
                            ? 0f : Mathf.Clamp01(Mathf.InverseLerp(item.Min, item.Max, v));

                    float bw = 78f * s, bh = 6f * s;
                    var bar = new Rect(right - bw, row.y + (row.height - bh) * 0.5f, bw, bh);

                    MenuStyle.Fill(bar, new Color(1f, 1f, 1f, active ? 0.25f : 0.12f));
                    MenuStyle.Fill(new Rect(bar.x, bar.y, bar.width * t, bar.height),
                                   active ? new Color(0.08f, 0.06f, 0.12f, 1f) : (Color)theme.Accent);

                    MenuStyle.Text(new Rect(right - bw - 66f * s, row.y, 60f * s, row.height),
                                   v.ToString(item.Format), Mathf.RoundToInt(13f * s), fg,
                                   TextAnchor.MiddleRight);
                    break;
                }

                case MenuItemType.List:
                {
                    var opts = item.GetOptions();
                    string text = "-";
                    if (opts != null && opts.Count > 0)
                        text = opts[Mathf.Clamp(item.GetIndex(), 0, opts.Count - 1)];

                    if (active) text = "◀  " + text + "  ▶";

                    MenuStyle.Text(new Rect(right - 200f * s, row.y, 200f * s, row.height),
                                   text, Mathf.RoundToInt(13f * s), fg, TextAnchor.MiddleRight);
                    break;
                }

                case MenuItemType.Submenu:
                    MenuStyle.Text(new Rect(right - 20f * s, row.y, 20f * s, row.height),
                                   "▶", Mathf.RoundToInt(13f * s), fg, TextAnchor.MiddleRight);
                    break;
            }
        }

        private void DrawDescription(MenuTheme theme, float x, float y, float w, float s)
        {
            var sel = Selected();
            string text = sel != null && !string.IsNullOrEmpty(sel.Description)
                        ? sel.Description
                        : "Arrows / Numpad  •  Enter select  •  Backspace back";

            var rect = new Rect(x, y + 3f * s, w, DESC_H * s);
            MenuStyle.Fill(rect, new Color32(0, 0, 0, 235));
            MenuStyle.Fill(new Rect(x, rect.y, w, 2f * s), theme.Accent);

            MenuStyle.Text(new Rect(x + PAD_X * s, rect.y, w - PAD_X * 2f * s, DESC_H * s),
                           text, Mathf.RoundToInt(12f * s), new Color(1f, 1f, 1f, 0.75f),
                           TextAnchor.MiddleLeft);
        }
    }
}
