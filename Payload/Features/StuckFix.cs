using System;
using System.Reflection;
using UnityEngine;

namespace SamanthaTrainer.Payload.Features
{
    // Recovery for the wardrobe freeze, where the player is left unable to move after a
    // dressing animation is interrupted.
    //
    // Dressing and Up_Dress are StateMachineBehaviours: they raise inventory.Wearing and
    // player.Anim_UpDress_Process when their animator state begins and lower them on state
    // exit. If that state never exits - which is what happens when the wardrobe is closed
    // mid-animation - the flags stay true forever. Roxanne_Control gates interaction and
    // movement on !inventory.Wearing, and Wear() refuses to run while either flag is set,
    // so the character is stuck with no way back.
    //
    // The fix lowers the flags by hand and clears the animator state that goes with them.
    public static class StuckFix
    {
        // Dressing animations run a few seconds at most, so anything still "in progress"
        // well past that is a stuck flag rather than a long animation.
        public const float StuckThresholdSeconds = 6f;

        public static bool AutoFix = true;

        private static float _busySince = -1f;
        private static int _fixCount;

        public static int FixCount => _fixCount;

        // True while the game believes a dressing animation is running.
        public static bool IsBusy
        {
            get
            {
                var inv = GameRefs.Inventory;
                var player = GameRefs.Player;
                if (inv == null || player == null) return false;
                return inv.Wearing || player.Anim_UpDress_Process;
            }
        }

        public static float BusySeconds
            => _busySince < 0f ? 0f : Time.unscaledTime - _busySince;

        public static void Tick()
        {
            if (!IsBusy)
            {
                _busySince = -1f;
                return;
            }

            if (_busySince < 0f) _busySince = Time.unscaledTime;

            if (AutoFix && BusySeconds > StuckThresholdSeconds)
            {
                Apply();
                _busySince = -1f;
            }
        }

        // Clears the stuck state. Safe to run even when nothing is wrong.
        public static string Apply()
        {
            var inv = GameRefs.Inventory;
            var player = GameRefs.Player;

            if (inv == null || player == null) return "player not found";

            EditorFeatures.Safe(() =>
            {
                // The two flags that actually block movement and further dressing.
                inv.Wearing = false;
                player.Anim_UpDress_Process = false;

                // Pending-wear bookkeeping, or the next wear request is swallowed.
                inv.Waiting_For_Wear = false;
                inv.Wardrobe_Wear = false;

                player.Showing = false;
                if (player.Covered) player.Stop_Covering();
            });

            ClearAnimator(player.anim);
            RestoreConstraints();
            ClearMotorLocks();

            EditorFeatures.Safe(() => inv.Turn_Cloth_Items(true));

            _busySince = -1f;
            _fixCount++;
            Debug.Log("[SMT-Trainer] wardrobe unstick applied");
            return "unstuck";
        }

        // Drops the triggers and bools the dressing states leave behind. Parameters are
        // checked first so a name this build does not have cannot spam the log.
        private static void ClearAnimator(Animator anim)
        {
            if (anim == null || anim.runtimeAnimatorController == null) return;

            EditorFeatures.Safe(() =>
            {
                foreach (var p in anim.parameters)
                {
                    switch (p.type)
                    {
                        case AnimatorControllerParameterType.Trigger:
                            if (p.name == "Dress" || p.name == "Undress" || p.name == "No" ||
                                p.name == "Accept_Requied_Unwear" || p.name == "Tip_Cloth")
                                anim.ResetTrigger(p.nameHash);
                            break;

                        case AnimatorControllerParameterType.Bool:
                            if (p.name == "Upped_Dress" || p.name == "Excited" || p.name == "Have_Toy")
                                anim.SetBool(p.nameHash, false);
                            else if (p.name == "ReadyWalkUpdressed")
                                anim.SetBool(p.nameHash, true);
                            break;
                    }
                }
            });
        }

        // Dressing switches off slot 3's parent constraints halfway through its animation.
        // An interrupted animation never switches them back, which leaves that garment
        // detached from the body.
        private static void RestoreConstraints()
        {
            var inv = GameRefs.Inventory;
            var data = GameRefs.Data;
            if (inv?.Clothes == null || data?.Clothes == null) return;
            if (inv.Clothes.Length <= 3 || data.Clothes.Length <= 3) return;
            if (!data.Clothes[3].Weared) return;

            EditorFeatures.Safe(() =>
            {
                var constraints = inv.Clothes[3].constraints;
                if (constraints == null) return;
                foreach (var c in constraints)
                    if (c != null) c.constraintActive = true;
            });
        }

        // Invector's movement locks are internal to Assembly-CSharp, so they are only
        // reachable by reflection from here. Best effort: if the fields move in a future
        // build the rest of the fix still runs.
        private static void ClearMotorLocks()
        {
            var motor = GameRefs.Motor;
            if (motor == null) return;

            EditorFeatures.Safe(() =>
            {
                var type = motor.GetType();
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                foreach (var name in new[] { "lockMovement", "lockAnimMovement", "lockRotation", "lockAnimRotation" })
                {
                    var field = type.GetField(name, flags);
                    if (field != null && field.FieldType == typeof(bool)) field.SetValue(motor, false);
                }

                motor.enabled = true;
            });
        }

        public static string StatusText
        {
            get
            {
                if (GameRefs.Inventory == null || GameRefs.Player == null) return "player not loaded";
                if (!IsBusy) return _fixCount > 0 ? $"idle - fixed {_fixCount}x" : "idle";
                return $"dressing busy {BusySeconds:0.0}s";
            }
        }
    }
}
