using System;
using System.Collections.Generic;

namespace SamanthaTrainer.Payload.UI
{
    public enum MenuItemType { Action, Toggle, Slider, List, Submenu, Separator }

    public class MenuItem
    {
        public MenuItemType Type = MenuItemType.Action;
        public string Label = "";
        public string Description = "";

        // Action
        public Action OnActivate;

        // Shown as a toast after OnActivate runs. Used for actions with no visible result.
        public string Confirmation;

        // Toggle
        public Func<bool> GetToggle;
        public Action<bool> SetToggle;

        // Slider
        public Func<float> GetValue;
        public Action<float> SetValue;
        public float Min = 0f, Max = 1f, Step = 0.05f;
        public string Format = "0.00";

        // List (left/right through a set of options)
        public Func<int> GetIndex;
        public Action<int> SetIndex;
        public Func<IList<string>> GetOptions;

        // Submenu
        public MenuPage Submenu;

        public bool Enabled = true;

        // ─── Factory helpers ──────────────────────────────────────────────────────
        public static MenuItem Action_(string label, Action onActivate, string desc = "",
                                      string confirmation = null)
            => new MenuItem
            {
                Type = MenuItemType.Action, Label = label, OnActivate = onActivate,
                Description = desc, Confirmation = confirmation
            };

        public static MenuItem Toggle(string label, Func<bool> get, Action<bool> set, string desc = "")
            => new MenuItem { Type = MenuItemType.Toggle, Label = label, GetToggle = get, SetToggle = set, Description = desc };

        public static MenuItem Slider(string label, Func<float> get, Action<float> set,
                                      float min, float max, float step, string format = "0.00", string desc = "")
            => new MenuItem
            {
                Type = MenuItemType.Slider, Label = label, GetValue = get, SetValue = set,
                Min = min, Max = max, Step = step, Format = format, Description = desc
            };

        public static MenuItem List(string label, Func<int> getIndex, Action<int> setIndex,
                                    Func<IList<string>> options, string desc = "")
            => new MenuItem
            {
                Type = MenuItemType.List, Label = label, GetIndex = getIndex,
                SetIndex = setIndex, GetOptions = options, Description = desc
            };

        public static MenuItem Sub(string label, MenuPage page, string desc = "")
            => new MenuItem { Type = MenuItemType.Submenu, Label = label, Submenu = page, Description = desc };

        public static MenuItem Separator(string label = "")
            => new MenuItem { Type = MenuItemType.Separator, Label = label, Enabled = false };
    }

    public class MenuPage
    {
        public string Title;
        public string Subtitle;
        public List<MenuItem> Items = new List<MenuItem>();

        // Rebuilt on open, for pages whose contents depend on live game state.
        public Action<MenuPage> OnOpen;

        // Object the side preview should frame. Null means no preview.
        public Func<UnityEngine.Transform> PreviewTarget;

        // Caption shown on the preview panel header.
        public string PreviewCaption;

        // Framing bias for the preview. Below 1 moves the camera in closer, which the
        // whole-character pages use so they are not stuck at a distant full-body shot.
        public float PreviewZoom = 1f;

        // Message shown when the preview has nothing to draw. Evaluated each frame so it can
        // explain the actual reason - an unworn garment has its renderers disabled, so there
        // is no geometry to frame until you put it on.
        public Func<string> PreviewEmptyMessage;

        public int Selected;
        public int Scroll;

        public MenuPage(string title, string subtitle = "")
        {
            Title = title;
            Subtitle = subtitle;
        }

        public MenuPage Add(MenuItem item) { Items.Add(item); return this; }
    }
}
