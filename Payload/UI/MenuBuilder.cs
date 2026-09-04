using System;
using System.Collections.Generic;
using SamanthaTrainer.Payload.Features;
using UnityEngine;

namespace SamanthaTrainer.Payload.UI
{
    // Builds the menu tree: Editor (Body / Clothing / Presets), Player, World, Unlock All.
    public static class MenuBuilder
    {
        private static TextPrompt _prompt;

        public static MenuPage Build(TextPrompt prompt)
        {
            _prompt = prompt;

            var root = new MenuPage("SMT TRAINER", "Samantha v2.11");

            root.Add(MenuItem.Sub("Editor", BuildEditor(), "Body, clothing and presets"));
            root.Add(MenuItem.Sub("Player", BuildPlayer(), "Speed, risk and happiness"));
            root.Add(MenuItem.Sub("World",  BuildWorld(),  "Time and weather"));
            root.Add(MenuItem.Sub("Unlock All", BuildUnlock(), "Hairstyles, tools and hidden categories"));
            root.Add(MenuItem.Separator("SETTINGS"));
            root.Add(MenuItem.List("Theme",
                () => MenuThemes.Index,
                i  => MenuThemes.Index = i,
                ThemeNames,
                "Menu colour scheme"));

            return root;
        }

        private static IList<string> ThemeNames()
        {
            var names = new List<string>();
            foreach (var t in MenuThemes.All) names.Add(t.Name);
            return names;
        }

        // ─── Editor ───────────────────────────────────────────────────────────────
        private static MenuPage BuildEditor()
        {
            var page = new MenuPage("EDITOR", "Customisation")
            { PreviewTarget = PreviewTargets.Body, PreviewCaption = "Character", PreviewZoom = 0.55f };

            page.Add(MenuItem.Sub("Body",     BuildBody(),     "Colours, proportions and style"));
            page.Add(MenuItem.Sub("Clothing", BuildClothing(), "Every clothing slot"));
            page.Add(MenuItem.Sub("Presets",  BuildPresets(),  "Save and load body / clothing sets"));

            return page;
        }

        // ─── Editor > Body ────────────────────────────────────────────────────────
        private static MenuPage BuildBody()
        {
            var page = new MenuPage("BODY", "Colours, proportions, style")
            { PreviewTarget = PreviewTargets.Body, PreviewCaption = "Body", PreviewZoom = 0.6f };

            page.Add(MenuItem.Separator("COLOURS"));
            foreach (ColorSlot slot in Enum.GetValues(typeof(ColorSlot)))
            {
                var captured = slot;
                string name = EditorFeatures.SlotNames[(int)captured];
                page.Add(MenuItem.Sub(name,
                    BuildColorPage(name,
                        () => EditorFeatures.GetColor(captured),
                        c  => EditorFeatures.SetColor(captured, c),
                        () => PreviewTargets.ForColorSlot(captured),
                        ZoomFor(captured)),
                    "Unrestricted colour, including values above 1.0"));
            }

            page.Add(MenuItem.Separator("STYLE"));
            page.Add(MenuItem.List("Hairstyle",
                () => EditorFeatures.Hairstyle,
                i  => EditorFeatures.Hairstyle = i,
                () => EditorFeatures.IndexOptions(EditorFeatures.HairstyleCount, "Style"),
                "All styles selectable, bought or not"));
            page.Add(MenuItem.List("Skin Colour",
                () => EditorFeatures.Skincolor,
                i  => EditorFeatures.Skincolor = i,
                () => EditorFeatures.IndexOptions(EditorFeatures.SkinCount, "Skin")));
            page.Add(MenuItem.List("Pubic Style",
                () => EditorFeatures.PubicStyle,
                i  => EditorFeatures.PubicStyle = i,
                () => EditorFeatures.IndexOptions(EditorFeatures.PubicCount, "Style")));
            page.Add(MenuItem.Toggle("Eyeshadows",
                () => EditorFeatures.Eyeshadows,
                v  => EditorFeatures.Eyeshadows = v));

            page.Add(MenuItem.Separator("PROPORTIONS  (unclamped)"));
            page.Add(MenuItem.Slider("Boobs Size",
                () => EditorFeatures.Boobs, v => EditorFeatures.Boobs = v, -1f, 3f, 0.05f));
            page.Add(MenuItem.Slider("Ass Size",
                () => EditorFeatures.Ass, v => EditorFeatures.Ass = v, -1f, 3f, 0.05f));
            page.Add(MenuItem.Slider("Fatness",
                () => EditorFeatures.Fatness, v => EditorFeatures.Fatness = v, -1f, 3f, 0.05f));
            page.Add(MenuItem.Slider("Eye Size",
                () => EditorFeatures.EyeSize, v => EditorFeatures.EyeSize = v, -1f, 3f, 0.05f));

            return page;
        }

        // Fingernails have no isolated mesh so they fall back to the whole body, which needs
        // pulling in. The facial slots already frame tightly on their own.
        private static float ZoomFor(ColorSlot slot)
            => slot == ColorSlot.Fingernails ? 0.55f : 1f;

        // ─── Editor > Clothing ────────────────────────────────────────────────────
        private static MenuPage BuildClothing()
        {
            var page = new MenuPage("CLOTHING", "All slots")
            { PreviewTarget = PreviewTargets.Body, PreviewCaption = "Outfit", PreviewZoom = 0.7f };

            // Built on open: the clothing array only exists once a save is loaded.
            page.OnOpen = p =>
            {
                p.Items.Clear();
                p.Selected = 0;
                p.Scroll = 0;

                int count = ClothingFeatures.Count;
                if (count == 0)
                {
                    p.Add(MenuItem.Separator("Load a save first"));
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    int index = i;
                    p.Add(MenuItem.Sub($"[{index}]  {ClothingFeatures.Name(index)}",
                                       BuildClothingSlot(index),
                                       "Worn state, variant, colour and tiling"));
                }

                p.Add(MenuItem.Separator("ALL SLOTS"));
                p.Add(MenuItem.Action_("Wear Everything", () =>
                {
                    for (int i = 0; i < ClothingFeatures.Count; i++) ClothingFeatures.SetWorn(i, true);
                }, "", "Everything worn"));
                p.Add(MenuItem.Action_("Remove Everything", () =>
                {
                    for (int i = 0; i < ClothingFeatures.Count; i++) ClothingFeatures.SetWorn(i, false);
                }, "", "Everything removed"));
            };

            return page;
        }

        private static MenuPage BuildClothingSlot(int index)
        {
            var page = new MenuPage("CLOTHING", "Slot " + index)
            {
                PreviewTarget = () => PreviewTargets.Cloth(index),
                // An unworn garment's renderers are disabled, so there is nothing to frame.
                PreviewEmptyMessage = () => ClothingFeatures.GetWorn(index)
                                          ? "nothing to preview"
                                          : "enable Worn to see a preview"
            };

            // Rebuilt on open so the title and variant count reflect the loaded save.
            page.OnOpen = p =>
            {
                p.Subtitle = ClothingFeatures.Name(index);
                p.PreviewCaption = ClothingFeatures.Name(index);
                if (p.Items.Count > 0) return;

                p.Add(MenuItem.Toggle("Worn",
                    () => ClothingFeatures.GetWorn(index),
                    v  => ClothingFeatures.SetWorn(index, v)));

                p.Add(MenuItem.List("Variant",
                    () => ClothingFeatures.GetVariant(index),
                    i  => ClothingFeatures.SetVariant(index, i),
                    () => EditorFeatures.IndexOptions(ClothingFeatures.VariantCount(index), "Variant"),
                    "Switch the mesh used for this slot"));

                p.Add(MenuItem.Sub("Colour",
                    BuildColorPage(ClothingFeatures.Name(index),
                        () => ClothingFeatures.GetColor(index),
                        c  => ClothingFeatures.SetColor(index, c),
                        () => PreviewTargets.Cloth(index),
                        1f,
                        () => ClothingFeatures.GetWorn(index)
                            ? "nothing to preview"
                            : "enable Worn to see a preview")));

                p.Add(MenuItem.Separator("TILING"));
                p.Add(MenuItem.Slider("Tiling X",
                    () => ClothingFeatures.GetTiling(index).x,
                    v  => { var t = ClothingFeatures.GetTiling(index); t.x = v; ClothingFeatures.SetTiling(index, t); },
                    0.1f, 16f, 0.1f, "0.0"));
                p.Add(MenuItem.Slider("Tiling Y",
                    () => ClothingFeatures.GetTiling(index).y,
                    v  => { var t = ClothingFeatures.GetTiling(index); t.y = v; ClothingFeatures.SetTiling(index, t); },
                    0.1f, 16f, 0.1f, "0.0"));
                p.Add(MenuItem.Action_("Reset Tiling",
                    () => ClothingFeatures.SetTiling(index, Vector2.one), "", "Tiling reset"));
            };

            return page;
        }

        // ─── Editor > Presets ─────────────────────────────────────────────────────
        private static MenuPage BuildPresets()
        {
            var page = new MenuPage("PRESETS", "Documents\\SMT-Trainer");

            page.Add(MenuItem.Separator("BODY"));
            page.Add(MenuItem.Action_("Save Body Preset...", () =>
                PromptForName("Body preset name", name =>
                {
                    if (PresetStore.SaveBody(name)) Toast.Show("Body preset saved: " + name);
                    else _prompt.ShowError("Nothing to save - load a game first.");
                }), "Saves colours, proportions and style"));
            page.Add(MenuItem.Sub("Load Body Preset",
                BuildPresetList(PresetKind.Body), "Apply a saved body"));

            page.Add(MenuItem.Separator("CLOTHING"));
            page.Add(MenuItem.Action_("Save Clothing Preset...", () =>
                PromptForName("Clothing preset name", name =>
                {
                    if (PresetStore.SaveClothing(name)) Toast.Show("Clothing preset saved: " + name);
                    else _prompt.ShowError("Nothing to save - load a game first.");
                }), "Saves every slot's state, colour and tiling"));
            page.Add(MenuItem.Sub("Load Clothing Preset",
                BuildPresetList(PresetKind.Clothing), "Apply a saved outfit"));

            page.Add(MenuItem.Separator("FOLDER"));
            page.Add(MenuItem.Action_("Open Presets Folder", OpenPresetFolder,
                PresetStore.RootFolder, "Opened presets folder"));

            return page;
        }

        private static void PromptForName(string title, Action<string> onName)
        {
            _prompt.Open(title, "", text =>
            {
                var name = (text ?? "").Trim();
                if (name.Length == 0) { _prompt.ShowError("Name cannot be empty."); return; }
                onName(name);
            });
        }

        private static MenuPage BuildPresetList(PresetKind kind)
        {
            var page = new MenuPage(kind == PresetKind.Body ? "BODY PRESETS" : "CLOTHING PRESETS",
                                    "Saved sets")
            { PreviewTarget = PreviewTargets.Body, PreviewCaption = "Character", PreviewZoom = 0.55f };

            // Rebuilt on open so presets saved this session show up without a restart.
            page.OnOpen = p =>
            {
                p.Items.Clear();
                p.Selected = 0;
                p.Scroll = 0;

                var names = PresetStore.List(kind);
                if (names.Count == 0)
                {
                    p.Add(MenuItem.Separator("No presets saved yet"));
                    return;
                }

                foreach (var n in names)
                {
                    string name = n;
                    p.Add(MenuItem.Action_(name, () =>
                    {
                        bool ok = kind == PresetKind.Body
                                ? PresetStore.LoadBody(name)
                                : PresetStore.LoadClothing(name);
                        if (ok) Toast.Show("Applied: " + name);
                        else Toast.Error("Could not apply " + name);
                    }, "Enter to apply"));
                }

                p.Add(MenuItem.Separator("MANAGE"));
                foreach (var n in names)
                {
                    string name = n;
                    p.Add(MenuItem.Action_("Delete  " + name,
                        () =>
                        {
                            if (PresetStore.Delete(kind, name)) Toast.Show("Deleted: " + name);
                            else Toast.Error("Could not delete " + name);
                        },
                        "Reopen this page to refresh the list"));
                }
            };

            return page;
        }

        private static void OpenPresetFolder()
        {
            EditorFeatures.Safe(() =>
            {
                var dir = PresetStore.RootFolder;
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                Application.OpenURL("file:///" + dir.Replace('\\', '/'));
            });
        }

        // RGBA editor for one colour. Channels go past 1.0 on purpose - the game's own HSV
        // wheel cannot express that, and hair feeds its colour into an emission term.
        private static MenuPage BuildColorPage(string name, Func<Color> get, Action<Color> set,
                                               Func<Transform> preview = null, float zoom = 1f,
                                               Func<string> emptyMessage = null)
        {
            var page = new MenuPage(name.ToUpperInvariant(), "RGBA / hex")
            {
                PreviewTarget = preview ?? PreviewTargets.Body,
                PreviewCaption = name,
                PreviewZoom = zoom,
                PreviewEmptyMessage = emptyMessage
            };

            page.Add(MenuItem.Slider("Red",   () => get().r, v => { var c = get(); c.r = v; set(c); }, 0f, 4f, 0.02f, "0.00"));
            page.Add(MenuItem.Slider("Green", () => get().g, v => { var c = get(); c.g = v; set(c); }, 0f, 4f, 0.02f, "0.00"));
            page.Add(MenuItem.Slider("Blue",  () => get().b, v => { var c = get(); c.b = v; set(c); }, 0f, 4f, 0.02f, "0.00"));
            page.Add(MenuItem.Slider("Alpha", () => get().a, v => { var c = get(); c.a = v; set(c); }, 0f, 1f, 0.02f, "0.00"));

            page.Add(MenuItem.Separator("HEX"));
            page.Add(MenuItem.Action_("Enter Hex...", () =>
            {
                _prompt.Open("Hex colour  (RRGGBB or RRGGBBAA)", ColorPresets.ToHex(get()), text =>
                {
                    if (ColorPresets.TryParseHex(text, out var parsed)) set(parsed);
                    else _prompt.ShowError("Not a valid hex colour.");
                });
            }, "Type an exact hex value"));

            page.Add(MenuItem.Separator("PALETTE"));
            page.Add(MenuItem.Action_("Save To Palette...", () =>
                PromptForName("Palette entry name", n =>
                {
                    ColorPresets.Add(n, get());
                    Toast.Show("Saved to palette: " + n);
                }),
                "Single colours, reusable anywhere"));
            page.Add(MenuItem.Sub("Apply From Palette", BuildPalettePage(set, page.PreviewTarget, name, page.PreviewZoom)));

            return page;
        }

        private static MenuPage BuildPalettePage(Action<Color> set, Func<Transform> preview,
                                                 string caption, float zoom)
        {
            var page = new MenuPage("PALETTE", "Saved colours")
            { PreviewTarget = preview ?? PreviewTargets.Body, PreviewCaption = caption, PreviewZoom = zoom };

            page.OnOpen = p =>
            {
                p.Items.Clear();
                p.Selected = 0;
                p.Scroll = 0;

                if (ColorPresets.Count == 0)
                {
                    p.Add(MenuItem.Separator("No colours saved yet"));
                    return;
                }

                for (int i = 0; i < ColorPresets.Count; i++)
                {
                    int index = i;
                    var entry = ColorPresets.All[index];
                    p.Add(MenuItem.Action_(entry.Name, () =>
                    {
                        if (ColorPresets.TryGet(index, out var c)) set(c);
                    }, ColorPresets.ToHex(entry.Color)));
                }

                p.Add(MenuItem.Separator("MANAGE"));
                p.Add(MenuItem.Action_("Delete Last Entry",
                    () => ColorPresets.RemoveAt(ColorPresets.Count - 1), "", "Palette entry deleted"));
                p.Add(MenuItem.Action_("Reload From Disk", ColorPresets.Load, ColorPresets.FilePath,
                    "Palette reloaded"));
            };

            return page;
        }

        // ─── Player ───────────────────────────────────────────────────────────────
        private static MenuPage BuildPlayer()
        {
            var page = new MenuPage("PLAYER", "Movement and stats");
            page.OnOpen = p => p.Subtitle = StuckFix.StatusText;

            page.Add(MenuItem.Separator("MOVEMENT"));
            page.Add(MenuItem.Toggle("Speed Modifier",
                () => PlayerFeatures.Enabled,
                v  => PlayerFeatures.Enabled = v,
                "Scales walk, run and sprint"));
            page.Add(MenuItem.Slider("Speed Multiplier",
                () => PlayerFeatures.SpeedMultiplier,
                v  => PlayerFeatures.SpeedMultiplier = v,
                0.1f, 25f, 0.1f, "0.0"));
            page.Add(MenuItem.Action_("Reset Speeds", PlayerFeatures.Reset,
                "Restore the game's original values", "Speeds reset"));

            page.Add(MenuItem.Separator("STATS"));
            page.Add(MenuItem.Toggle("0 Risk Level",
                () => PlayerFeatures.ZeroRisk,
                v  => PlayerFeatures.ZeroRisk = v,
                "Pins nudity level and watchers to zero"));
            page.Add(MenuItem.Toggle("0 Embarrassment",
                () => PlayerFeatures.ZeroEmbarrassment,
                v  => PlayerFeatures.ZeroEmbarrassment = v,
                "Pins shame at zero, so it never maxes out"));
            page.Add(MenuItem.Toggle("Maximal Happiness",
                () => PlayerFeatures.MaxHappiness,
                v  => PlayerFeatures.MaxHappiness = v,
                "Holds happiness at 100"));

            page.Add(MenuItem.Separator("FIXES"));
            page.Add(MenuItem.Action_("Fix Stuck Player", () => Toast.Show(StuckFix.Apply()),
                "Frees the character after a frozen wardrobe animation  (F9)"));
            page.Add(MenuItem.Toggle("Auto-Fix Stuck",
                () => StuckFix.AutoFix,
                v  => StuckFix.AutoFix = v,
                "Clears the freeze on its own after " + StuckFix.StuckThresholdSeconds + "s"));

            return page;
        }

        // ─── World ────────────────────────────────────────────────────────────────
        private static MenuPage BuildWorld()
        {
            var page = new MenuPage("WORLD", "Time and weather");

            page.Add(MenuItem.Separator("TIME"));
            page.Add(MenuItem.Slider("Time Of Day",
                () => WorldFeatures.Timeline,
                v  => WorldFeatures.Timeline = v,
                0f, 24f, 0.25f, "0.00"));
            page.Add(MenuItem.Toggle("Freeze Time",
                () => WorldFeatures.FreezeTime,
                v  => WorldFeatures.FreezeTime = v,
                "Stops the day/night cycle"));
            page.Add(MenuItem.Slider("Day Length (hours)",
                () => WorldFeatures.DayLength,
                v  => WorldFeatures.DayLength = v,
                0.5f, 96f, 0.5f, "0.0",
                "Lower means time passes faster"));

            page.Add(MenuItem.Separator("WEATHER"));
            page.Add(MenuItem.List("Weather",
                () => WorldFeatures.WeatherIndex,
                i  => WorldFeatures.WeatherIndex = i,
                WorldFeatures.WeatherNames,
                "The game's own weather profiles"));
            page.Add(MenuItem.Action_("Apply Instantly", WorldFeatures.ApplyWeatherInstantly,
                "Skip the blend", "Weather applied"));
            page.Add(MenuItem.Action_("Reset To Default", WorldFeatures.ResetWeather, "", "Weather reset"));

            return page;
        }

        // ─── Unlock ───────────────────────────────────────────────────────────────
        private static MenuPage BuildUnlock()
        {
            var page = new MenuPage("UNLOCK ALL", "Progression gates");

            page.Add(MenuItem.Action_("Unlock Everything", UnlockFeatures.UnlockEverything,
                "Hairstyles, tools, items and categories", "Everything unlocked"));
            page.Add(MenuItem.Separator("INDIVIDUAL"));
            page.Add(MenuItem.Action_("Unlock All Clothing", UnlockFeatures.UnlockAllClothing,
                "Marks every clothing variant as bought", "All clothing unlocked"));
            page.Add(MenuItem.Toggle("Dev Mode (Test Game)",
                () => UnlockFeatures.DevMode,
                v  => UnlockFeatures.DevMode = v,
                "The game's own flag - wear anything, bought or not"));
            page.Add(MenuItem.Action_("Unlock All Hairstyles", UnlockFeatures.UnlockAllHairstyles,
                "", "All hairstyles unlocked"));
            page.Add(MenuItem.Action_("Unlock Hidden Categories", UnlockFeatures.UnlockAllCategories,
                "Clears the editor's blocked parameters", "Hidden categories unlocked"));
            page.Add(MenuItem.Action_("Refill Editor Tools", UnlockFeatures.RefillTools,
                "The x N counters in the editor", "Editor tools refilled"));
            page.Add(MenuItem.Action_("Refill Items", UnlockFeatures.RefillItems, "", "Items refilled"));

            page.Add(MenuItem.Separator("MONEY"));
            page.Add(MenuItem.Action_("Add $1,000",   () => UnlockFeatures.AddMoney(1000),   "Cash and bank both", "+$1,000"));
            page.Add(MenuItem.Action_("Add $10,000",  () => UnlockFeatures.AddMoney(10000),  "Cash and bank both", "+$10,000"));
            page.Add(MenuItem.Action_("Add $100,000", () => UnlockFeatures.AddMoney(100000), "Cash and bank both", "+$100,000"));

            return page;
        }
    }
}
