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

`Castle Wars Project/Assets/SharedCode/` — assembly `CastleWars.Shared` (`noEngineReferences: true`). Compiled by both Unity and the server (`<Compile Include="..\Castle Wars Project\Assets\SharedCode\**\*.cs" />`). **No UnityEngine types allowed** — enforced at compile time by the asmdef.

Two sub-layers:

**`Core/`** — base infrastructure (reusable across games):
- `BaseEntity` — `Id: ulong` (internal set), `Version: uint` (internal set)
- `EntityRegistry` — flat `Dictionary<ulong, BaseEntity>`; fires `OnEntityRegistered` / `OnEntityMutated` events
- `GameSession` — abstract outer container; owns the registry, registers `CommandHandler<T>` generics, routes `Apply(ILogicCommand)`, proxies registry events
- `CommandHandler<TCommand>` — generic abstract base; concrete handlers registered via `RegisterHandler()`
- `ILogicCommand` / `ISystemCommand` — marker interfaces

**`Game/`** — Castle Wars specific:
- `Entities/` — `SessionEntity` (fixed Id=1, routing root), `MapEntity`, `RegionEntity`, `CityEntity`, `ArmyEntity`, `PlayerEntity`, `FactionEntity`
- `Commands/` — each file contains both the command data class and its handler: `CreateMapCommand`, `MoveArmyCommand`
- `CastleWarsSession` — concrete `GameSession`; registers the session entity first (→ Id=1), registers all handlers; exposes `Seed(entity)` for meta-data bootstrapping

**SharedCode rules:**
- No `UnityEngine.*` types. Use `int`/`long` instead of `float`/`Vector3`.
- Fixed-point math: `MovementProgress` is 0–1000 (= 0.0–1.0).
- Unity-only code in shared files must use `#if UNITY_5_3_OR_NEWER`.

### Unity visualization (prototype only)

`Assets/Scripts/` — assembly `CastleWars.Client`, references `CastleWars.Shared`.

`Visualization/EntityVisualizer` — abstract `MonoBehaviour` base; subscribes to `GameSession.OnEntityRegistered` and `OnEntityMutated` on `Bind(session)`.

Three concrete visualizers, each reacting only to its own entity type:
- `MapVisualizer` — `RegionEntity` → spawns `Plane` tiles
- `ArmyVisualizer` — `ArmyEntity` → spawns `Sphere`, lerps position by `MovementProgress`
- `CityVisualizer` — `CityEntity` → spawns `Cube`

`Bootstrap/LocalSimulation` — creates `CastleWarsSession`, binds visualizers, applies `CreateMapCommand`, seeds test data, drives tick loop (`Mutate(army)` on each tick triggers `ArmyVisualizer` update).

## Key Conventions

- Every Unity asset must have a paired `.meta` file committed alongside it.
- `Library/`, `Temp/`, `UserSettings/`, and `bin/`/`obj/` are gitignored — do not commit them.
- Server `.sln` and `.csproj` **are tracked** (unlike the Unity-generated ones, which are ignored).
- Preferred IDEs: Rider or Visual Studio (both configured in `Packages/manifest.json`).
- **Do not run any git commands** (commit, add, rm, push, reset, etc.) unless explicitly asked to do so.
