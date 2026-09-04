# SMT Trainer

A trainer for the game **Samantha** (v2.11 beta) by Walters Games.

Unlike its predecessor [TML Trainer](https://github.com/eincoder420/TML-Trainer), which read and wrote
the game's memory from the outside, SMT Trainer is an **internal mod**: a small C# assembly loaded
directly into the game's Mono runtime by a purpose-built injector. That means it can call the game's
own code, so changes apply live instead of only after the game happens to refresh them.

No MelonLoader, no BepInEx — the injector is part of this project.

## Features

*   **Editor › Body**
    *   Unrestricted colour for hair, eyes, lips, eyeshadow, fingernails, pubic hair and scalp
    *   RGBA sliders that go past 1.0 for HDR/glow, which the in-game HSV wheel cannot express
    *   Hex entry, plus a saveable colour palette
    *   Every hairstyle, skin tone and pubic style selectable, bought or not
    *   Unclamped body proportions (boobs, ass, fatness, eye size)
*   **Editor › Clothing**
    *   Every clothing slot: worn state, mesh variant, colour and texture tiling
    *   Variant lists expose meshes the wardrobe never offers
    *   Wear or remove everything at once
*   **Editor › Presets**
    *   Named Body and Clothing presets, saved separately under `Documents\SMT-Trainer\`
    *   Plain text format, hand-editable and shareable
*   **Live preview** — a 3D panel beside the menu showing the actual asset being edited,
    updating as you change it
*   **Player**
    *   Walking / running / sprinting speed multiplier
    *   0 Risk Level, 0 Embarrassment, Maximal Happiness
    *   Fix Stuck Player (`F9`), with an automatic watchdog for the frozen wardrobe animation
*   **World**
    *   Time of day, freeze time, day length
    *   Weather changer driven through the game's own Azure[Sky] profiles
*   **Unlock All**
    *   All clothing variants, all hairstyles, hidden editor categories
    *   Editor tools and consumables, money
*   Seven colour themes for the menu

## Controls

| Key | Action |
| --- | --- |
| `Insert` | Open / close the menu |
| Arrows or Numpad `8` `2` `4` `6` | Navigate and adjust |
| `Enter` / Numpad `5` | Select |
| `Backspace` / Numpad `0` | Back |
| `Home` / `End` | Turn the preview |
| `PgUp` / `PgDn` | Zoom the preview |
| `Del` | Toggle preview auto-spin |
| `F9` | Fix stuck player |

## Source Structure

*   `Injector/` - The loader. A standalone .NET 6 exe.
    *   `ProcessMemory.cs` - Process attach, memory read/write, remote allocation and threads.
    *   `Mono/PeExports.cs` - Reads the export table out of the target's loaded Mono module.
    *   `Mono/CallScript.cs` - Builds one x64 shellcode blob that chains the Mono calls.
    *   `Mono/MonoInjector.cs` - The load sequence, with per-step failure reporting.
*   `Payload/` - The injected assembly, compiled against the game's own DLLs.
    *   `Features/` - Editor, clothing, player, world, unlock and preset logic.
    *   `UI/` - The NativeUI-style IMGUI menu, preview panel and toasts.
*   `Build-Trainer.ps1` - Builds the payload, then the injector that embeds it.

## Requirements

The injector is built as a framework-dependent .NET 6 application to keep the executable small.

Because of this, **you must have the .NET 6 Desktop Runtime installed** to run the `.exe`.

🔗 **[Download .NET 6.0 Desktop Runtime (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)**

## How to Build from Source

1. Clone or download this repository.
2. Open PowerShell in the root folder.
3. Run the build script:
   ```powershell
   powershell -ExecutionPolicy Bypass -File Build-Trainer.ps1
   ```
4. The final trainer will be in the `publish/` folder: `publish/SamanthaTrainer.exe`.

`-ExecutionPolicy Bypass` applies to that one invocation only and does not change any
machine setting. Without it, Windows blocks unsigned scripts by default and the build will
not start.

The payload is compiled against the game's own assemblies, which is what lets it call the game's
code directly instead of going through reflection. On the first run the script locates your game
install by itself — checking a running `Samantha.exe` first, then the usual folders — and copies
the assemblies it needs into `lib/`. Every build after that uses `lib/` and does not need the
game at all.

Those assemblies belong to Walters Games and Unity, so `lib/` is deliberately not committed to
this repository. If auto-detection cannot find your install, point at it once and it is
remembered:

```powershell
powershell -ExecutionPolicy Bypass -File Build-Trainer.ps1 -GameDir "X:\Games\Samantha_v211_beta"
```

*(The build requires the .NET 6 SDK on your development machine.)*

## How to Use

1. Launch `Samantha.exe`.
2. Launch `publish\SamanthaTrainer.exe` as Administrator (it will prompt automatically).
3. Click **Inject** once the game is detected.
4. Press **Insert** in-game to open the menu.
