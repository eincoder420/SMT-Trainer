using UnityEngine;

namespace SamanthaTrainer.Payload.Features
{
    // Full control over every clothing slot: worn state, variant, colour and tiling.
    //
    // The game's own editor only exposes a handful of these, and only for a few slots.
    // State changes are pushed through Inventory_Script.Wear_Cloth_On_Model, which is the
    // game's own mesh refresh - going through Wear() instead would route into the dressing
    // animation and silently do nothing while the player is busy.
    public static class ClothingFeatures
    {
        public static int Count => GameRefs.Data?.Clothes?.Length ?? 0;

        public static bool Valid(int index)
        {
            var d = GameRefs.Data;
            return d?.Clothes != null && index >= 0 && index < d.Clothes.Length;
        }

        public static string Name(int index)
        {
            if (!Valid(index)) return "?";
            string n = GameRefs.Data.Clothes[index].Name;
            return string.IsNullOrEmpty(n) ? "Slot " + index : n;
        }

        // Number of mesh variants the slot has, read from the live inventory.
        public static int VariantCount(int index)
        {
            var inv = GameRefs.Inventory;
            if (inv?.Clothes == null || index < 0 || index >= inv.Clothes.Length) return 0;
            return inv.Clothes[index].Mesh?.Length ?? 0;
        }

        // ─── Worn ─────────────────────────────────────────────────────────────────
        public static bool GetWorn(int index)
            => Valid(index) && GameRefs.Data.Clothes[index].Weared;

        public static void SetWorn(int index, bool worn)
        {
            if (!Valid(index)) return;
            GameRefs.Data.Clothes[index].Weared = worn;
            Refresh(index);
        }

        // ─── Variant ──────────────────────────────────────────────────────────────
        public static int GetVariant(int index)
            => Valid(index) ? GameRefs.Data.Clothes[index].Current_Variant : 0;

        public static void SetVariant(int index, int variant)
        {
            if (!Valid(index)) return;

            int count = VariantCount(index);
            if (count > 0) variant = Mathf.Clamp(variant, 0, count - 1);

            GameRefs.Data.Clothes[index].Current_Variant = variant;

            // Keep the inventory's own selection in step, or the game will snap it back.
            var inv = GameRefs.Inventory;
            if (inv?.Clothes != null && index < inv.Clothes.Length)
                inv.Clothes[index].Chosen_Variant = variant;

            Refresh(index);
        }

        // ─── Colour ───────────────────────────────────────────────────────────────
        public static Color GetColor(int index)
            => Valid(index) ? GameRefs.Data.Clothes[index].main_color : Color.white;

        public static void SetColor(int index, Color color)
        {
            if (!Valid(index)) return;
            GameRefs.Data.Clothes[index].main_color = color;
            ApplyMaterials(index);
        }

        // ─── Tiling ───────────────────────────────────────────────────────────────
        public static Vector2 GetTiling(int index)
            => Valid(index) ? GameRefs.Data.Clothes[index].Tiling : Vector2.one;

        public static void SetTiling(int index, Vector2 tiling)
        {
            if (!Valid(index)) return;
            GameRefs.Data.Clothes[index].Tiling = tiling;
            ApplyMaterials(index);
        }

        // ─── Application ──────────────────────────────────────────────────────────
        private static void Refresh(int index)
        {
            var inv = GameRefs.Inventory;
            if (inv == null) return;
            EditorFeatures.Safe(() => inv.Wear_Cloth_On_Model(index));
        }

        // Push colour and tiling onto every renderer belonging to the slot.
        private static void ApplyMaterials(int index)
        {
            var inv = GameRefs.Inventory;
            if (inv?.Clothes == null || index < 0 || index >= inv.Clothes.Length) return;
            if (!Valid(index)) return;

            Color color = GameRefs.Data.Clothes[index].main_color;
            Vector2 tiling = GameRefs.Data.Clothes[index].Tiling;
            if (tiling == Vector2.zero) tiling = Vector2.one;

            Apply(inv.Clothes[index].Mesh, color, tiling);
            Apply(inv.Clothes[index].Inv_Mesh, color, tiling);
        }

        private static void Apply(Transform[] meshes, Color color, Vector2 tiling)
        {
            if (meshes == null) return;

            foreach (var mesh in meshes)
            {
                if (mesh == null) continue;
                foreach (var r in mesh.GetComponentsInChildren<Renderer>(true))
                {
                    if (r == null) continue;
                    foreach (var m in r.materials)
                    {
                        if (m == null) continue;
                        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
                        else m.color = color;
                        m.mainTextureScale = tiling;
                    }
                }
            }
        }

        // Reapply everything, used after loading a clothing preset.
        public static void ApplyAll()
        {
            for (int i = 0; i < Count; i++)
            {
                Refresh(i);
                ApplyMaterials(i);
            }
        }
    }
}
