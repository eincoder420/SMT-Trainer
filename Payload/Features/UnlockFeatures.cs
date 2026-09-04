using UnityEngine;

namespace SamanthaTrainer.Payload.Features
{
    // The game's own gating switches: bought haircuts, consumable editor tools, and the
    // editor categories it keeps hidden until progress unlocks them.
    public static class UnlockFeatures
    {
        public const int RefillAmount = 999;

        public static string Status
        {
            get
            {
                var d = GameRefs.Data;
                if (d == null) return "no save loaded";
                int cuts = d.items.Haircuts_Bought?.Length ?? 0;
                int tools = d.items.Remain_Tools?.Length ?? 0;
                int blocked = d.Blocked_Param?.Length ?? 0;
                return $"{cuts} haircuts, {tools} tools, {blocked} params";
            }
        }

        // Edit_Base.Check_Edit_Haircuts gates each hairstyle button on
        // Haircuts_Bought[i] && Remain_Tools[6] > 0, so both have to be satisfied.
        public static void UnlockAllHairstyles()
        {
            var d = GameRefs.Data;
            if (d?.items.Haircuts_Bought == null) return;

            for (int i = 0; i < d.items.Haircuts_Bought.Length; i++)
                d.items.Haircuts_Bought[i] = true;

            RefillTools();
            EditorFeatures.Safe(() => GameRefs.Edit?.Check_Edit_Haircuts());
        }

        // Tops up the consumables shown as "x N" next to the editor rows.
        public static void RefillTools()
        {
            var d = GameRefs.Data;
            if (d?.items.Remain_Tools == null) return;

            for (int i = 0; i < d.items.Remain_Tools.Length; i++)
                d.items.Remain_Tools[i] = RefillAmount;
        }

        public static void RefillItems()
        {
            var d = GameRefs.Data;
            if (d?.items.Remain_Items == null) return;

            for (int i = 0; i < d.items.Remain_Items.Length; i++)
                d.items.Remain_Items[i] = RefillAmount;
        }

        // Clears Game_Data.Blocked_Param, which is what hides editor categories.
        // Routed through the game's own Subparam_Window.Unblock_Quiet so its windows
        // stay in sync rather than just flipping the flags behind its back.
        public static void UnlockAllCategories()
        {
            var d = GameRefs.Data;
            if (d?.Blocked_Param == null) return;

            var windows = Object.FindObjectsOfType<Subparam_Window>();
            for (int i = 0; i < d.Blocked_Param.Length; i++)
            {
                int id = i;
                bool routed = false;

                foreach (var w in windows)
                {
                    if (w == null || w.Sub_parameter == null || id >= w.Sub_parameter.Length) continue;
                    EditorFeatures.Safe(() => w.Unblock_Quiet(id));
                    routed = true;
                    break;
                }

                if (!routed) d.Blocked_Param[id] = false;
            }

            foreach (var w in windows)
            {
                if (w == null || !w.params_open) continue;
                // Reopen so newly unblocked rows become visible straight away.
                EditorFeatures.Safe(() => { w.Switch_Subparam(); w.Switch_Subparam(); });
            }
        }

        // ─── Clothing ─────────────────────────────────────────────────────────────
        // Marks every clothing variant as bought.
        //
        // Spawned_Cloth[v].Bought is the real gate. Wardrobe_Button caches it into its own
        // field during Start, so the live buttons have to be updated too or already-spawned
        // wardrobe entries stay greyed out until the scene reloads. Spawned is cleared as
        // well, since a variant lying on the floor is also treated as unavailable.
        public static void UnlockAllClothing()
        {
            var d = GameRefs.Data;
            if (d?.Clothes == null) return;

            for (int id = 0; id < d.Clothes.Length; id++)
            {
                var variants = d.Clothes[id].Spawned_Cloth;
                if (variants == null) continue;

                for (int v = 0; v < variants.Length; v++)
                {
                    variants[v].Bought = true;
                    variants[v].Spawned = false;
                }
            }

            RefreshClothingButtons();
            RefreshShop();
        }

        private static void RefreshClothingButtons()
        {
            var d = GameRefs.Data;
            if (d?.Clothes == null) return;

            foreach (var button in GameRefs.FindAllInactive<Wardrobe_Button>())
            {
                if (button == null) continue;
                if (button.id < 0 || button.id >= d.Clothes.Length) continue;

                var variants = d.Clothes[button.id].Spawned_Cloth;
                if (variants == null || button.Variant < 0 || button.Variant >= variants.Length) continue;

                button.Bought = true;
                EditorFeatures.Safe(button.Reactivate_Button);
            }
        }

        // The phone's shop hides entries that are already bought.
        private static void RefreshShop()
        {
            var phone = GameRefs.Phone;
            if (phone == null) return;
            EditorFeatures.Safe(phone.Check_Shop_Objects);
        }

        // The game's own developer flag. Wardrobe_Button.Wear_Chosen_Cloth_Variant forces
        // Bought to true whenever it is set, so it acts as a blanket override for anything
        // UnlockAllClothing might not have reached.
        public static bool DevMode
        {
            get => GameRefs.Data?.Test_Game ?? false;
            set
            {
                var d = GameRefs.Data;
                if (d == null) return;
                d.Test_Game = value;
                if (value) RefreshClothingButtons();
            }
        }

        public static void UnlockEverything()
        {
            UnlockAllHairstyles();
            UnlockAllClothing();
            RefillTools();
            RefillItems();
            UnlockAllCategories();
        }

        // ─── Money ────────────────────────────────────────────────────────────────
        public static int Money
        {
            get => GameRefs.Data?.money.Remain_Money ?? 0;
            set { var d = GameRefs.Data; if (d != null) d.money.Remain_Money = value; }
        }

        public static int BankBalance
        {
            get => GameRefs.Data?.money.Remain_Atm_Balance ?? 0;
            set { var d = GameRefs.Data; if (d != null) d.money.Remain_Atm_Balance = value; }
        }

        // Adds to cash and bank balance both.
        //
        // These are two separate wallets: shops on the phone spend Remain_Atm_Balance while
        // vending machines and the restaurant spend Remain_Money. Topping up only one leaves
        // the other empty, which looks like the cheat did nothing.
        // The phone's money labels are only rewritten when it recounts, so refresh it too.
        public static void AddMoney(int amount)
        {
            var d = GameRefs.Data;
            if (d == null) return;

            d.money.Remain_Money += amount;
            d.money.Remain_Atm_Balance += amount;
            RefreshMoneyUI();
        }

        public static void RefreshMoneyUI()
        {
            var phone = GameRefs.Phone;
            if (phone == null) return;
            EditorFeatures.Safe(phone.Recount_Money);
            EditorFeatures.Safe(phone.Check_Shop_Objects);
        }
    }
}
