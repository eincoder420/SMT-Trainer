using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace SamanthaTrainer.Payload.Features
{
    public struct ColorPreset
    {
        public string Name;
        public Color  Color;
    }

    // Named colours saved next to the game's own save data.
    //
    // Stored as plain "name=r,g,b,a" lines rather than JSON: the format stays hand-editable
    // and it avoids depending on the exact Newtonsoft build the game happens to ship.
    // Channels are written unclamped so HDR values above 1 survive a round trip.
    public static class ColorPresets
    {
        private static readonly List<ColorPreset> _presets = new List<ColorPreset>();
        private static bool _loaded;

        // Kept beside the Body/Clothing presets so everything lives in one place.
        public static string FilePath => Path.Combine(PresetStore.RootFolder, "palettes.txt");

        public static IList<ColorPreset> All
        {
            get { EnsureLoaded(); return _presets; }
        }

        public static int Count { get { EnsureLoaded(); return _presets.Count; } }

        public static IList<string> Names()
        {
            EnsureLoaded();
            var names = new List<string>();
            foreach (var p in _presets) names.Add(p.Name);
            if (names.Count == 0) names.Add("(none)");
            return names;
        }

        public static void Add(string name, Color color)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(name)) name = "Preset " + (_presets.Count + 1);

            for (int i = 0; i < _presets.Count; i++)
            {
                if (_presets[i].Name != name) continue;
                _presets[i] = new ColorPreset { Name = name, Color = color };
                Save();
                return;
            }

            _presets.Add(new ColorPreset { Name = name, Color = color });
            Save();
        }

        public static void RemoveAt(int index)
        {
            EnsureLoaded();
            if (index < 0 || index >= _presets.Count) return;
            _presets.RemoveAt(index);
            Save();
        }

        public static bool TryGet(int index, out Color color)
        {
            EnsureLoaded();
            if (index < 0 || index >= _presets.Count) { color = Color.white; return false; }
            color = _presets[index].Color;
            return true;
        }

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            Load();
        }

        public static void Load()
        {
            _presets.Clear();
            try
            {
                if (!File.Exists(FilePath)) return;

                foreach (var raw in File.ReadAllLines(FilePath))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line[0] == '#') continue;

                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    var name = line.Substring(0, eq).Trim();
                    var parts = line.Substring(eq + 1).Split(',');
                    if (parts.Length < 3) continue;

                    if (!TryFloat(parts[0], out float r)) continue;
                    if (!TryFloat(parts[1], out float g)) continue;
                    if (!TryFloat(parts[2], out float b)) continue;
                    float a = 1f;
                    if (parts.Length > 3) TryFloat(parts[3], out a);

                    _presets.Add(new ColorPreset { Name = name, Color = new Color(r, g, b, a) });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SMT-Trainer] could not read presets: " + ex.Message);
            }
        }

        public static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var lines = new List<string> { "# SMT-Trainer colour presets: name=r,g,b,a" };
                foreach (var p in _presets)
                    lines.Add($"{p.Name}={F(p.Color.r)},{F(p.Color.g)},{F(p.Color.b)},{F(p.Color.a)}");

                File.WriteAllLines(FilePath, lines.ToArray());
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SMT-Trainer] could not save presets: " + ex.Message);
            }
        }

        // ─── Hex conversion ───────────────────────────────────────────────────────
        // RRGGBB or RRGGBBAA, with or without a leading '#'.
        public static bool TryParseHex(string text, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrEmpty(text)) return false;

            var s = text.Trim().TrimStart('#');
            if (s.Length != 6 && s.Length != 8) return false;

            if (!byte.TryParse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r)) return false;
            if (!byte.TryParse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g)) return false;
            if (!byte.TryParse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b)) return false;

            byte a = 255;
            if (s.Length == 8 &&
                !byte.TryParse(s.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a))
                return false;

            color = new Color32(r, g, b, a);
            return true;
        }

        // Hex is 8-bit, so values above 1.0 cannot be represented - they clamp here.
        // The RGBA sliders remain the way to set HDR colours.
        public static string ToHex(Color c)
        {
            var c32 = (Color32)new Color(
                Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), Mathf.Clamp01(c.a));
            return $"#{c32.r:X2}{c32.g:X2}{c32.b:X2}{c32.a:X2}";
        }

        private static bool TryFloat(string s, out float v)
            => float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);

        private static string F(float v) => v.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
