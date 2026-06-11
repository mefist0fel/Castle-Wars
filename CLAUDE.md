# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Castle Wars** — multiplayer strategy prototype, v0.0.1. Targets Android, iOS, and Win32 desktop.

Two separate sub-projects in the same repo:
- `Castle Wars Projects/` — Unity 6 client (editor version `6000.3.5f2`)
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

- Open project path: `Castle Wars Projects/`
- Main scene: `Assets/Scenes/Game.unity`
- Render pipeline: **URP** (`com.unity.render-pipelines.universal` 17.3.0)
- Input: **Input System** (`com.unity.inputsystem` 1.17.0)
- UI: **uGUI** (`com.unity.ugui` 2.0.0)
- Tests: `com.unity.test-framework` 1.6.0 — run via Unity Editor → Window → General → Test Runner

## Architecture

The project uses a client–server split:

- **Client (Unity)** handles rendering, input, and game UI. URP is configured with separate PC and Mobile renderer/pipeline assets under `Assets/Settings/`.
- **Server (.NET 8)** will handle authoritative game logic and networking. Currently a stub (`Program.cs`).

## Key Conventions

- Every Unity asset must have a paired `.meta` file committed alongside it.
- `Library/`, `Temp/`, `UserSettings/`, and `bin/`/`obj/` are gitignored — do not commit them.
- Server `.sln` and `.csproj` **are tracked** (unlike the Unity-generated ones, which are ignored).
- Preferred IDEs: Rider or Visual Studio (both configured in `Packages/manifest.json`).
- **Do not run any git commands** (commit, add, rm, push, reset, etc.) unless explicitly asked to do so.
