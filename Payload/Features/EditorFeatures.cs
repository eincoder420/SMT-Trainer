using System;
using System.Collections.Generic;
using UnityEngine;

namespace SamanthaTrainer.Payload.Features
{
    // Every colour slot the trainer can drive, including ones the game's UI hides.
    public enum ColorSlot
    {
        Hair, Eyes, Lips, Eyeshadow, Fingernails, Pubic, Scalp
    }

    // Character editor: colours, body proportions, hair, and the game's hidden categories.
    //
    // Writing to Game_Data.Character alone is not enough - the game only pushes those values
    // onto materials when it re-runs its own refresh. So every setter here writes the data
    // AND applies the material directly, which makes edits show up immediately.
    public static class EditorFeatures
    {
        public static readonly string[] SlotNames =
        {
            "Hair", "Eyes", "Lips", "Eyeshadow", "Fingernails", "Pubic", "Scalp"
        };

        // ─── Colours ──────────────────────────────────────────────────────────────
        public static Color GetColor(ColorSlot slot)
        {
            var d = GameRefs.Data;
            if (d == null) return Color.white;

            switch (slot)
            {
                case ColorSlot.Hair:        return d.Character.hair_color;
                case ColorSlot.Eyes:        return d.Character.eye_color;
                case ColorSlot.Lips:        return d.Character.lips_color;
                case ColorSlot.Eyeshadow:   return d.Character.eyeshadow_color;
                case ColorSlot.Fingernails: return d.Character.finger_color;
                case ColorSlot.Pubic:       return d.Character.pussy_color;
                case ColorSlot.Scalp:       return d.Character.hair_color;
                default:                    return Color.white;
            }
        }

        public static void SetColor(ColorSlot slot, Color c)
        {
            var d = GameRefs.Data;
            var e = GameRefs.Edit;
            if (d == null) return;

            switch (slot)
            {
                case ColorSlot.Hair:
                case ColorSlot.Scalp:
                    d.Character.hair_color = c;
                    if (e != null)
                    {
                        // Mirrors Edit_Base.Set_Hair_Materials, including the emission it derives.
                        if (e.hair_material != null)
                            for (int i = 0; i < e.hair_material.Length; i++)
                            {
                                if (e.hair_material[i] == null) continue;
                                e.hair_material[i].color = c;
                                e.hair_material[i].SetColor("_Emission", c / 4f);
                                if (e.inventory_hair_material != null && i < e.inventory_hair_material.Length
                                    && e.inventory_hair_material[i] != null)
                                    e.inventory_hair_material[i].color = c;
                            }
                        if (e.scalp_material != null) e.scalp_material.color = c;
                    }
                    break;

                case ColorSlot.Eyes:
                    d.Character.eye_color = c;
                    if (e != null && e.eye_material != null) e.eye_material.color = c;
                    break;

                case ColorSlot.Lips:
                    d.Character.lips_color = c;
                    if (e != null && e.lips_material != null) e.lips_material.color = c;
                    break;

                case ColorSlot.Eyeshadow:
                    d.Character.eyeshadow_color = c;
                    if (e != null && e.eyeshadow_material != null) e.eyeshadow_material.color = c;
                    break;

                case ColorSlot.Fingernails:
                    d.Character.finger_color = c;
                    if (e != null && e.fingernail_material != null) e.fingernail_material.color = c;
                    break;

                case ColorSlot.Pubic:
                    d.Character.pussy_color = c;
                    if (e != null && e.pussy_hair_material != null) e.pussy_hair_material.color = c;
                    break;
            }
        }

        // ─── Body proportions ─────────────────────────────────────────────────────
        // The in-game sliders clamp to 0..1; the underlying fields do not, so the trainer
        // deliberately allows a wider range.
        public static float Boobs
        {
            get => GameRefs.Data?.Character.boobs_size ?? 0f;
            set
            {
                var d = GameRefs.Data; if (d == null) return;
                d.Character.boobs_size = value;
                Safe(() => GameRefs.Edit?.Get_Boobs_Settings());
            }
        }

        public static float Ass
        {
            get => GameRefs.Data?.Character.ass_size ?? 0f;
            set
            {
                var d = GameRefs.Data; if (d == null) return;
                d.Character.ass_size = value;
                Safe(() => GameRefs.Edit?.Get_Ass_Settings());
            }
        }

        public static float Fatness
        {
            get => GameRefs.Data?.Character.fatness ?? 0f;
            set { var d = GameRefs.Data; if (d != null) d.Character.fatness = value; }
        }

        public static float EyeSize
        {
            get => GameRefs.Data?.Character.eye_size ?? 0f;
            set { var d = GameRefs.Data; if (d != null) d.Character.eye_size = value; }
        }

        // ─── Hair / skin / pubic style ────────────────────────────────────────────
        public static int HairstyleCount => GameRefs.Edit?.hairstyles != null
                                          ? GameRefs.Edit.hairstyles.childCount : 0;

        public static int SkinCount => GameRefs.Edit?.skin_materials?.Length ?? 0;

        public static int PubicCount => GameRefs.Edit?.pussy_hairs != null
                                      ? GameRefs.Edit.pussy_hairs.childCount : 0;

        public static int Hairstyle
        {
            get => GameRefs.Data?.Character.hairstyle ?? 0;
            set => Safe(() => GameRefs.Edit?.Set_Hairstyle(value));
        }

        public static int Skincolor
        {
            get => GameRefs.Data?.Character.skincolor ?? 0;
            set => Safe(() => GameRefs.Edit?.Set_Skincolor(value));
        }

        public static int PubicStyle
        {
            get => GameRefs.Data?.Character.pussy_hairs ?? 0;
            set => Safe(() => GameRefs.Edit?.Set_Pussy_Hairs(value));
        }

        public static bool Eyeshadows
        {
            get => GameRefs.Data?.Character.Eyeshadows ?? false;
            set { var d = GameRefs.Data; if (d != null) d.Character.Eyeshadows = value; }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────
        public static IList<string> IndexOptions(int count, string prefix)
        {
            var list = new List<string>();
            for (int i = 0; i < Mathf.Max(count, 1); i++) list.Add(prefix + " " + i);
            if (count <= 0) list[0] = "n/a";
            return list;
        }

        // The game's refresh methods touch UI objects that only exist while the editor is
        // open, so they can throw when called from the trainer. The data write has already
        // happened by then; swallowing here keeps a failed refresh from killing the menu.
        internal static void Safe(Action action)
        {
            try { action(); }
            catch (Exception ex) { Debug.LogWarning("[SMT-Trainer] refresh skipped: " + ex.Message); }
        }
    }
}
