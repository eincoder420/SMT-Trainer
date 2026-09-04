using System;
using System.Threading;

namespace TooMuchLightTrainer.Core
{
    public static class TrainerState
    {
        // Connection & Process state
        public static volatile bool IsAttached = false;
        public static volatile int GamePid = 0;
        private static long _gameBase = 0;
        public static long GameBase
        {
            get => Interlocked.Read(ref _gameBase);
            set => Interlocked.Exchange(ref _gameBase, value);
        }

        // Trainer Feature Toggles
        public static volatile bool GodModeEnabled = false;
        public static volatile bool InfiniteAmmoEnabled = false;
        public static volatile bool InfiniteStaminaEnabled = false;
        public static volatile bool DisableAIEnabled = false;
        public static volatile bool FreezeEnemiesEnabled = false;

        // Speed & Damage Multipliers
        private static int _speedBits = BitConverter.ToInt32(BitConverter.GetBytes(1.0f), 0);
        public static float SpeedMultiplier {
            get => BitConverter.ToSingle(BitConverter.GetBytes(Volatile.Read(ref _speedBits)), 0);
            set => Volatile.Write(ref _speedBits, BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }

        private static int _dmgBits = BitConverter.ToInt32(BitConverter.GetBytes(1.0f), 0);
        public static float DamageMultiplier {
            get => BitConverter.ToSingle(BitConverter.GetBytes(Volatile.Read(ref _dmgBits)), 0);
            set => Volatile.Write(ref _dmgBits, BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }
        
        // World Time Settings
        private static int _cachedTimeBits = BitConverter.ToInt32(BitConverter.GetBytes(0.0f), 0);
        public static float CachedWorldTime {
            get => BitConverter.ToSingle(BitConverter.GetBytes(Volatile.Read(ref _cachedTimeBits)), 0);
            set => Volatile.Write(ref _cachedTimeBits, BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }
        private static int _timeBits = BitConverter.ToInt32(BitConverter.GetBytes(-1.0f), 0);
        public static float WorldTimeOverride
        {
            get => BitConverter.ToSingle(BitConverter.GetBytes(Volatile.Read(ref _timeBits)), 0);
            set => Volatile.Write(ref _timeBits, BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }

        public static volatile bool FreezeTimeEnabled = false;
        public static volatile bool AntiFuckModeEnabled = false;
        public static volatile bool NoRecoilEnabled = false;
        public static volatile bool NoSpreadEnabled = false;

        // Game Memory Pointers
        public static volatile IntPtr DevPtr = IntPtr.Zero;
        public static volatile IntPtr HcPtr = IntPtr.Zero;
        public static volatile IntPtr RcPtr = IntPtr.Zero;
        public static volatile IntPtr SkillPtr = IntPtr.Zero;
        public static volatile IntPtr ArPtr = IntPtr.Zero;
        public static volatile IntPtr InvPtr = IntPtr.Zero;
        public static volatile IntPtr StatePtr = IntPtr.Zero;
        public static volatile IntPtr TakedownPtr = IntPtr.Zero;
        public static volatile IntPtr PowerPtr = IntPtr.Zero;
        public static volatile IntPtr DateTimePtr = IntPtr.Zero;
        public static volatile IntPtr SingletonPtr = IntPtr.Zero;
        public static volatile IntPtr CamRecoilPtr = IntPtr.Zero;
        public static volatile IntPtr StcPtr = IntPtr.Zero;

        // Player Stats Snapshot
        public struct StatsSnapshot
        {
            public float Hp, MaxHp, Stamina, MaxStamina;
            public int Ammo, Level, RepLv;
            public float Arousal;
            public int[] InvSlots;
        }
        private static readonly object _statsLock = new object();
        private static StatsSnapshot? _stats = null;
        public static void WriteStats(StatsSnapshot s) { lock (_statsLock) _stats = s; }
        public static StatsSnapshot? ReadStats() { lock (_statsLock) return _stats; }
        public static void ClearStats() { lock (_statsLock) _stats = null; }

        // Action Queue
        private static readonly System.Collections.Concurrent.ConcurrentQueue<Action> _pendingActions = new();
        public static void QueueAction(Action a) => _pendingActions.Enqueue(a);
        public static Action? DrainAction() => _pendingActions.TryDequeue(out var a) ? a : null;

        public static float ScreenWidth { get; set; } = 1920f;
        public static float ScreenHeight { get; set; } = 1080f;

        // HUD Text Cache
        private static string _hudText = "Waiting...";
        private static readonly object _hudLock = new object();
        public static void WriteHudText(string t) { lock (_hudLock) _hudText = t; }
        public static string ReadHudText() { lock (_hudLock) return _hudText; }

        // Reset State on Detach
        public static void ResetOnDetach()
        {
            IsAttached = false; GamePid = 0; GameBase = 0;
            DevPtr = HcPtr = RcPtr = SkillPtr = ArPtr = InvPtr = StatePtr = TakedownPtr = PowerPtr
                   = DateTimePtr = SingletonPtr = IntPtr.Zero;
            FreezeTimeEnabled = false;
            AntiFuckModeEnabled = false;
            WorldTimeOverride = -1.0f;
            ClearStats();
            while (_pendingActions.TryDequeue(out _)) { }
        }
    }
}


