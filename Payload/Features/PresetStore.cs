using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace SamanthaTrainer.Payload.Features
{
    public enum PresetKind { Body, Clothing }

    // Named Body and Clothing presets, kept as separate files under
    // Documents\SMT-Trainer\ so they can be saved, swapped and shared independently.
    //
    // Format is flat "key=value" text rather than JSON: it stays hand-editable and does
    // not depend on whichever Newtonsoft build the game happens to ship.
    public static class PresetStore
    {
        public static string RootFolder =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SMT-Trainer");

        public static string FolderFor(PresetKind kind)
            => Path.Combine(RootFolder, kind == PresetKind.Body ? "Body" : "Clothing");

        private static string PathFor(PresetKind kind, string name)
            => Path.Combine(FolderFor(kind), Sanitise(name) + ".txt");

        // Preset names available on disk, newest first.
        public static IList<string> List(PresetKind kind)
        {
            var names = new List<string>();
            try
            {
                var dir = FolderFor(kind);
                if (!Directory.Exists(dir)) return names;

                var files = new List<string>(Directory.GetFiles(dir, "*.txt"));
                files.Sort((a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));
                foreach (var f in files) names.Add(Path.GetFileNameWithoutExtension(f));
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SMT-Trainer] could not list presets: " + ex.Message);
            }
            return names;
        }

        public static bool Delete(PresetKind kind, string name)
        {
            try
            {
                var path = PathFor(kind, name);
                if (!File.Exists(path)) return false;
                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SMT-Trainer] could not delete preset: " + ex.Message);
                return false;
            }
        }

        // ─── Body ─────────────────────────────────────────────────────────────────
        public static bool SaveBody(string name)
        {
            var d = GameRefs.Data;
            if (d == null) return false;

            var w = new Writer();
            w.Add("kind", "body");
            w.Add("hairstyle",   d.Character.hairstyle);
            w.Add("skincolor",   d.Character.skincolor);
            w.Add("pussy_hairs", d.Character.pussy_hairs);
            w.Add("piercing",    d.Character.piercing);
            w.Add("eyeshadows",  d.Character.Eyeshadows);
            w.Add("boobs_size",  d.Character.boobs_size);
            w.Add("ass_size",    d.Character.ass_size);
            w.Add("fatness",     d.Character.fatness);
            w.Add("eye_size",    d.Character.eye_size);
            w.Add("hair_color",      d.Character.hair_color);
            w.Add("eye_color",       d.Character.eye_color);
            w.Add("lips_color",      d.Character.lips_color);
            w.Add("eyeshadow_color", d.Character.eyeshadow_color);
            w.Add("finger_color",    d.Character.finger_color);
            w.Add("pussy_color",     d.Character.pussy_color);

            return Write(PresetKind.Body, name, w);
        }

        public static bool LoadBody(string name)
        {
            var map = Read(PresetKind.Body, name);
            if (map == null) return false;

            var d = GameRefs.Data;
            if (d == null) return false;

            // Colours go through EditorFeatures so the materials update immediately.
            if (TryColor(map, "hair_color", out var c))      EditorFeatures.SetColor(ColorSlot.Hair, c);
            if (TryColor(map, "eye_color", out c))           EditorFeatures.SetColor(ColorSlot.Eyes, c);
            if (TryColor(map, "lips_color", out c))          EditorFeatures.SetColor(ColorSlot.Lips, c);
            if (TryColor(map, "eyeshadow_color", out c))     EditorFeatures.SetColor(ColorSlot.Eyeshadow, c);
            if (TryColor(map, "finger_color", out c))        EditorFeatures.SetColor(ColorSlot.Fingernails, c);
            if (TryColor(map, "pussy_color", out c))         EditorFeatures.SetColor(ColorSlot.Pubic, c);

            if (TryFloat(map, "boobs_size", out float f)) EditorFeatures.Boobs   = f;
            if (TryFloat(map, "ass_size", out f))         EditorFeatures.Ass     = f;
            if (TryFloat(map, "fatness", out f))          EditorFeatures.Fatness = f;
            if (TryFloat(map, "eye_size", out f))         EditorFeatures.EyeSize = f;

            if (TryInt(map, "piercing", out int i)) d.Character.piercing = i;
            if (TryBool(map, "eyeshadows", out bool b)) EditorFeatures.Eyeshadows = b;

            // Style setters call into the game and rebuild meshes, so they go last.
            if (TryInt(map, "hairstyle", out i))   EditorFeatures.Hairstyle  = i;
            if (TryInt(map, "skincolor", out i))   EditorFeatures.Skincolor  = i;
            if (TryInt(map, "pussy_hairs", out i)) EditorFeatures.PubicStyle = i;

            return true;
        }

        // ─── Clothing ─────────────────────────────────────────────────────────────
        public static bool SaveClothing(string name)
        {
            int count = ClothingFeatures.Count;
            if (count == 0) return false;

            var w = new Writer();
            w.Add("kind", "clothing");
            w.Add("count", count);

            for (int i = 0; i < count; i++)
            {
                w.Add($"slot{i}.worn",    ClothingFeatures.GetWorn(i));
                w.Add($"slot{i}.variant", ClothingFeatures.GetVariant(i));
                w.Add($"slot{i}.color",   ClothingFeatures.GetColor(i));
                w.Add($"slot{i}.tiling",  ClothingFeatures.GetTiling(i));
            }

            return Write(PresetKind.Clothing, name, w);
        }

        public static bool LoadClothing(string name)
        {
            var map = Read(PresetKind.Clothing, name);
            if (map == null) return false;

            // Saves from a different build may hold a different number of slots, so clamp
            // to whatever this game actually has rather than trusting the file.
            int count = ClothingFeatures.Count;
            if (TryInt(map, "count", out int saved)) count = Mathf.Min(count, saved);

            for (int i = 0; i < count; i++)
            {
                if (TryBool(map, $"slot{i}.worn", out bool worn))     ClothingFeatures.SetWorn(i, worn);
                if (TryInt(map, $"slot{i}.variant", out int variant)) ClothingFeatures.SetVariant(i, variant);
                if (TryColor(map, $"slot{i}.color", out var color))   ClothingFeatures.SetColor(i, color);
                if (TryVec2(map, $"slot{i}.tiling", out var tiling))  ClothingFeatures.SetTiling(i, tiling);
            }

            ClothingFeatures.ApplyAll();
            return true;
        }

        // ─── File IO ──────────────────────────────────────────────────────────────
        private static bool Write(PresetKind kind, string name, Writer w)
        {
            try
            {
                var dir = FolderFor(kind);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(PathFor(kind, name), w.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SMT-Trainer] could not save preset: " + ex.Message);
                return false;
            }
        }

        private static Dictionary<string, string> Read(PresetKind kind, string name)
        {
            try
            {
                var path = PathFor(kind, name);
                if (!File.Exists(path)) return null;

                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var raw in File.ReadAllLines(path))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line[0] == '#') continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    map[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
                }
                return map;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SMT-Trainer] could not read preset: " + ex.Message);
                return null;
            }
        }

        // Strip characters Windows will not accept in a file name.
        public static string Sanitise(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unnamed";
            var chars = name.Trim().ToCharArray();
            foreach (var bad in Path.GetInvalidFileNameChars())
                for (int i = 0; i < chars.Length; i++)
                    if (chars[i] == bad) chars[i] = '_';
            var cleaned = new string(chars).Trim();
            return cleaned.Length == 0 ? "Unnamed" : cleaned;
        }

        // ─── Parsing ──────────────────────────────────────────────────────────────
        private static bool TryInt(Dictionary<string, string> m, string k, out int v)
        {
            v = 0;
            return m.TryGetValue(k, out var s)
                && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);
        }

        private static bool TryFloat(Dictionary<string, string> m, string k, out float v)
        {
            v = 0f;
            return m.TryGetValue(k, out var s)
                && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
        }

        private static bool TryBool(Dictionary<string, string> m, string k, out bool v)
        {
            v = false;
            return m.TryGetValue(k, out var s) && bool.TryParse(s, out v);
        }

        private static bool TryColor(Dictionary<string, string> m, string k, out Color v)
        {
            v = Color.white;
            if (!m.TryGetValue(k, out var s)) return false;
            var p = s.Split(',');
            if (p.Length < 3) return false;
            if (!Num(p[0], out float r) || !Num(p[1], out float g) || !Num(p[2], out float b)) return false;
            float a = 1f;
            if (p.Length > 3) Num(p[3], out a);
            v = new Color(r, g, b, a);
            return true;
        }

        private static bool TryVec2(Dictionary<string, string> m, string k, out Vector2 v)
        {
            v = Vector2.one;
            if (!m.TryGetValue(k, out var s)) return false;
            var p = s.Split(',');
            if (p.Length < 2) return false;
            if (!Num(p[0], out float x) || !Num(p[1], out float y)) return false;
            v = new Vector2(x, y);
            return true;
        }

        private static bool Num(string s, out float v)
            => float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);

        // Builds the key=value body of a preset file.
        private sealed class Writer
        {
            private readonly System.Text.StringBuilder _sb = new System.Text.StringBuilder();

            public Writer() { _sb.AppendLine("# SMT-Trainer preset"); }

            public void Add(string key, string value) => _sb.AppendLine(key + "=" + value);
            public void Add(string key, int value)    => Add(key, value.ToString(CultureInfo.InvariantCulture));
            public void Add(string key, bool value)   => Add(key, value ? "true" : "false");
            public void Add(string key, float value)  => Add(key, F(value));
            public void Add(string key, Color v)      => Add(key, $"{F(v.r)},{F(v.g)},{F(v.b)},{F(v.a)}");
            public void Add(string key, Vector2 v)    => Add(key, $"{F(v.x)},{F(v.y)}");

            private static string F(float v) => v.ToString("0.#####", CultureInfo.InvariantCulture);

            public override string ToString() => _sb.ToString();
        }
    }
}
