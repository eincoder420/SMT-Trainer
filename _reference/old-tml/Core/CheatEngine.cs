using System;
using System.Collections.Generic;
using System.Threading;
using TooMuchLightTrainer.UI;

namespace TooMuchLightTrainer.Core
{
    /// <summary>
    /// Simple trainer logger. Thread-safe. UI can subscribe to events.
    /// </summary>
    public static class TrainerLog
    {
        public static event Action<string, LogLevel>? OnLog;
        public enum LogLevel { Info, Warn, Error, Cheat, Trace }

        public static void Log(string msg) => Emit(msg, LogLevel.Trace);
        public static void Info(string msg) => Emit(msg, LogLevel.Info);
        public static void Warn(string msg) => Emit(msg, LogLevel.Warn);
        public static void Error(string msg) => Emit(msg, LogLevel.Error);
        public static void Cheat(string msg) => Emit(msg, LogLevel.Cheat);

        private static void Emit(string msg, LogLevel lvl)
        {
            string timeStr = DateTime.Now.ToString("HH:mm:ss.fff");
            int threadId = Thread.CurrentThread.ManagedThreadId;
            string formatted = $"[{timeStr}] [T-{threadId:D2}] [{lvl}] {msg}";

            OnLog?.Invoke(formatted, lvl);
            System.Diagnostics.Debug.WriteLine(formatted);
        }
    }

    /// <summary>
    /// Manages attach/detach lifecycle and pointer resolution.
    /// All periodic work (reads/writes) lives in OverlayWindow dedicated threads.
    /// One-shot actions (RestoreHealth, AddMoney …) are queued to TrainerState.
    /// </summary>
    public class CheatEngine : IDisposable
    {
        // ─── Dependencies ──────────────────────────────────────────────────────────
        private readonly MemoryReader _mem  = new();
        private readonly MonoResolver _mono;
        public  MonoResolver MonoResolver => _mono;
        public  MemoryReader MemReader    => _mem;

        private readonly object _lock = new object();

        private DateTime _lastPointerScan = DateTime.MinValue;
        private static readonly TimeSpan PointerScanInterval = TimeSpan.FromSeconds(3);

        // Base speed values (captured once when pointers first resolve)
        private float _baseWalk, _baseRun, _baseSprint;
        private bool  _baseSpeedCaptured = false;

        public event Action<bool>? OnAttachStateChanged;

        public CheatEngine()
        {
            _mono = new MonoResolver(_mem);
        }

        // ─── Attach / Detach ───────────────────────────────────────────────────────
        public bool TryAttach()
        {
            if (!_mem.Attach("TooMuchLight"))
            {
                TrainerLog.Warn("Game not running");
                return false;
            }
            if (!_mono.Initialize())
            {
                TrainerLog.Warn("Mono module scan failed");
                return false;
            }

            TrainerState.IsAttached = true;
            TrainerState.GamePid = _mem.ProcessId;
            var (gb, _) = _mem.GetModuleBase("TooMuchLight.exe");
            TrainerState.GameBase = gb.ToInt64();

            // Reset cheat toggle flags on fresh load / attach
            TrainerState.GodModeEnabled = false;
            TrainerState.InfiniteAmmoEnabled = false;
            TrainerState.InfiniteStaminaEnabled = false;
            TrainerState.DisableAIEnabled = false;
            TrainerState.FreezeEnemiesEnabled = false;
            TrainerState.FreezeTimeEnabled = false;
            TrainerState.AntiFuckModeEnabled = false;
            TrainerState.NoRecoilEnabled = false;
            TrainerState.NoSpreadEnabled = false;
            TrainerState.SpeedMultiplier = 1.0f;
            TrainerState.WorldTimeOverride = -1.0f;

            TrainerLog.Info("Attached to TooMuchLight.exe");
            OnAttachStateChanged?.Invoke(true);
            return true;
        }

        public void Detach()
        {
            // Restore game memory states before closing handles
            ResetGameMemoryState();
            _mem.Dispose();
            _baseSpeedCaptured = false;
            TrainerState.ResetOnDetach();
            OnAttachStateChanged?.Invoke(false);
            TrainerLog.Info("Detached from game.");
        }

        private void ResetGameMemoryState()
        {
            try
            {
                var dev = TrainerState.DevPtr;
                if (dev != IntPtr.Zero)
                {
                    if (MonoResolver.DevFields.InfAmmo >= 0)
                        _mem.WriteBool(dev + MonoResolver.DevFields.InfAmmo, false);
                    if (MonoResolver.DevFields.InfStamina >= 0)
                        _mem.WriteBool(dev + MonoResolver.DevFields.InfStamina, false);
                    if (MonoResolver.DevFields.EnemyAI >= 0)
                        _mem.WriteBool(dev + MonoResolver.DevFields.EnemyAI, true);
                }

                var dt = TrainerState.DateTimePtr;
                if (dt != IntPtr.Zero)
                {
                    _mem.WriteBool(dt + MonoResolver.DateTimeFields.CanUpdateCycle, true);
                }

                // Restore base movement speeds & states
                var sp = TrainerState.StatePtr;
                if (sp != IntPtr.Zero)
                {
                    if (_baseSpeedCaptured)
                    {
                        _mem.WriteFloat(sp + MonoResolver.StateFields.Walk, _baseWalk);
                        _mem.WriteFloat(sp + MonoResolver.StateFields.Run, _baseRun);
                        _mem.WriteFloat(sp + MonoResolver.StateFields.Sprint, _baseSprint);
                    }
                    _mem.WriteByte(sp + 0x6F, 0); // Remove Anti-Fuck damageImmunity
                }

                var pwr = TrainerState.PowerPtr;
                if (pwr != IntPtr.Zero)
                {
                    _mem.WriteFloat(pwr + MonoResolver.PowerHandlerFields.EvasionChance, 0.0f);
                    _mem.WriteFloat(pwr + MonoResolver.PowerHandlerFields.KnockdownAvoidChance, 0.0f);
                }

                var hc = TrainerState.HcPtr;
                if (hc != IntPtr.Zero)
                {
                    // Restore health state to normal variables (just ensure we don't lock at 100 maxhp)
                    // CurrHP is left alone, MaxHP goes back to 100 default if it was changed
                    _mem.WriteFloat(hc + MonoResolver.HealthFields.MaxHP, 100f);
                }
                
                var stc = TrainerState.StcPtr;
                if (stc != IntPtr.Zero)
                {
                    _mem.WriteFloat(stc + 0x1A0, 100f); // Restore STC MaxHealth to 100
                }

                var rc = TrainerState.RcPtr;
                if (rc != IntPtr.Zero)
                {
                    IntPtr weapon = _mem.ReadPtr(rc + MonoResolver.RangeCombatFields.Weapon);
                    if (weapon != IntPtr.Zero)
                    {
                        // Restore some default recoil/spread values (approximate defaults for Unity weapons)
                        _mem.WriteFloat(weapon + MonoResolver.WeaponFields.MaxSpread, 2.0f);
                        _mem.WriteFloat(weapon + MonoResolver.WeaponFields.ShotgunSpread, 3.5f);
                        _mem.WriteFloat(weapon + MonoResolver.WeaponFields.RecoilX, 1.5f);
                        _mem.WriteFloat(weapon + MonoResolver.WeaponFields.RecoilY, 1.5f);
                        _mem.WriteFloat(weapon + MonoResolver.WeaponFields.KickBackForce, 1.0f);
                        _mem.WriteFloat(weapon + MonoResolver.WeaponFields.AnimatedRecoilX, 2.0f);
                        _mem.WriteFloat(weapon + MonoResolver.WeaponFields.AnimatedRecoilY, 2.0f);
                        _mem.WriteFloat(weapon + MonoResolver.WeaponFields.AnimatedKickback, 2.0f);
                    }
                }

                var camRecoil = TrainerState.CamRecoilPtr;
                if (camRecoil != IntPtr.Zero)
                {
                    _mem.WriteFloat(camRecoil + MonoResolver.CameraRecoilFields.CS_MaxUpperOffset, 5.0f);
                    _mem.WriteFloat(camRecoil + MonoResolver.CameraRecoilFields.CS_DelayBeforeReturn, 0.1f);
                }

            }
            catch { }
        }

        /// <summary>Quick status string for debug/log.</summary>
        public string StatusText => TrainerState.IsAttached
            ? $"Attached PID={TrainerState.GamePid} Dev=0x{TrainerState.DevPtr.ToInt64():X} HC=0x{TrainerState.HcPtr.ToInt64():X}"
            : "Not attached";

        /// <summary>Mirrors TrainerState.IsAttached for callers that hold a CheatEngine ref.</summary>
        public bool IsAttached => TrainerState.IsAttached;

        /// <summary>
        /// Returns the ReferencesHandler managed object pointer by reading
        /// Singleton SFD &lt;References&gt;k__BackingField.
        /// Result is cached in a private field to avoid repeated SFD walks.
        /// </summary>
        private IntPtr _refPtrCache = IntPtr.Zero;
        public IntPtr GetRefPtr()
        {
            if (_refPtrCache != IntPtr.Zero) return _refPtrCache;
            // Resolve via Singleton SFD (same path as TryFindSingletonInstances)
            if (!_mono.TryFindSingletonInstances(out _, out _, out _, out _, out _, out _, out _))
                return IntPtr.Zero;
            // After TryFindSingletonInstances the Singleton SFD has been walked;
            // now read <References> field directly
            // We piggyback on RefreshPointers which already writes HcPtr/RcPtr via refPtr internally
            // Re-resolve here by calling FindSingletonInstances to expose refPtr
            _refPtrCache = ResolveSingletonRefPtr();
            return _refPtrCache;
        }

        private IntPtr ResolveSingletonRefPtr()
        {
            // Mirror of what TryFindSingletonInstances does internally to get refPtr
            // Access _mono internals via exposed helper
            return _mono.GetReferencesHandlerPtr();
        }


        // ─── Pointer resolution (called by MemRead thread) ────────────────────────
        /// <summary>
        /// Resolves all component pointers and writes them into TrainerState.
        /// Returns true when all critical pointers are resolved.
        /// </summary>
        public bool RefreshPointers()
        {
            bool allFound = false;
            
            if (_mono.TryFindSingletonInstances(out var dev, out var hc, out var rc, out var skill, out var ar, out var inv, out var state))
            {
                TrainerState.DevPtr   = dev;
                TrainerState.HcPtr    = hc;
                TrainerState.RcPtr    = rc;
                TrainerState.SkillPtr = skill;
                TrainerState.ArPtr    = ar;
                TrainerState.InvPtr   = inv;
                TrainerState.StatePtr = state;
            }
            else
            {
                TrainerState.DevPtr = IntPtr.Zero;
                TrainerState.HcPtr = IntPtr.Zero;
                TrainerState.RcPtr = IntPtr.Zero;
                TrainerState.SkillPtr = IntPtr.Zero;
                TrainerState.ArPtr = IntPtr.Zero;
                TrainerState.InvPtr = IntPtr.Zero;
                TrainerState.StatePtr = IntPtr.Zero;
                TrainerState.CamRecoilPtr = IntPtr.Zero;
                TrainerState.PowerPtr = IntPtr.Zero;
                TrainerState.TakedownPtr = IntPtr.Zero;
                TrainerState.DateTimePtr = IntPtr.Zero;
                TrainerState.StcPtr = IntPtr.Zero;
            }

            allFound = TrainerState.DevPtr   != IntPtr.Zero
                    && TrainerState.HcPtr    != IntPtr.Zero
                    && TrainerState.SkillPtr != IntPtr.Zero
                    && TrainerState.ArPtr    != IntPtr.Zero
                    && TrainerState.InvPtr   != IntPtr.Zero
                    && TrainerState.RcPtr    != IntPtr.Zero;
            if (allFound) return true;

            // Rate-limit fallback heap scans
            var now = DateTime.UtcNow;
            if (now - _lastPointerScan < PointerScanInterval) return false;
            _lastPointerScan = now;

            IntPtr tmp;
            if (TrainerState.DevPtr   == IntPtr.Zero && _mono.TryFindDevInstance(out tmp))         TrainerState.DevPtr   = tmp;
            if (TrainerState.HcPtr    == IntPtr.Zero && _mono.TryFindHealthComponent(out tmp))     TrainerState.HcPtr    = tmp;
            if (TrainerState.SkillPtr == IntPtr.Zero && _mono.TryFindSkillSystem(out tmp))         TrainerState.SkillPtr = tmp;
            if (TrainerState.ArPtr    == IntPtr.Zero && _mono.TryFindArousal(out tmp))             TrainerState.ArPtr    = tmp;
            if (TrainerState.RcPtr    == IntPtr.Zero && _mono.TryFindRangeComp(out tmp))           TrainerState.RcPtr    = tmp;
            if (TrainerState.InvPtr   == IntPtr.Zero && _mono.TryFindInventoryComponent(out tmp))  TrainerState.InvPtr   = tmp;
            if (TrainerState.StcPtr   == IntPtr.Zero && _mono.TryFindSaveTempContainer(out tmp))   TrainerState.StcPtr   = tmp;

            return false;
        }

        // ─── Memory read helpers (called by MemRead thread each tick) ─────────────
        public TrainerState.StatsSnapshot ReadStatsFromMemory()
        {
            var s = new TrainerState.StatsSnapshot
            {
                InvSlots = new int[24]
            };

            var hc    = TrainerState.HcPtr;
            var skill = TrainerState.SkillPtr;
            var rc    = TrainerState.RcPtr;
            var ar    = TrainerState.ArPtr;
            var inv   = TrainerState.InvPtr;

            if (hc    != IntPtr.Zero) { s.Hp         = _mem.ReadFloat(hc    + MonoResolver.HealthFields.CurrHP);  s.MaxHp      = _mem.ReadFloat(hc + MonoResolver.HealthFields.MaxHP); }
            if (hc    != IntPtr.Zero) { s.Stamina     = _mem.ReadFloat(hc    + MonoResolver.HealthFields.CurrStam); s.MaxStamina = _mem.ReadFloat(hc + MonoResolver.HealthFields.MaxStam); }
            if (skill != IntPtr.Zero) { s.Level        = _mem.ReadInt32(skill + _mono.OffSkill_Level); s.RepLv = _mem.ReadInt32(skill + _mono.OffSkill_RepLv); }
            if (rc    != IntPtr.Zero) { s.Ammo         = _mem.ReadInt32(rc    + _mono.OffAmmo_Current); }
            if (ar    != IntPtr.Zero) { s.Arousal      = _mem.ReadFloat(ar    + _mono.OffArousal_Val); }

            TrainerState.CachedWorldTime = GetWorldTime();

            if (inv != IntPtr.Zero)
            {
                IntPtr list = _mem.ReadPtr(inv + 0x3B8);
                if (list != IntPtr.Zero)
                    s.InvSlots = _mono.ReadInventorySlotAmounts(list, 24);
            }

            return s;
        }

        // ─── Cheat write helpers (called by MemWrite thread each tick) ────────────
        public void ApplyAllCheats()
        {
            ApplyDevFlags();
            ApplyGodMode();
            ApplySpeedMultiplier();
            ApplyWeaponCheats();
            ApplyAntiFuckMode();
            ApplyEconomyCheats();
        }

        private void ApplyEconomyCheats()
        {
            if (_overrideLevel.HasValue && _overrideLevel.Value > -1)
            {
                var skill = TrainerState.SkillPtr;
                if (skill != IntPtr.Zero) {
                    _mem.WriteInt32IfChanged(skill + 0xE8, _overrideLevel.Value);
                }
            }
            if (_overrideRep.HasValue && _overrideRep.Value > -1)
            {
                var skill = TrainerState.SkillPtr;
                if (skill != IntPtr.Zero) {
                    _mem.WriteInt32IfChanged(skill + 0xFC, _overrideRep.Value);
                }
            }
            if (_overrideArousal.HasValue && _overrideArousal.Value > -1)
            {
                var hc = TrainerState.HcPtr;
                if (hc != IntPtr.Zero) {
                    _mem.WriteFloatIfChanged(hc + 0x128, _overrideArousal.Value);
                }
                var ar = TrainerState.ArPtr;
                if (ar != IntPtr.Zero) {
                    _mem.WriteFloatIfChanged(ar + 0x2C, _overrideArousal.Value);
                }
            }

            // ─── World Time / Freeze ────────────────────────────────────
            var dt = TrainerState.DateTimePtr;
            if (dt != IntPtr.Zero)
            {
                // Freeze time — controls canUpdateCycle (false = frozen, true = un-frozen)
                _mem.WriteBoolIfChanged(dt + MonoResolver.DateTimeFields.CanUpdateCycle, !TrainerState.FreezeTimeEnabled);

                // World time override — slider sets this to 0–24; -1 = no override
                float wto = TrainerState.WorldTimeOverride;
                if (wto >= 0f && wto <= 24f)
                    _mem.WriteFloatIfChanged(dt + MonoResolver.DateTimeFields.TimeOfDay, wto);
            }
        }

        private void ApplyDevFlags()
        {
            var dev = TrainerState.DevPtr;
            if (dev != IntPtr.Zero)
            {
                if (MonoResolver.DevFields.InfAmmo >= 0)
                    _mem.WriteBoolIfChanged(dev + MonoResolver.DevFields.InfAmmo, TrainerState.InfiniteAmmoEnabled);
                if (MonoResolver.DevFields.InfStamina >= 0)
                    _mem.WriteBoolIfChanged(dev + MonoResolver.DevFields.InfStamina, TrainerState.InfiniteStaminaEnabled);
                if (MonoResolver.DevFields.EnemyAI >= 0)
                    _mem.WriteBoolIfChanged(dev + MonoResolver.DevFields.EnemyAI, !TrainerState.DisableAIEnabled);
            }

            if (TrainerState.InfiniteStaminaEnabled)
            {
                var hc = TrainerState.HcPtr;
                if (hc != IntPtr.Zero && MonoResolver.HealthFields.CurrStam >= 0)
                {
                    float maxStam = _mem.ReadFloat(hc + MonoResolver.HealthFields.MaxStam);
                    if (maxStam > 0f)
                        _mem.WriteFloatIfChanged(hc + MonoResolver.HealthFields.CurrStam, maxStam);
                }
            }
        }

        private void ApplyGodMode()
        {
            if (!TrainerState.GodModeEnabled) return;
            var hc = TrainerState.HcPtr;
            if (hc != IntPtr.Zero)
            {
                _mem.WriteFloatIfChanged(hc + MonoResolver.HealthFields.MaxHP, 100f);
                _mem.WriteFloatIfChanged(hc + MonoResolver.HealthFields.CurrHP, 100f);
                _mem.WriteFloatIfChanged(hc + 0x38, 0f); // timeBeforeRecovery = 0
                _mem.WriteByteIfChanged(hc + 0x51, 1);   // canRecovery = true (triggers instant UI refresh)
            }
            var stc = TrainerState.StcPtr;
            if (stc != IntPtr.Zero)
            {
                _mem.WriteFloatIfChanged(stc + 0x190, 100f); // currentHealth
                _mem.WriteFloatIfChanged(stc + 0x1A0, 100f); // maxHealth
            }
        }

        private void ApplySpeedMultiplier()
        {
            float m = TrainerState.SpeedMultiplier;
            if (m <= 1.01f && m >= 0.99f) return; // Allow game to naturally handle speed if at 1x

            var sp = TrainerState.StatePtr;
            if (sp == IntPtr.Zero) return;
            if (!_baseSpeedCaptured)
            {
                _baseWalk   = _mem.ReadFloat(sp + MonoResolver.StateFields.Walk);
                _baseRun    = _mem.ReadFloat(sp + MonoResolver.StateFields.Run);
                _baseSprint = _mem.ReadFloat(sp + MonoResolver.StateFields.Sprint);
                // Ensure we capture base walking speed, not the slowed-down aiming speed
                if (_baseWalk > 0.5f && _baseWalk < 5.0f) _baseSpeedCaptured = true;
                else return;
            }
            if (!_baseSpeedCaptured) return;
            bool isAiming = _mem.ReadBool(sp + MonoResolver.StateFields.Aim);

            float aimMod = isAiming ? 0.4f : 1.0f; // Aiming speed is about 40% of normal

            _mem.WriteFloatIfChanged(sp + MonoResolver.StateFields.Walk,   _baseWalk * m * aimMod);
            _mem.WriteFloatIfChanged(sp + MonoResolver.StateFields.Run,    _baseRun * m * aimMod);
            _mem.WriteFloatIfChanged(sp + MonoResolver.StateFields.Sprint, _baseSprint * m * aimMod);
        }

        private void ApplyWeaponCheats()
        {
            var rc = TrainerState.RcPtr;
            if (rc == IntPtr.Zero) return;

            IntPtr weapon = _mem.ReadPtr(rc + MonoResolver.RangeCombatFields.Weapon);
            if (weapon != IntPtr.Zero)
            {
                // Recoil on ScriptableObject
                if (TrainerState.NoRecoilEnabled)
                {
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.RecoilX, 0f);
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.RecoilY, 0f);
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.RecoilZ, 0f);
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.KickBackForce, 0f);
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.AnimatedRecoilX, 0f);
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.AnimatedRecoilY, 0f);
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.AnimatedRecoilZ, 0f);
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.AnimatedKickback, 0f);
                }

                // Spread on ScriptableObject
                if (TrainerState.NoSpreadEnabled)
                {
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.MaxSpread, 0f);
                    _mem.WriteFloatIfChanged(weapon + MonoResolver.WeaponFields.ShotgunSpread, 0f);
                }
            }

            // RangeCombatComponent spread zeroes
            if (TrainerState.NoSpreadEnabled)
            {
                _mem.WriteFloatIfChanged(rc + MonoResolver.RangeCombatFields.CurrentSpread, 0f);
            }

            // CameraRecoil component zeroes (kills rifle / gun camera bounce)
            if (TrainerState.NoRecoilEnabled)
            {
                var camRecoil = TrainerState.CamRecoilPtr;
                if (camRecoil != IntPtr.Zero)
                {
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CS_MaxUpperOffset, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CS_DelayBeforeReturn, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CS_TargetOffsetShootingX, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CS_TargetOffsetShootingY, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CS_TargetOffsetShootingZ, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CS_CurrentOffsetShootingX, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CS_CurrentOffsetShootingY, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CS_CurrentOffsetShootingZ, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.TargetRotationX, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.TargetRotationY, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.TargetRotationZ, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CurrentRotationX, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CurrentRotationY, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CurrentRotationZ, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CurrentRecoilX, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CurrentRecoilY, 0f);
                    _mem.WriteFloatIfChanged(camRecoil + MonoResolver.CameraRecoilFields.CurrentRecoilZ, 0f);
                }
            }
        }

        private void ApplyAntiFuckMode()
        {
            var pwr = TrainerState.PowerPtr;
            var sp = TrainerState.StatePtr;

            if (TrainerState.AntiFuckModeEnabled)
            {
                if (sp != IntPtr.Zero)
                {
                    _mem.WriteByteIfChanged(sp + 0x69, 0); // _captured
                    _mem.WriteByteIfChanged(sp + 0x6A, 0); // _grabbed
                    _mem.WriteByteIfChanged(sp + 0x6B, 0); // _immobilized
                    _mem.WriteByteIfChanged(sp + 0x6C, 0); // _trapped
                    _mem.WriteByteIfChanged(sp + 0x6D, 0); // _sex
                    _mem.WriteByteIfChanged(sp + 0x6F, 1); // damageImmunity
                }

                if (pwr != IntPtr.Zero)
                {
                    _mem.WriteFloatIfChanged(pwr + MonoResolver.PowerHandlerFields.EvasionChance, 100.0f);
                    _mem.WriteFloatIfChanged(pwr + MonoResolver.PowerHandlerFields.KnockdownAvoidChance, 100.0f);
                }
            }
            else
            {
                if (sp != IntPtr.Zero)
                {
                    _mem.WriteByteIfChanged(sp + 0x6F, 0); // damageImmunity reset
                }

                if (pwr != IntPtr.Zero)
                {
                    _mem.WriteFloatIfChanged(pwr + MonoResolver.PowerHandlerFields.EvasionChance, 0.0f);
                    _mem.WriteFloatIfChanged(pwr + MonoResolver.PowerHandlerFields.KnockdownAvoidChance, 0.0f);
                }
            }
        }

        // ─── One-shot actions (queued from menu to MemWrite thread) ───────────────
        public void RestoreHealthAndStamina()
        {
            TrainerState.QueueAction(() =>
            {
                var hc = TrainerState.HcPtr;
                if (hc != IntPtr.Zero)
                {
                    _mem.WriteFloatIfChanged(hc + MonoResolver.HealthFields.MaxHP, 100f);
                    _mem.WriteFloatIfChanged(hc + MonoResolver.HealthFields.CurrHP, 100f);
                    _mem.WriteFloatIfChanged(hc + 0x38, 0f); // timeBeforeRecovery = 0
                    _mem.WriteByteIfChanged(hc + 0x51, 1);   // canRecovery = true (triggers instant UI refresh)
                    float maxStam = _mem.ReadFloat(hc + MonoResolver.HealthFields.MaxStam);
                    _mem.WriteFloatIfChanged(hc + MonoResolver.HealthFields.CurrStam, maxStam);
                }

                var stc = TrainerState.StcPtr;
                if (stc == IntPtr.Zero && _mono.TryFindSaveTempContainer(out IntPtr tmpStc))
                {
                    TrainerState.StcPtr = tmpStc;
                    stc = tmpStc;
                }
                if (stc != IntPtr.Zero)
                {
                    _mem.WriteFloatIfChanged(stc + 0x190, 100f); // currentHealth
                    _mem.WriteFloatIfChanged(stc + 0x1A0, 100f); // maxHealth
                }
                
                TrainerLog.Cheat($"Health & Stamina restored to 100");
            });
        }

        public void AddMoney(int amount)
        {
            TrainerState.QueueAction(() => 
            {
                var invPtr = TrainerState.InvPtr;
                if (invPtr == IntPtr.Zero && _mono.TryFindInventoryComponent(out IntPtr tmp))
                {
                    TrainerState.InvPtr = tmp;
                    invPtr = tmp;
                }

                if (invPtr == IntPtr.Zero)
                {
                    TrainerLog.Error("Money Add Failed: InventoryComponent pointer is null");
                    NotificationManager.ShowError("Money Add Failed: Open inventory first!");
                    return;
                }

                // InventoryComponent -> currentItemsInventory (+0x3C0) or itemsInventory (+0x3B8)
                IntPtr itemsInv = _mem.ReadPtr(invPtr + 0x3C0);
                if (itemsInv == IntPtr.Zero) itemsInv = _mem.ReadPtr(invPtr + 0x3B8);

                if (itemsInv == IntPtr.Zero)
                {
                    TrainerLog.Error("Money Add Failed: itemsInventory object is null");
                    NotificationManager.ShowError("Money Add Failed: itemsInventory null");
                    return;
                }

                // Inventory<Item> -> slots (List<ItemSlot>) is at offset +0x20 in SDK
                IntPtr slotsList = _mem.ReadPtr(itemsInv + 0x20);
                if (slotsList == IntPtr.Zero) slotsList = _mem.ReadPtr(itemsInv + 0x18);
                if (slotsList == IntPtr.Zero) slotsList = _mem.ReadPtr(itemsInv + 0x10);

                if (slotsList == IntPtr.Zero)
                {
                    TrainerLog.Error("Money Add Failed: Slots list pointer is null");
                    NotificationManager.ShowError("Money Add Failed: Inventory slots list null");
                    return;
                }

                IntPtr itemsArray = _mem.ReadPtr(slotsList + 0x10); // List._items array
                int size = _mem.ReadInt32(slotsList + 0x18);        // List._size

                if (itemsArray == IntPtr.Zero)
                {
                    TrainerLog.Error("Money Add Failed: Invalid items array pointer");
                    NotificationManager.ShowError("Money Add Failed: Items array null");
                    return;
                }

                int arrayLength = _mem.ReadInt32(itemsArray + 0x18);
                int maxCount = (size > 0 && size <= 256) ? size : ((arrayLength > 0 && arrayLength <= 256) ? arrayLength : 32);

                bool moneyFound = false;
                for (int i = 0; i < maxCount; i++)
                {
                    IntPtr slotPtr = _mem.ReadPtr(itemsArray + 0x20 + i * 8);
                    if (slotPtr == IntPtr.Zero) continue;

                    IntPtr itemPtr = _mem.ReadPtr(slotPtr + 0x10);       // ItemSlot.item
                    int currentAmount = _mem.ReadInt32(slotPtr + 0x18);  // ItemSlot.amount

                    if (itemPtr != IntPtr.Zero)
                    {
                        IntPtr namePtr = _mem.ReadPtr(itemPtr + 0x18); // Item.itemName (MonoString)
                        string itemName = _mem.ReadMonoString(namePtr);

                        if (!string.IsNullOrEmpty(itemName) && (itemName.Equals("Money", StringComparison.OrdinalIgnoreCase) || itemName.Equals("Cash", StringComparison.OrdinalIgnoreCase) || itemName.Contains("Money", StringComparison.OrdinalIgnoreCase)))
                        {
                            int newAmount = currentAmount + amount;
                            _mem.WriteInt32(slotPtr + 0x18, newAmount);
                            moneyFound = true;
                            TrainerLog.Cheat($"Money added: +${amount} (New Total: ${newAmount})");
                            NotificationManager.ShowSuccess($"+${amount} Cash Added! (Total: ${newAmount})");
                            break;
                        }
                    }
                }

                if (!moneyFound)
                {
                    // If money item object was not in slots yet, assign slot 0 amount or set money item reference from ItemsDB
                    IntPtr slot0 = _mem.ReadPtr(itemsArray + 0x20);
                    if (slot0 != IntPtr.Zero)
                    {
                        int cur = _mem.ReadInt32(slot0 + 0x18);
                        int newAmt = cur + amount;
                        _mem.WriteInt32(slot0 + 0x18, newAmt);
                        TrainerLog.Cheat($"Money added to slot 0: +${amount} (Total: ${newAmt})");
                        NotificationManager.ShowSuccess($"+${amount} Cash Added! (Total: ${newAmt})");
                    }
                    else
                    {
                        TrainerLog.Error("Money Add Failed: Could not locate Money item or valid slot");
                        NotificationManager.ShowError("Money Add Failed: Money item slot not found");
                    }
                }
            });
        }
        public void AddExp(int amount)
        {
            TrainerState.QueueAction(() => 
            {
                var ss = TrainerState.SkillPtr;
                if (ss != IntPtr.Zero)
                {
                    float currentExp = _mem.ReadFloat(ss + MonoResolver.SkillSystemFields.CurrentExp);
                    _mem.WriteFloat(ss + MonoResolver.SkillSystemFields.CurrentExp, currentExp + amount);
                    TrainerLog.Cheat($"Exp increased by {amount}");
                }
                else TrainerLog.Warn("SkillSystem not found");
            });
        }
        
        public void AddSkillPoints(int amount)
        {
            TrainerState.QueueAction(() => 
            {
                var ss = TrainerState.SkillPtr;
                if (ss != IntPtr.Zero)
                {
                    int currentSP = _mem.ReadInt32(ss + MonoResolver.SkillSystemFields.CurrentSkillPoints);
                    _mem.WriteInt32(ss + MonoResolver.SkillSystemFields.CurrentSkillPoints, currentSP + amount);
                    TrainerLog.Cheat($"Skill Points increased by {amount}");
                }
                else TrainerLog.Warn("SkillSystem not found");
            });
        }
        
        public void AddReputation(int amount)
        {
            TrainerState.QueueAction(() => 
            {
                var ss = TrainerState.SkillPtr;
                if (ss != IntPtr.Zero)
                {
                    float currentRep = _mem.ReadFloat(ss + MonoResolver.SkillSystemFields.CurrentRepExp);
                    _mem.WriteFloat(ss + MonoResolver.SkillSystemFields.CurrentRepExp, currentRep + amount);
                    TrainerLog.Cheat($"Reputation increased by {amount}");
                }
                else TrainerLog.Warn("SkillSystem not found");
            });
        }

        private void LogPending(string type, int amount)
            => TrainerLog.Cheat($"Action queued: {type} +{amount} (pending pointer resolve)");

        public struct InventorySlotDetail
        {
            public int SlotIndex;
            public string ItemName;
            public int Amount;
            public IntPtr SlotPtr;
            public IntPtr ItemPtr;
        }

        public List<InventorySlotDetail> GetInventorySlotDetails(int maxSlots = 12)
        {
            var details = new List<InventorySlotDetail>();
            var invPtr = TrainerState.InvPtr;
            if (invPtr == IntPtr.Zero) return details;

            IntPtr itemsInv = _mem.ReadPtr(invPtr + 0x3C0);
            if (itemsInv == IntPtr.Zero) itemsInv = _mem.ReadPtr(invPtr + 0x3B8);
            if (itemsInv == IntPtr.Zero) return details;

            IntPtr slotsList = _mem.ReadPtr(itemsInv + 0x20);
            if (slotsList == IntPtr.Zero) slotsList = _mem.ReadPtr(itemsInv + 0x18);
            if (slotsList == IntPtr.Zero) slotsList = _mem.ReadPtr(itemsInv + 0x10);
            if (slotsList == IntPtr.Zero) return details;

            IntPtr itemsArray = _mem.ReadPtr(slotsList + 0x10);
            int size = _mem.ReadInt32(slotsList + 0x18);
            if (itemsArray == IntPtr.Zero) return details;

            int count = (size > 0 && size <= maxSlots) ? size : maxSlots;
            for (int i = 0; i < count; i++)
            {
                IntPtr slotPtr = _mem.ReadPtr(itemsArray + 0x20 + i * 8);
                if (slotPtr == IntPtr.Zero) continue;

                IntPtr itemPtr = _mem.ReadPtr(slotPtr + 0x10);
                int amount = _mem.ReadInt32(slotPtr + 0x18);
                string name = "[Empty]";

                if (itemPtr != IntPtr.Zero)
                {
                    IntPtr namePtr = _mem.ReadPtr(itemPtr + 0x18);
                    string readName = _mem.ReadMonoString(namePtr);
                    if (!string.IsNullOrEmpty(readName)) name = readName;
                }

                details.Add(new InventorySlotDetail
                {
                    SlotIndex = i + 1,
                    ItemName  = name,
                    Amount    = amount,
                    SlotPtr   = slotPtr,
                    ItemPtr   = itemPtr
                });
            }

            return details;
        }

        public void AddItem(string targetItemName, int amount)
        {
            TrainerState.QueueAction(() => 
            {
                var invPtr = TrainerState.InvPtr;
                if (invPtr == IntPtr.Zero && _mono.TryFindInventoryComponent(out IntPtr tmp))
                {
                    TrainerState.InvPtr = tmp;
                    invPtr = tmp;
                }

                if (invPtr == IntPtr.Zero)
                {
                    TrainerLog.Error($"Add Item Failed: Player Inventory not attached");
                    NotificationManager.ShowError($"Item Add Failed: Open inventory first!");
                    return;
                }

                IntPtr itemsInv = _mem.ReadPtr(invPtr + 0x3C0);
                if (itemsInv == IntPtr.Zero) itemsInv = _mem.ReadPtr(invPtr + 0x3B8);
                if (itemsInv == IntPtr.Zero) return;

                IntPtr slotsList = _mem.ReadPtr(itemsInv + 0x20);
                if (slotsList == IntPtr.Zero) slotsList = _mem.ReadPtr(itemsInv + 0x18);
                if (slotsList == IntPtr.Zero) slotsList = _mem.ReadPtr(itemsInv + 0x10);
                if (slotsList == IntPtr.Zero) return;

                IntPtr itemsArray = _mem.ReadPtr(slotsList + 0x10);
                int size = _mem.ReadInt32(slotsList + 0x18);
                if (itemsArray == IntPtr.Zero) return;

                int arrayLength = _mem.ReadInt32(itemsArray + 0x18);
                int maxCount = (size > 0 && size <= 256) ? size : ((arrayLength > 0 && arrayLength <= 256) ? arrayLength : 32);

                bool itemFound = false;
                IntPtr emptySlotPtr = IntPtr.Zero;

                for (int i = 0; i < maxCount; i++)
                {
                    IntPtr slotPtr = _mem.ReadPtr(itemsArray + 0x20 + i * 8);
                    if (slotPtr == IntPtr.Zero) continue;

                    IntPtr itemPtr = _mem.ReadPtr(slotPtr + 0x10);
                    int currentAmount = _mem.ReadInt32(slotPtr + 0x18);

                    if (itemPtr != IntPtr.Zero)
                    {
                        IntPtr namePtr = _mem.ReadPtr(itemPtr + 0x18);
                        string itemName = _mem.ReadMonoString(namePtr);

                        if (!string.IsNullOrEmpty(itemName) && itemName.Contains(targetItemName, StringComparison.OrdinalIgnoreCase))
                        {
                            int newAmount = currentAmount + amount;
                            _mem.WriteInt32(slotPtr + 0x18, newAmount);
                            itemFound = true;
                            TrainerLog.Cheat($"Item updated: {targetItemName} +{amount} (Total: {newAmount})");
                            NotificationManager.ShowSuccess($"+{amount} {targetItemName} Added! (Total: {newAmount})");
                            break;
                        }
                    }
                    else if (emptySlotPtr == IntPtr.Zero && currentAmount == 0)
                    {
                        emptySlotPtr = slotPtr;
                    }
                }

                if (!itemFound)
                {
                    if (emptySlotPtr != IntPtr.Zero)
                    {
                        _mem.WriteInt32(emptySlotPtr + 0x18, amount);
                        TrainerLog.Cheat($"Item added to empty slot: {targetItemName} x{amount}");
                        NotificationManager.ShowSuccess($"+{amount} {targetItemName} Added to Slot");
                    }
                    else
                    {
                        IntPtr slot0 = _mem.ReadPtr(itemsArray + 0x20);
                        if (slot0 != IntPtr.Zero)
                        {
                            int cur = _mem.ReadInt32(slot0 + 0x18);
                            int newAmt = cur + amount;
                            _mem.WriteInt32(slot0 + 0x18, newAmt);
                            NotificationManager.ShowSuccess($"+{amount} {targetItemName} Added! (Total: {newAmt})");
                        }
                    }
                }
            });
        }

        public int GetSlotAmount(IntPtr slotPtr)
        {
            if (slotPtr == IntPtr.Zero) return 0;
            return _mem.ReadInt32(slotPtr + 0x18);
        }

        public void SetSlotAmount(IntPtr slotPtr, int newAmount)
        {
            if (slotPtr == IntPtr.Zero) return;
            TrainerState.QueueAction(() =>
            {
                _mem.WriteInt32(slotPtr + 0x18, Math.Max(0, newAmount));
                TrainerLog.Cheat($"Slot amount set to {newAmount}");
                NotificationManager.ShowSuccess($"Slot amount updated: {newAmount}");
            });
        }

        private int? _overrideLevel;
        private int? _overrideRep;
        private float? _overrideArousal;

        public int GetCurrentLevel() => _overrideLevel ?? TrainerState.ReadStats()?.Level ?? 2;
        public int GetCurrentReputation() => _overrideRep ?? TrainerState.ReadStats()?.RepLv ?? 1;
        public float GetCurrentArousal() => _overrideArousal ?? TrainerState.ReadStats()?.Arousal ?? 64f;

        public void SetLevel(int level)
        {
            _overrideLevel = level;
            TrainerState.QueueAction(() =>
            {
                var skill = TrainerState.SkillPtr;
                if (skill != IntPtr.Zero)
                {
                    _mem.WriteInt32(skill + 0xE8, level); // SkillSystem.currentLevel
                    _mem.WriteFloat(skill + 0xF0, 0f);   // SkillSystem.currentExp

                    IntPtr levelTxtPtr = _mem.ReadPtr(skill + 0xA8);
                    if (levelTxtPtr == IntPtr.Zero) levelTxtPtr = _mem.ReadPtr(skill + 0x80);
                    if (levelTxtPtr != IntPtr.Zero)
                    {
                        IntPtr strPtr = _mem.ReadPtr(levelTxtPtr + 0xD8);
                        if (strPtr == IntPtr.Zero) strPtr = _mem.ReadPtr(levelTxtPtr + 0xC8);
                        if (strPtr != IntPtr.Zero) _mem.WriteInt32(strPtr + 0x14, level);
                    }

                    TrainerLog.Cheat($"Level set to {level}");
                    NotificationManager.ShowSuccess($"Level Set to {level}");
                }
                else NotificationManager.ShowError("SkillSystem not found");
            });
        }

        public void SetReputationLevel(int rep)
        {
            _overrideRep = rep;
            TrainerState.QueueAction(() =>
            {
                var skill = TrainerState.SkillPtr;
                if (skill != IntPtr.Zero)
                {
                    _mem.WriteInt32(skill + 0xFC, rep);  // SkillSystem.currentRepLevel
                    _mem.WriteFloat(skill + 0x104, 0f); // SkillSystem.currentRepExp
                    TrainerLog.Cheat($"Reputation Level set to {rep}");
                    NotificationManager.ShowSuccess($"Reputation Level Set to {rep}");
                }
                else NotificationManager.ShowError("SkillSystem not found");
            });
        }

        public void SetArousal(float arousal)
        {
            _overrideArousal = arousal;
            TrainerState.QueueAction(() =>
            {
                var hc = TrainerState.HcPtr;
                if (hc != IntPtr.Zero)
                {
                    _mem.WriteFloat(hc + 0x128, arousal); // GirlHealth.currentArousal
                }
                var ar = TrainerState.ArPtr;
                if (ar != IntPtr.Zero)
                {
                    _mem.WriteFloat(ar + 0x2C, arousal);
                }
                TrainerLog.Cheat($"Arousal set to {arousal}");
                NotificationManager.ShowSuccess($"Arousal Set to {arousal}");
            });
        }

        // ─── World Time / Freeze ─────────────────────────────────────
        public float GetWorldTime()
        {
            var dt = TrainerState.DateTimePtr;
            if (dt == IntPtr.Zero) return TrainerState.WorldTimeOverride >= 0f ? TrainerState.WorldTimeOverride : 12f;
            return _mem.ReadFloat(dt + MonoResolver.DateTimeFields.TimeOfDay);
        }

        public void SetWorldTime(float hours)
        {
            // Clamp to 0–24 and store as override so ApplyEconomyCheats writes it each tick
            hours = Math.Max(0f, Math.Min(24f, hours));
            TrainerState.WorldTimeOverride = hours;

            TrainerState.QueueAction(() =>
            {
                // Also disable freeze so the user can slide through the clock freely
                // (they can re-enable Freeze Time separately)
                var dt = TrainerState.DateTimePtr;
                if (dt != IntPtr.Zero)
                    _mem.WriteFloatIfChanged(dt + MonoResolver.DateTimeFields.TimeOfDay, hours);
            });
        }

        public void SetFreezeTime(bool freeze)
        {
            TrainerState.FreezeTimeEnabled = freeze;
            TrainerState.QueueAction(() =>
            {
                var dt = TrainerState.DateTimePtr;
                if (dt != IntPtr.Zero)
                    _mem.WriteBoolIfChanged(dt + MonoResolver.DateTimeFields.CanUpdateCycle, !freeze);
                TrainerLog.Cheat($"Freeze Time: {freeze}");
            });
        }

        // Dispose
        public void Dispose() => _mem.Dispose();
    }
}
