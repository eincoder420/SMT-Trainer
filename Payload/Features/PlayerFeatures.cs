using UnityEngine;

namespace SamanthaTrainer.Payload.Features
{
    // Movement speed, driven through the Invector third-person motor the game uses.
    public static class PlayerFeatures
    {
        private static bool  _captured;
        private static float _walk, _run, _sprint, _strafeWalk, _strafeRun, _strafeSprint;

        private static float _multiplier = 1f;

        // Scales every movement speed. Applied continuously in Tick, because the motor
        // resets speedMultiplier back to its default on various gameplay events.
        public static float SpeedMultiplier
        {
            get => _multiplier;
            set => _multiplier = Mathf.Clamp(value, 0.1f, 25f);
        }

        public static bool Enabled;

        private static void Capture()
        {
            var m = GameRefs.Motor;
            if (m == null || _captured) return;
            if (m.freeSpeed == null || m.strafeSpeed == null) return;

            _walk         = m.freeSpeed.walkSpeed;
            _run          = m.freeSpeed.runningSpeed;
            _sprint       = m.freeSpeed.sprintSpeed;
            _strafeWalk   = m.strafeSpeed.walkSpeed;
            _strafeRun    = m.strafeSpeed.runningSpeed;
            _strafeSprint = m.strafeSpeed.sprintSpeed;
            _captured = true;
        }

        // ─── Risk & happiness ─────────────────────────────────────────────────────
        // The game shows risk as Nake_Level + Watchers_Count, so holding both at zero pins
        // the meter empty. Note this also suppresses the happiness the game would normally
        // award for being seen, since that is derived from the same two values - pair it
        // with MaxHappiness if you still want the meter full.
        public static bool ZeroRisk;

        // Holds Happiness at its ceiling. The game clamps the field to 0-100.
        public static bool MaxHappiness;

        // Pins Shame at zero. Fill_Shame accumulates it while you are being watched and the
        // game starts its fail warning once it reaches 100, so holding it down keeps that
        // from ever triggering.
        public static bool ZeroEmbarrassment;

        public const float HappinessMax = 100f;

        public static void TickPlayer()
        {
            var p = GameRefs.Player;
            if (p == null) return;

            if (ZeroRisk)
            {
                p.Nake_Level = 0;
                p.Watchers_Count = 0;
            }

            if (ZeroEmbarrassment) p.Shame = 0f;
            if (MaxHappiness) p.Happiness = HappinessMax;
        }

        public static void Tick()
        {
            TickPlayer();

            var m = GameRefs.Motor;
            if (m == null) return;

            Capture();
            if (!_captured) return;

            float k = Enabled ? _multiplier : 1f;

            // speedMultiplier alone is not enough: with root motion the animation drives the
            // distance, so the base speeds have to scale too.
            m.speedMultiplier = k;
            m.freeSpeed.walkSpeed      = _walk         * k;
            m.freeSpeed.runningSpeed   = _run          * k;
            m.freeSpeed.sprintSpeed    = _sprint       * k;
            m.strafeSpeed.walkSpeed    = _strafeWalk   * k;
            m.strafeSpeed.runningSpeed = _strafeRun    * k;
            m.strafeSpeed.sprintSpeed  = _strafeSprint * k;
        }

        // Restore the speeds captured when the trainer first saw the motor.
        public static void Reset()
        {
            Enabled = false;
            _multiplier = 1f;
            Tick();
        }
    }
}
