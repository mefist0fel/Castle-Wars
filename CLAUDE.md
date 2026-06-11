# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Castle Wars** — multiplayer strategy prototype, v0.0.1. Targets Android, iOS, and Win32 desktop.

Two separate sub-projects in the same repo:
- `Castle Wars Project/` — Unity 6 client (editor version `6000.3.5f2`)
- `Castle Wars Server/` — .NET 8 server (C#, `Castle Wars Server.csproj`)

## Server Commands

```powershell
cd "Castle Wars Server"

dotnet build                        # debug build
dotnet run                          # run locally
dotnet publish -c Release           # release build (output in bin/Release/)
```

## Unity Client

The client must be opened and run through the **Unity Editor** (version 6000.3.5f2 or higher). There is no standalone CLI build script yet.

- Open project path: `Castle Wars Project/`
- Main scene: `Assets/Scenes/Game.unity`
- Render pipeline: **URP** (`com.unity.render-pipelines.universal` 17.3.0)
- Input: **Input System** (`com.unity.inputsystem` 1.17.0)
- UI: **uGUI** (`com.unity.ugui` 2.0.0)
- Tests: `com.unity.test-framework` 1.6.0 — run via Unity Editor → Window → General → Test Runner

## Architecture

### Client–Server split

- **Client** (`Castle Wars Project/`) — Unity 6, URP. Handles rendering, input, game UI. PC/Mobile renderer assets in `Assets/Settings/`.
- **Server** (`Castle Wars Server/`) — .NET 8 console, authoritative simulation. Server Authority: all game logic runs server-side.

### SharedCode — the shared logic layer

`Castle Wars Project/Assets/SharedCode/` contains pure C# with **no UnityEngine references**. The server `.csproj` compiles these files directly via `<Compile Include="..\Castle Wars Project\Assets\SharedCode\**\*.cs" />` — no DLL, no desync.

Key namespaces:
- `CastleWars.Shared.Entities` — all game entities inherit `BaseEntity` (`Id: ulong`, `Version: uint`)
- `CastleWars.Shared.World` — `WorldState`: flat `Dictionary<ulong, BaseEntity>` registry, links between entities are IDs only (no circular refs)
- `CastleWars.Shared.Protocol` — `ILogicCommand` (gameplay actions), `ISystemCommand` (subscription/LOD)

**Rules for SharedCode:**
- No `UnityEngine.*` types. Use `int`/`long` for coordinates and values instead of `float`/`Vector3`.
- Fixed-point math: values like `MovementProgress` are integers scaled ×1000 (0–1000 = 0.0–1.0).
- Unity-only code in shared files must be wrapped in `#if UNITY_5_3_OR_NEWER`.

### Unity visualization (prototype only)

`Assets/Scripts/Bootstrap/LocalSimulation.cs` — MonoBehaviour that builds a test `WorldState` (5×5 grid, 2 factions, cities, armies) and drives a local tick loop. Attach to a GameObject in the scene and wire `MapVisualizer` in the Inspector.

`Assets/Scripts/Visualization/` — `MapVisualizer` spawns Unity primitives (Plane = region, Cube = city, Sphere = army) and updates army positions each frame based on `MovementProgress`.

## Key Conventions

- Every Unity asset must have a paired `.meta` file committed alongside it.
- `Library/`, `Temp/`, `UserSettings/`, and `bin/`/`obj/` are gitignored — do not commit them.
- Server `.sln` and `.csproj` **are tracked** (unlike the Unity-generated ones, which are ignored).
- Preferred IDEs: Rider or Visual Studio (both configured in `Packages/manifest.json`).
- **Do not run any git commands** (commit, add, rm, push, reset, etc.) unless explicitly asked to do so.
