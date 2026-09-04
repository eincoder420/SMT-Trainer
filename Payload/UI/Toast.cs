using System.Collections.Generic;
using UnityEngine;

namespace SamanthaTrainer.Payload.UI
{
    // Transient confirmation popups. Actions that change state without any visible result
    // (unlocking, saving a preset) call Show so there is feedback that something happened.
    public static class Toast
    {
        private class Entry
        {
            public string Message;
            public float Born;
            public bool IsError;
        }

        private const float LIFETIME  = 2.6f;
        private const float SLIDE_IN  = 0.18f;
        private const float FADE_OUT  = 0.5f;
        private const int   MAX_SHOWN = 4;

        private static readonly List<Entry> _entries = new List<Entry>();

        public static void Show(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            _entries.Add(new Entry { Message = message, Born = Time.unscaledTime, IsError = false });
            Trim();
        }

        public static void Error(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            _entries.Add(new Entry { Message = message, Born = Time.unscaledTime, IsError = true });
            Trim();
        }

        private static void Trim()
        {
            while (_entries.Count > MAX_SHOWN) _entries.RemoveAt(0);
        }

        public static void Draw()
        {
            if (_entries.Count == 0) return;

            float scale = Mathf.Max(1f, Screen.height / 1080f);
            float w = 300f * scale;
            float h = 34f * scale;
            float gap = 6f * scale;
            float margin = 42f * scale;

            var theme = MenuThemes.Current;
            float now = Time.unscaledTime;

            // Newest sits at the bottom, older ones stack upward.
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var e = _entries[i];
                float age = now - e.Born;

                if (age > LIFETIME) { _entries.RemoveAt(i); continue; }

                // Ease out of the left edge on entry, fade away at the end of the lifetime.
                float slide = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / SLIDE_IN));
                float alpha = age > LIFETIME - FADE_OUT
                            ? Mathf.Clamp01((LIFETIME - age) / FADE_OUT)
                            : 1f;

                int fromBottom = _entries.Count - 1 - i;
                float x = margin - (1f - slide) * 40f * scale;
                float y = Screen.height - margin - h - fromBottom * (h + gap);

                var rect = new Rect(x, y, w, h);
                Color bar = e.IsError ? new Color(0.88f, 0.30f, 0.36f) : (Color)theme.Accent;

                MenuStyle.Fill(rect, new Color(0.04f, 0.03f, 0.07f, 0.93f * alpha));
                MenuStyle.Fill(new Rect(rect.x, rect.y, 3f * scale, rect.height),
                               new Color(bar.r, bar.g, bar.b, alpha));

                MenuStyle.Text(new Rect(rect.x + 12f * scale, rect.y, rect.width - 20f * scale, rect.height),
                               e.Message, Mathf.RoundToInt(13f * scale),
                               new Color(1f, 1f, 1f, alpha), TextAnchor.MiddleLeft);
            }
        }
    }
}
