using UnityEngine;

namespace SamanthaTrainer.Payload.Features
{
    // Resolves the specific scene object each editor page should preview.
    //
    // These return the live objects on the character, so whatever the preview camera frames
    // is the real mesh with the real material - a colour change is visible immediately with
    // no extra plumbing.
    public static class PreviewTargets
    {
        // The hairstyle currently selected, from Edit_Base's hairstyle folder.
        public static Transform Hair()
        {
            var edit = GameRefs.Edit;
            if (edit?.hairstyles == null) return Head();

            // Get_Hairstyle_Settings enables only the child matching Character.hairstyle,
            // so the active child is the style actually being worn.
            int index = GameRefs.Data?.Character.hairstyle ?? 0;
            if (index >= 0 && index < edit.hairstyles.childCount)
            {
                var child = edit.hairstyles.GetChild(index);
                if (child != null && child.gameObject.activeInHierarchy) return child;
            }

            foreach (Transform child in edit.hairstyles)
                if (child.gameObject.activeInHierarchy) return child;

            return Head();
        }

        public static Transform Pubic()
        {
            var edit = GameRefs.Edit;
            if (edit?.pussy_hairs == null) return Body();

            int index = GameRefs.Data?.Character.pussy_hairs ?? 0;
            if (index >= 0 && index < edit.pussy_hairs.childCount)
            {
                var child = edit.pussy_hairs.GetChild(index);
                if (child != null && child.gameObject.activeInHierarchy) return child;
            }
            return Body();
        }

        public static Transform Head()
        {
            var edit = GameRefs.Edit;
            if (edit?.head != null) return edit.head.transform;
            return Body();
        }

        public static Transform Eyes()
        {
            var edit = GameRefs.Edit;
            if (edit?.eyes != null) return edit.eyes.transform;
            return Head();
        }

        public static Transform Body()
        {
            var edit = GameRefs.Edit;
            if (edit?.body != null) return edit.body.transform;

            var inv = GameRefs.Inventory;
            if (inv?.Sam_Parent != null) return inv.Sam_Parent;

            return GameRefs.Player != null ? GameRefs.Player.transform : null;
        }

        // The mesh for a clothing slot's currently selected variant.
        public static Transform Cloth(int slot)
        {
            var inv = GameRefs.Inventory;
            if (inv?.Clothes == null || slot < 0 || slot >= inv.Clothes.Length) return Body();

            var meshes = inv.Clothes[slot].Mesh;
            if (meshes == null || meshes.Length == 0) return Body();

            int variant = ClothingFeatures.GetVariant(slot);
            if (variant >= 0 && variant < meshes.Length && meshes[variant] != null)
                return meshes[variant];

            return Body();
        }

        // Which object best shows off a given colour slot.
        public static Transform ForColorSlot(ColorSlot slot)
        {
            switch (slot)
            {
                case ColorSlot.Hair:
                case ColorSlot.Scalp:
                    return Hair();
                case ColorSlot.Eyes:
                    return Eyes();
                case ColorSlot.Lips:
                case ColorSlot.Eyeshadow:
                    return Head();
                case ColorSlot.Pubic:
                    return Pubic();
                default:
                    // Fingernails live on the body mesh and cannot be isolated.
                    return Body();
            }
        }
    }
}
