using System;
using UnityEngine;

namespace SamanthaTrainer.Payload.UI
{
    // Modal single-line text entry, used for hex colours and preset names.
    // While it is active the menu ignores navigation keys so typing isn't swallowed.
    public class TextPrompt
    {
        private const string CONTROL_NAME = "SMT_PromptField";

        public bool Active { get; private set; }

        private string _title = "";
        private string _text = "";
        private string _error = "";
        private Action<string> _onConfirm;
        private bool _focusRequested;

        public void Open(string title, string initial, Action<string> onConfirm)
        {
            _title = title;
            _text = initial ?? "";
            _error = "";
            _onConfirm = onConfirm;
            _focusRequested = true;
            Active = true;
        }

        public void Close()
        {
            Active = false;
            _onConfirm = null;
            _error = "";
        }

        public void ShowError(string message) => _error = message;

        public void Draw()
        {
            if (!Active) return;

            var theme = MenuThemes.Current;

            float scale = Mathf.Max(1f, Screen.height / 1080f);
            float w = 400f * scale, h = 140f * scale;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            // Dim the scene so the prompt reads as modal.
            Fill(0, 0, Screen.width, Screen.height, new Color(0f, 0f, 0f, 0.55f));
            Fill(x, y, w, h, new Color32(20, 16, 32, 245));
            Fill(x, y, w, 32f * scale, theme.HdrMid);
            Fill(x, y + 32f * scale - 2f * scale, w, 2f * scale, theme.Accent);

            Label(new Rect(x + 14f * scale, y, w - 28f * scale, 32f * scale), _title, Mathf.RoundToInt(14f * scale), FontStyle.Bold,
                  Color.white, TextAnchor.MiddleLeft);

            var fieldRect = new Rect(x + 14f * scale, y + 50f * scale, w - 28f * scale, 28f * scale);

            // Enter/Escape must be handled before the TextField consumes the event.
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    var cb = _onConfirm;
                    var value = _text;
                    e.Use();
                    cb?.Invoke(value);   // may call ShowError and keep the prompt open
                    if (Active && string.IsNullOrEmpty(_error)) Close();
                    return;
                }
                if (e.keyCode == KeyCode.Escape)
                {
                    e.Use();
                    Close();
                    return;
                }
            }

            GUI.SetNextControlName(CONTROL_NAME);
            var style = new GUIStyle(GUI.skin.textField) { fontSize = Mathf.RoundToInt(15f * scale) };
            style.font = MenuStyle.Font;
            _text = GUI.TextField(fieldRect, _text, 64, style);

            if (_focusRequested)
            {
                GUI.FocusControl(CONTROL_NAME);
                _focusRequested = false;
            }

            string hint = string.IsNullOrEmpty(_error) ? "Enter to confirm  -  Esc to cancel" : _error;
            var hintColour = string.IsNullOrEmpty(_error)
                           ? new Color(1f, 1f, 1f, 0.5f)
                           : new Color(1f, 0.42f, 0.45f, 1f);
            Label(new Rect(x + 14f * scale, y + 88f * scale, w - 28f * scale, 24f * scale), hint, Mathf.RoundToInt(12f * scale), FontStyle.Normal,
                  hintColour, TextAnchor.MiddleLeft);
        }

        private static void Fill(float x, float y, float w, float h, Color color)
            => MenuStyle.Fill(new Rect(x, y, w, h), color);

        private static void Label(Rect rect, string text, int size, FontStyle style,
                                  Color color, TextAnchor anchor)
            => MenuStyle.Text(rect, text, size, color, anchor, style == FontStyle.Bold);
    }
}
