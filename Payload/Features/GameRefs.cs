using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.AzureSky;

namespace SamanthaTrainer.Payload.Features
{
    // Cached handles to the game objects the trainer drives.
    // Re-resolved periodically because scene loads invalidate them.
    public static class GameRefs
    {
        private static float _nextScan;
        private const float RESCAN_INTERVAL = 2f;

        public static Inventory_Script Inventory;
        public static Edit_Base Edit;
        public static AzureTimeController Time_;
        public static AzureSkyController Sky;
        public static vThirdPersonMotor Motor;
        public static Roxanne_Control Player;
        public static Smartphone Phone;

        // The game's central ScriptableObject: character, clothes, items, money.
        public static Game_Data Data => Inventory != null ? Inventory.data
                                      : Edit      != null ? Edit.data
                                      : null;

        public static bool HasCharacter => Data != null && Edit != null;
        public static bool HasWorld     => Time_ != null;
        public static bool HasPlayer    => Motor != null;

        public static void Tick()
        {
            if (UnityEngine.Time.unscaledTime < _nextScan) return;
            _nextScan = UnityEngine.Time.unscaledTime + RESCAN_INTERVAL;
            Rescan();
        }

        // Finds a component even when its GameObject is disabled.
        // Unity 2019.4 has no includeInactive overload on FindObjectOfType, and
        // Resources.FindObjectsOfTypeAll also returns assets, so scene membership is checked.
        public static T FindInactive<T>() where T : Component
        {
            foreach (var candidate in Resources.FindObjectsOfTypeAll<T>())
            {
                if (candidate == null) continue;
                if (candidate.hideFlags != HideFlags.None) continue;
                if (!candidate.gameObject.scene.IsValid()) continue;   // skip prefab assets
                return candidate;
            }
            return null;
        }

        public static T[] FindAllInactive<T>() where T : Component
        {
            var found = new List<T>();
            foreach (var candidate in Resources.FindObjectsOfTypeAll<T>())
            {
                if (candidate == null) continue;
                if (candidate.hideFlags != HideFlags.None) continue;
                if (!candidate.gameObject.scene.IsValid()) continue;
                found.Add(candidate);
            }
            return found.ToArray();
        }

        public static void Rescan()
        {
            if (Inventory == null) Inventory = Object.FindObjectOfType<Inventory_Script>();
            if (Edit      == null) Edit      = Object.FindObjectOfType<Edit_Base>();
            if (Time_     == null) Time_     = Object.FindObjectOfType<AzureTimeController>();
            if (Sky       == null) Sky       = Object.FindObjectOfType<AzureSkyController>();
            if (Motor     == null) Motor     = Object.FindObjectOfType<vThirdPersonMotor>();
            // The phone is disabled while stowed, so a plain FindObjectOfType misses it.
            if (Phone     == null) Phone     = FindInactive<Smartphone>();

            // Inventory_Script already holds the player, so prefer that over another scene sweep.
            if (Player == null) Player = Inventory != null && Inventory.Player != null
                                       ? Inventory.Player
                                       : Object.FindObjectOfType<Roxanne_Control>();
        }
    }
}
