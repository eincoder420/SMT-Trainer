using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TooMuchLightTrainer.Core;
using WpfColor = System.Windows.Media.Color;

namespace TooMuchLightTrainer.UI
{
    public partial class OverlayWindow : Window
    {
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
        [DllImport("user32.dll")] private static extern IntPtr FindWindow(string? cls, string title);
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vk);
        [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        [StructLayout(LayoutKind.Sequential)] struct RECT { public int L, T, R, B; }
        private static bool IsKeyDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;



        private readonly CheatEngine _cheat;
        private readonly InputHook   _input;
        private readonly NativeMenu  _menu;
        private readonly DebugPanel  _debugPanel;

        private Thread? _gameWatchThread;
        private Thread? _memReadThread;
        private Thread? _memWriteThread;
        private Thread? _debugThread;
        private volatile bool _running = true;
        private volatile bool _debugEnabled = false;
        private volatile bool _showDebugMenu = false;

        private IntPtr _gameHwnd = IntPtr.Zero;
        private int    _dotCycle;
        private static readonly string[] Dots = { ".", "..", "..." };

        private float _addPistolAmmoAmt  = 10f;
        private float _addRifleAmmoAmt   = 30f;
        private float _addShotgunAmmoAmt = 10f;

        public OverlayWindow(CheatEngine cheat)
        {
            InitializeComponent();
            _cheat      = cheat;
            _input      = new InputHook();
            _menu       = new NativeMenu(cheat);
            _debugPanel = new DebugPanel(cheat);

            MenuCanvas.Children.Add(_menu);
            Canvas.SetLeft(_debugPanel, 335);
            Canvas.SetTop (_debugPanel, 0);
            MenuCanvas.Children.Add(_debugPanel);


            _input.OnToggleMenu += () => Dispatcher.BeginInvoke(ToggleMenu);
            _input.OnUp    += () => Dispatcher.BeginInvoke(() => _menu.NavigateUp());
            _input.OnDown  += () => Dispatcher.BeginInvoke(() => _menu.NavigateDown());
            _input.OnLeft  += () => Dispatcher.BeginInvoke(() => _menu.NavigateLeft());
            _input.OnRight += () => Dispatcher.BeginInvoke(() => _menu.NavigateRight());
            _input.OnSelect+= () => Dispatcher.BeginInvoke(() => { if (_menu.IsShowing) _menu.Select(); });
            _input.OnBack  += () => Dispatcher.BeginInvoke(NavigateBack);

            Width  = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;

            NotificationManager.Initialize(NotificationHost);
            NotificationManager.ApplyTheme(MenuThemes.All[0]);

            BuildMenuStructure();
            SpawnParticles();

            _cheat.OnAttachStateChanged += attached => Dispatcher.BeginInvoke(() => OnAttachChanged(attached));
            _menu.Show();
            _debugPanel.Visibility = Visibility.Collapsed;
            if (_debugEnabled) _debugPanel.Show();
            else _debugPanel.Hide();
            OnAttachChanged(false);

            StartGameWatchThread();
            StartMemReadThread();
            StartMemWriteThread();
            StartDebugThread();
            StartRenderLoop();
        }

        private void NavigateBack()
        {
            _menu.Back();
            if (!_menu.IsShowing) _debugPanel.Hide();
        }

        private void ToggleMenu()
        {
            _menu.Toggle();
            if (_menu.IsShowing)
            {
                if (_debugEnabled) _debugPanel.Show();
                else _debugPanel.Hide();
            }
            else
            {
                _debugPanel.Hide();
            }
        }

        private void BuildMenuStructure()
        {
            var fovColors = new[] { "White", "Cyan", "Green", "Red" };
            var items = new List<UI.MenuItem>
            {
                new UI.MenuItem { Label = "Player Options", Type = MenuItemType.Submenu, SubItems = new List<UI.MenuItem>
                {
                    new UI.MenuItem { Label = "God Mode",         Type = MenuItemType.Toggle,
                        GetToggle = () => TrainerState.GodModeEnabled,        SetToggle = v => { TrainerState.GodModeEnabled = v; NotificationManager.ShowToggle("God Mode", v); } },
                    new UI.MenuItem { Label = "Infinite Stamina", Type = MenuItemType.Toggle,
                        GetToggle = () => TrainerState.InfiniteStaminaEnabled, SetToggle = v => { TrainerState.InfiniteStaminaEnabled = v; NotificationManager.ShowToggle("Infinite Stamina", v); } },
                    new UI.MenuItem { Label = "Grab Immunity (Allways Dodge)", Type = MenuItemType.Toggle,
                        GetToggle = () => TrainerState.AntiFuckModeEnabled,
                        SetToggle = v => { TrainerState.AntiFuckModeEnabled = v; NotificationManager.ShowToggle("Anti-Fuck Mode", v); } },
                    new UI.MenuItem { Label = "Speed Multiplier", Type = MenuItemType.Slider,
                        SliderMin = 0.1f, SliderMax = 25.0f, SliderStep = 0.1f, SliderFormat = "0.0x",
                        GetSlider = () => TrainerState.SpeedMultiplier,        SetSlider = v => TrainerState.SpeedMultiplier = v }
                }},
                new UI.MenuItem { Label = "Weapon Options", Type = MenuItemType.Submenu, SubItems = new List<UI.MenuItem>
                {
                    new UI.MenuItem { Label = "No Recoil", Type = MenuItemType.Toggle,
                        GetToggle = () => TrainerState.NoRecoilEnabled, SetToggle = v => { TrainerState.NoRecoilEnabled = v; NotificationManager.ShowToggle("No Recoil", v); } },
                    new UI.MenuItem { Label = "No Spread", Type = MenuItemType.Toggle,
                        GetToggle = () => TrainerState.NoSpreadEnabled, SetToggle = v => { TrainerState.NoSpreadEnabled = v; NotificationManager.ShowToggle("No Spread", v); } },
                    new UI.MenuItem { Label = "Infinite Ammo", Type = MenuItemType.Toggle,
                        GetToggle = () => TrainerState.InfiniteAmmoEnabled,   SetToggle = v => { TrainerState.InfiniteAmmoEnabled = v; NotificationManager.ShowToggle("Infinite Ammo", v); } },
                    
                    new UI.MenuItem { Label = "Add Pistol Ammo", Type = MenuItemType.Submenu, SubItems = new List<UI.MenuItem>
                    {
                        new UI.MenuItem { Label = "Requires ammo in inv, reload weapon after adding!", Type = MenuItemType.Action, Tag = "WARNING_LABEL" },
                        new UI.MenuItem { Label = "Amount to Add", Type = MenuItemType.Slider, SliderMin = 1f, SliderMax = 100f, SliderStep = 1f, SliderFormat = "+{0:0}",
                            GetSlider = () => _addPistolAmmoAmt, SetSlider = v => _addPistolAmmoAmt = v },
                        new UI.MenuItem { Label = "Add Ammo", Type = MenuItemType.Action, OnActivate = () => _cheat.AddItem("Pistol Ammo", (int)_addPistolAmmoAmt) }
                    }},

                    new UI.MenuItem { Label = "Add Rifle Ammo", Type = MenuItemType.Submenu, SubItems = new List<UI.MenuItem>
                    {
                        new UI.MenuItem { Label = "Requires ammo in inv, reload weapon after adding!", Type = MenuItemType.Action, Tag = "WARNING_LABEL" },
                        new UI.MenuItem { Label = "Amount to Add", Type = MenuItemType.Slider, SliderMin = 1f, SliderMax = 100f, SliderStep = 1f, SliderFormat = "+{0:0}",
                            GetSlider = () => _addRifleAmmoAmt, SetSlider = v => _addRifleAmmoAmt = v },
                        new UI.MenuItem { Label = "Add Ammo", Type = MenuItemType.Action, OnActivate = () => _cheat.AddItem("Rifle Ammo", (int)_addRifleAmmoAmt) }
                    }},

                    new UI.MenuItem { Label = "Add Shotgun Ammo", Type = MenuItemType.Submenu, SubItems = new List<UI.MenuItem>
                    {
                        new UI.MenuItem { Label = "Requires ammo in inv, reload weapon after adding!", Type = MenuItemType.Action, Tag = "WARNING_LABEL" },
                        new UI.MenuItem { Label = "Amount to Add", Type = MenuItemType.Slider, SliderMin = 1f, SliderMax = 100f, SliderStep = 1f, SliderFormat = "+{0:0}",
                            GetSlider = () => _addShotgunAmmoAmt, SetSlider = v => _addShotgunAmmoAmt = v },
                        new UI.MenuItem { Label = "Add Ammo", Type = MenuItemType.Action, OnActivate = () => _cheat.AddItem("Shotgun Ammo", (int)_addShotgunAmmoAmt) }
                    }}
                }},
                new UI.MenuItem { Label = "World Options", Type = MenuItemType.Submenu, SubItems = new List<UI.MenuItem>
                {
                    // World Time: 0.0–24.0 displayed nicely as 24h clock HH:MM
                    new UI.MenuItem { Label = "World Time", Type = MenuItemType.Slider,
                        SliderMin = 0.0f, SliderMax = 24.0f, SliderStep = 0.5f,
                        SliderFormat = "{0:00:00}",
                        GetSlider = () => TrainerState.CachedWorldTime,
                        SetSlider = v =>
                        {
                            _cheat.SetWorldTime(v);
                            int hh = (int)v;
                            int mm = (int)((v - hh) * 60);
                            NotificationManager.ShowInfo($"World Time: {hh:D2}:{mm:D2}");
                        }
                    },
                    // Freeze Time toggle
                    new UI.MenuItem { Label = "Freeze Time", Type = MenuItemType.Toggle,
                        GetToggle = () => TrainerState.FreezeTimeEnabled,
                        SetToggle = v => { _cheat.SetFreezeTime(v); NotificationManager.ShowToggle("Freeze Time", v); }
                    },
                }},
                new UI.MenuItem { Label = "Economy", Type = MenuItemType.Submenu, SubItems = new List<UI.MenuItem>
                {
                    BuildInventoryMenu(),
                    new UI.MenuItem { Label = "Requires money in inventory to add more", Type = MenuItemType.Action, Tag = "WARNING_LABEL" },
                    new UI.MenuItem { Label = "Add $1000",           Type = MenuItemType.Action, OnActivate = () => _cheat.AddMoney(1000)    },
                    new UI.MenuItem { Label = "Add $5000",           Type = MenuItemType.Action, OnActivate = () => _cheat.AddMoney(5000)    },
                    new UI.MenuItem { Label = "Add $25000",          Type = MenuItemType.Action, OnActivate = () => _cheat.AddMoney(25000)   }
                }},
                new UI.MenuItem { Label = "Visuals", Type = MenuItemType.Submenu,
                    SubItems = MenuThemes.All.ConvertAll(t => new UI.MenuItem { Label = t.Name, Type = MenuItemType.Action,
                        OnActivate = () => Dispatcher.BeginInvoke(() => { _menu.ApplyTheme(t); _debugPanel.ApplyTheme(t); NotificationManager.ApplyTheme(t); NotificationManager.ShowInfo($"Theme applied: {t.Name}"); }) })
                }
            };

            if (_showDebugMenu)
            {
                items.Add(new UI.MenuItem { Label = "Debug", Type = MenuItemType.Submenu, SubItems = new List<UI.MenuItem>
                {
                    new UI.MenuItem { Label = "Toggling the panel may cause lag & stutter!", Type = MenuItemType.Action, Tag = "WARNING_LABEL" },
                    new UI.MenuItem { Label = "Toggle Debug Panel", Type = MenuItemType.Toggle,
                        GetToggle = () => _debugEnabled,
                        SetToggle = v => { _debugEnabled = v; Dispatcher.BeginInvoke(() => { if (v) _debugPanel.Show(); else _debugPanel.Hide(); }); }}
                }});
            }

            items.Add(new UI.MenuItem { Label = "Misc", Type = MenuItemType.Submenu, SubItems = new List<UI.MenuItem>
            {
                new UI.MenuItem { Label = "Debug Options", Type = MenuItemType.Toggle,
                    GetToggle = () => _showDebugMenu,
                    SetToggle = v => {
                        _showDebugMenu = v;
                        if (!v)
                        {
                            _debugEnabled = false;
                            Dispatcher.BeginInvoke(() => _debugPanel.Hide());
                        }
                        BuildMenuStructure();
                        NotificationManager.ShowToggle("Debug Options", v);
                    }},
                new UI.MenuItem { Label = "Exit Trainer",   Type = MenuItemType.Action, Tag = "EXIT_CONFIRM",
                    OnActivate = () => _menu.ShowExitConfirm(() => Application.Current.Dispatcher.BeginInvoke(new Action(Application.Current.Shutdown))) }
            }});
            
            if (!TrainerState.IsAttached)
            {
                items.Add(new UI.MenuItem { Label = "Attach to Game", Type = MenuItemType.Action, OnActivate = DoAttach });
            }

            _menu.SetMenuItems(items, "TML TRAINER", "Too Much Light v0.7a");
        }

        private void DoAttach()
        {
            ThreadPool.QueueUserWorkItem(_ => { bool ok = _cheat.TryAttach(); Dispatcher.BeginInvoke(() => OnAttachChanged(ok)); });
        }

        private void OnAttachChanged(bool attached)
        {
            if (attached)
            {
                _menu.SetStatusLine("ATTACHED  |  INS = Toggle Menu", attached: true);
                BuildMenuStructure();
            }
            else
            {
                _menu.SetStatusLine("Waiting for TooMuchLight.exe" + Dots[_dotCycle % 3], attached: false);
                BuildMenuStructure();
            }
            _gameHwnd = IntPtr.Zero;
        }

        // Holds the top-level inventory menu item so Refresh can rebuild SubItems in-place
        private UI.MenuItem? _inventoryMenu;

        // ── Thread 0: Game Watcher ──────────────────────────────────────────────────
        private UI.MenuItem BuildInventoryMenu()
        {
            _inventoryMenu = new UI.MenuItem
            {
                Label    = "Inventory Editor",
                Type     = MenuItemType.Submenu,
                SubItems = new List<UI.MenuItem>()
            };
            RefreshInventoryMenu();
            return _inventoryMenu;
        }

        private void RefreshInventoryMenu()
        {
            if (_inventoryMenu == null) return;
            var invSubItems = new List<UI.MenuItem>();

            // ── Refresh action always at top ────────────────────────────────────
            invSubItems.Add(new UI.MenuItem
            {
                Label = "[ Refresh Inventory ]",
                Type  = MenuItemType.Action,
                OnActivate = () =>
                {
                    RefreshInventoryMenu();
                    NotificationManager.ShowInfo("Inventory refreshed from memory");
                }
            });

            // ── Live slots from memory ──────────────────────────────────────────
            var slotsDetails = _cheat.GetInventorySlotDetails(24);
            if (slotsDetails.Count == 0)
            {
                invSubItems.Add(new UI.MenuItem
                {
                    Label = TrainerState.IsAttached ? "[ No Inventory Slots Found ]" : "[ Attach to Game First ]",
                    Type  = MenuItemType.Action
                });
            }
            else
            {
                foreach (var slot in slotsDetails)
                {
                    IntPtr targetSlotPtr = slot.SlotPtr;
                    invSubItems.Add(new UI.MenuItem
                    {
                        Label = $"Slot {slot.SlotIndex}: {slot.ItemName} (x{slot.Amount})",
                        Type  = MenuItemType.Submenu,
                        SubItems = new List<UI.MenuItem>
                        {
                            new UI.MenuItem
                            {
                                Label        = "Item Count",
                                Type         = MenuItemType.Slider,
                                SliderMin    = 0.0f,
                                SliderMax    = 999.0f,
                                SliderStep   = 1.0f,
                                SliderFormat = "Count: {0:0}",
                                GetSlider    = () => _cheat.GetSlotAmount(targetSlotPtr),
                                SetSlider    = v  => _cheat.SetSlotAmount(targetSlotPtr, (int)v)
                            },
                            new UI.MenuItem
                            {
                                Label      = "Apply Count",
                                Type       = MenuItemType.Action,
                                OnActivate = () => _cheat.SetSlotAmount(targetSlotPtr, _cheat.GetSlotAmount(targetSlotPtr))
                            },
                            new UI.MenuItem { Label = "Add +10",        Type = MenuItemType.Action, OnActivate = () => _cheat.SetSlotAmount(targetSlotPtr, _cheat.GetSlotAmount(targetSlotPtr) + 10)  },
                            new UI.MenuItem { Label = "Add +50",        Type = MenuItemType.Action, OnActivate = () => _cheat.SetSlotAmount(targetSlotPtr, _cheat.GetSlotAmount(targetSlotPtr) + 50)  },
                            new UI.MenuItem { Label = "Add +100",       Type = MenuItemType.Action, OnActivate = () => _cheat.SetSlotAmount(targetSlotPtr, _cheat.GetSlotAmount(targetSlotPtr) + 100) },
                            new UI.MenuItem { Label = "Set to 99",      Type = MenuItemType.Action, OnActivate = () => _cheat.SetSlotAmount(targetSlotPtr, 99)  },
                            new UI.MenuItem { Label = "Set to 999",     Type = MenuItemType.Action, OnActivate = () => _cheat.SetSlotAmount(targetSlotPtr, 999) },
                            new UI.MenuItem { Label = "Clear Slot (0)", Type = MenuItemType.Action, OnActivate = () => _cheat.SetSlotAmount(targetSlotPtr, 0)   }
                        }
                    });
                }
            }

            // ── Add Item from complete game item list ───────────────────────────
            // invSubItems.Add(BuildAddItemMenu());

            _inventoryMenu.SubItems = invSubItems;
        }

        private UI.MenuItem BuildAddItemMenu()
        {
            var gameItems = new (string Label, string Name, int Amount)[]
            {
                // Currency
                ("Cash ($500)",                   "Cash",                     500),
                ("Cash ($1,000)",                 "Cash",                    1000),
                ("Cash ($5,000)",                 "Cash",                    5000),
                ("Cash ($25,000)",                "Cash",                   25000),
                // Consumables
                ("Wet Wipe (x10)",                "Wet Wipe",                  10),
                ("Restorative Herb (x5)",         "Restorative Herb",           5),
                ("Green Herb (x5)",               "Green Herb",                 5),
                ("Red Herb (x5)",                 "Red Herb",                   5),
                ("Lavender (x5)",                 "Lavender",                   5),
                ("Healing Mixture (x5)",          "Healing Mixture",            5),
                ("Calming Mixture (x5)",          "Calming Mixture",            5),
                ("Bottle Of Water (x5)",          "Bottle Of Water",            5),
                ("Dog Food (x5)",                 "Dog Food",                   5),
                // Canned food
                ("Canned Beans (x5)",             "Canned Beans",               5),
                ("Canned Berries (x5)",           "Canned Berries",             5),
                ("Canned Bird (x5)",              "Canned Bird",                5),
                ("Canned Fish (x5)",              "Canned Fish",                5),
                ("Canned Fruits (x5)",            "Canned Fruits",              5),
                ("Canned Juice (x5)",             "Canned Juice",               5),
                ("Canned Meat (x5)",              "Canned Meat",                5),
                ("Canned Mushrooms (x5)",         "Canned Mushrooms",           5),
                ("Canned Pate (x5)",              "Canned Pate",                5),
                ("Canned Vegetables (x5)",        "Canned Vegetables",          5),
                // Crafting
                ("Scrap Metal (x20)",             "Scrap Metal",               20),
                ("Mutated DNA (x5)",              "Mutated DNA",                5),
                ("Synthesised Virus Enzyme (x5)", "Synthesised Virus Enzyme",   5),
                ("Cloth (x10)",                   "Cloth",                     10),
                ("Elite Fabric (x10)",            "Elite Fabric",              10),
                ("Leather (x10)",                 "Leather",                   10),
                ("Jewels (x10)",                  "Jewels",                    10),
                ("Clothes Dye (x5)",              "Clothes Dye",                5),
                // Ammo & grenades
                ("Pistol Ammo (x30)",             "Pistol Ammo",               30),
                ("Rifle Ammo (x30)",              "Rifle Ammo",                30),
                ("Shotgun Ammo (x20)",            "Shotgun Ammo",              20),
                ("HE Grenade (x5)",               "HE Grenade",                 5),
                ("Flashbang (x5)",                "Flashbang",                  5),
                // Weapons
                ("Glock Pistol",                  "Glock",                      1),
                ("SCAR Rifle",                    "SCAR",                       1),
                ("Shotgun",                       "Shotgun",                    1),
                ("AK-74M Rifle",                  "AK74M",                      1),
                ("Revolver",                      "Revolver",                   1),
                // Keys & wallets
                ("Wallet",                        "Wallet",                     1),
                ("Large Wallet",                  "Large Wallet",               1),
                ("Underground Key",               "Underground Key",            1),
                ("Village Pass",                  "Village Pass",               1),
            };

            var subItems = new List<UI.MenuItem>();
            foreach (var (lbl, name, amt) in gameItems)
            {
                string capName = name;
                int    capAmt  = amt;
                subItems.Add(new UI.MenuItem
                {
                    Label      = lbl,
                    Type       = MenuItemType.Action,
                    OnActivate = () => _cheat.AddItem(capName, capAmt)
                });
            }

            return new UI.MenuItem
            {
                Label    = "Add Item from Game List",
                Type     = MenuItemType.Submenu,
                SubItems = subItems
            };
        }

        private UI.MenuItem BuildProgressionMenu()
        {
            var snap = TrainerState.ReadStats();
            int curLevel = snap?.Level ?? 2;
            int curRep   = snap?.RepLv ?? 1;
            float curAr  = snap?.Arousal ?? 64f;

            return new UI.MenuItem
            {
                Label = "Character Level & Progression",
                Type  = MenuItemType.Submenu,
                SubItems = new List<UI.MenuItem>
                {
                    new UI.MenuItem
                    {
                        Label = $"Level (Current: {curLevel})",
                        Type  = MenuItemType.Submenu,
                        SubItems = new List<UI.MenuItem>
                        {
                            new UI.MenuItem { Label = "Set Level 1",  Type = MenuItemType.Action, OnActivate = () => _cheat.SetLevel(1) },
                            new UI.MenuItem { Label = "Set Level 2",  Type = MenuItemType.Action, OnActivate = () => _cheat.SetLevel(2) },
                            new UI.MenuItem { Label = "Set Level 5",  Type = MenuItemType.Action, OnActivate = () => _cheat.SetLevel(5) },
                            new UI.MenuItem { Label = "Set Level 10", Type = MenuItemType.Action, OnActivate = () => _cheat.SetLevel(10) },
                            new UI.MenuItem { Label = "Set Level 20", Type = MenuItemType.Action, OnActivate = () => _cheat.SetLevel(20) }
                        }
                    },
                    new UI.MenuItem
                    {
                        Label = $"Reputation (Current: {curRep})",
                        Type  = MenuItemType.Submenu,
                        SubItems = new List<UI.MenuItem>
                        {
                            new UI.MenuItem { Label = "Set Rep Level 0",  Type = MenuItemType.Action, OnActivate = () => _cheat.SetReputationLevel(0) },
                            new UI.MenuItem { Label = "Set Rep Level 1",  Type = MenuItemType.Action, OnActivate = () => _cheat.SetReputationLevel(1) },
                            new UI.MenuItem { Label = "Set Rep Level 2",  Type = MenuItemType.Action, OnActivate = () => _cheat.SetReputationLevel(2) },
                            new UI.MenuItem { Label = "Set Rep Level 5",  Type = MenuItemType.Action, OnActivate = () => _cheat.SetReputationLevel(5) }
                        }
                    },
                    new UI.MenuItem
                    {
                        Label = $"Arousal (Current: {curAr:0})",
                        Type  = MenuItemType.Submenu,
                        SubItems = new List<UI.MenuItem>
                        {
                            new UI.MenuItem { Label = "Set Arousal 0",   Type = MenuItemType.Action, OnActivate = () => _cheat.SetArousal(0) },
                            new UI.MenuItem { Label = "Set Arousal 30",  Type = MenuItemType.Action, OnActivate = () => _cheat.SetArousal(30) },
                            new UI.MenuItem { Label = "Set Arousal 64",  Type = MenuItemType.Action, OnActivate = () => _cheat.SetArousal(64) },
                            new UI.MenuItem { Label = "Set Arousal 100", Type = MenuItemType.Action, OnActivate = () => _cheat.SetArousal(100) }
                        }
                    }
                }
            };
        }

        private void StartGameWatchThread()
        {
            _gameWatchThread = new Thread(() =>
            {
                while (_running)
                {
                    try
                    {
                        bool wasAttached = TrainerState.IsAttached;
                        bool gameRunning = System.Diagnostics.Process.GetProcessesByName("TooMuchLight").Length > 0;
                        if (gameRunning && !wasAttached)
                        {
                            bool ok = _cheat.TryAttach();
                            Dispatcher.BeginInvoke(() => OnAttachChanged(ok));
                        }
                        else if (!gameRunning && wasAttached)
                        {
                            _cheat.Detach(); _gameHwnd = IntPtr.Zero;
                            Dispatcher.BeginInvoke(() => OnAttachChanged(false));
                        }
                        else if (!gameRunning)
                        {
                            _dotCycle++;
                            Dispatcher.BeginInvoke(() => _menu.SetStatusLine("Waiting for TooMuchLight.exe" + Dots[_dotCycle % 3], attached: false));
                        }
                        SyncWithGameWindow();
                    }
                    catch { }
                    Thread.Sleep(10);
                }
            }) { IsBackground = true, Name = "GameWatcher", Priority = ThreadPriority.BelowNormal };
            _gameWatchThread.Start();
        }

        // ── Thread 1: MemRead ───────────────────────────────────────────────────────
        private void StartMemReadThread()
        {
            _memReadThread = new Thread(() =>
            {
                while (_running)
                {
                    if (TrainerState.IsAttached)
                    {
                        try
                        {
                            _cheat.RefreshPointers();

                            var snap = _cheat.ReadStatsFromMemory();
                            TrainerState.WriteStats(snap);

                            string hud = snap.MaxHp > 0
                                ? $"HP {snap.Hp:0}/{snap.MaxHp:0}  STA {snap.Stamina:0.0}/{snap.MaxStamina:0.0}  LVL {snap.Level}  INS=Menu"
                                : "Resolving pointers\u2026  INS=Menu";
                            TrainerState.WriteHudText(hud);
                        }
                        catch { }
                    }
                    else
                    {
                        TrainerState.WriteHudText("Waiting for TooMuchLight.exe  INS=Menu");
                    }
                    Thread.Sleep(16);
                }
            }) { IsBackground = true, Name = "MemReader" };
            _memReadThread.Start();
        }

        // ── Thread 2: MemWrite ──────────────────────────────────────────────────────
        private void StartMemWriteThread()
        {
            _memWriteThread = new Thread(() =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                long lastWriteMs = 0;
                while (_running)
                {
                    if (TrainerState.IsAttached)
                    {
                        try
                        {
                            Action? action;
                            while ((action = TrainerState.DrainAction()) != null)
                            {
                                action.Invoke();
                            }
                            long now = sw.ElapsedMilliseconds;
                            if (now - lastWriteMs >= 36) // 16.6ms frame time + 20ms buffer = ~36ms
                            {
                                _cheat.ApplyAllCheats();
                                lastWriteMs = now;
                            }
                        }
                        catch (Exception ex)
                        {
                            TrainerLog.Error($"[MemWriter] Exception executing action/cheats: {ex}");
                        }
                    }
                    Thread.Sleep(2); // 2ms sleep for CPU cooling
                }
            }) { IsBackground = true, Name = "MemWriter" };
            _memWriteThread.Start();
        }

        // ── Thread 3: Debug Panel ───────────────────────────────────────────────────
        private void StartDebugThread()
        {
            _debugThread = new Thread(() =>
            {
                while (_running)
                {
                    if (_debugEnabled && _menu.IsShowing && _debugPanel.IsShowing)
                    {
                        Dispatcher.BeginInvoke(new Action(() => _debugPanel.Refresh()), System.Windows.Threading.DispatcherPriority.Background);
                    }
                    Thread.Sleep(300);
                }
            }) { IsBackground = true, Name = "DebugRefresh", Priority = ThreadPriority.Lowest };
            _debugThread.Start();
        }

        private void StartRenderLoop()
        {
            CompositionTarget.Rendering += OnRender;
        }

        private static string GetKeyName(int vk)
        {
            return vk switch
            {
                0x01 => "LButton",
                0x02 => "RButton",
                0x04 => "MButton",
                0x05 => "XButton1",
                0x06 => "XButton2",
                0x08 => "Backspace",
                0x09 => "Tab",
                0x0D => "Enter",
                0x10 => "Shift",
                0x11 => "Ctrl",
                0x12 => "Alt",
                0x14 => "CapsLock",
                0x1B => "Escape",
                0x20 => "Space",
                0x21 => "PageUp",
                0x22 => "PageDown",
                0x23 => "End",
                0x24 => "Home",
                0x25 => "Left",
                0x26 => "Up",
                0x27 => "Right",
                0x28 => "Down",
                0x2D => "Insert",
                0x2E => "Delete",
                >= 0x30 and <= 0x39 => ((char)vk).ToString(),
                >= 0x41 and <= 0x5A => ((char)vk).ToString(),
                >= 0x60 and <= 0x69 => $"Numpad{vk - 0x60}",
                >= 0x70 and <= 0x7B => $"F{vk - 0x6F}",
                0xA0 => "LShift", 0xA1 => "RShift",
                0xA2 => "LCtrl",  0xA3 => "RCtrl",
                0xA4 => "LAlt",   0xA5 => "RAlt",
                _ => $"VK_0x{vk:X2}"
            };
        }

        private void OnRender(object? sender, EventArgs e)
        {
            SyncWithGameWindow();
            TrainerState.ScreenWidth = (float)Width;
            TrainerState.ScreenHeight = (float)Height;
        }

        private void SyncWithGameWindow()
        {
            if (_gameHwnd == IntPtr.Zero) _gameHwnd = FindWindow(null, "TooMuchLight");
            if (_gameHwnd == IntPtr.Zero) return;
            if (GetWindowRect(_gameHwnd, out RECT r) && r.R > r.L && r.B > r.T)
                Dispatcher.BeginInvoke(() => { Left = r.L; Top = r.T; Width = r.R - r.L; Height = r.B - r.T; });
        }

        private void SpawnParticles()
        {
            // Disabled continuous particle animations to preserve 100% CPU/GPU performance on layered window
        }

        protected override void OnClosed(EventArgs e)
        {
            _running = false;
            _input.Dispose();
            _cheat.Dispose();
            base.OnClosed(e);
        }
    }
}
