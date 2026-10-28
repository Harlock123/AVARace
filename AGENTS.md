# AGENTS.md

Notes for anyone, human or AI agent, changing this code. `README.md` covers playing the game, its controls and
downloads; this file covers how the code is put together and what to watch when changing it.

## What this is

A clone of the 1981 vector arcade game Omega Race, in C# on .NET 9 and Avalonia 11, with green wireframe graphics
drawn in code and sound effects synthesised at start-up. One project, no tests.

## Commands

```bash
dotnet build AVARace.sln                 # needs the .NET 9 SDK; there is no global.json
dotnet run --project src/AVARace         # play it (no command-line arguments)
```

There are no tests yet. Warnings are not treated as errors.

## Layout

- `src/AVARace/Program.cs`, `App.axaml.cs`: start-up and dependency injection.
  `Services/ServiceCollectionExtensions.cs` registers the input handler, sound service, engine and renderer as
  singletons.
- `src/AVARace/Game/`:
  - `GameEngine.cs` owns the player, enemies, bullets and explosions, spawns waves and applies scoring and lives.
  - `GameState.cs` holds score, lives, wave and the paused and game-over flags (an `ObservableObject`).
  - `GameRenderer.cs` draws the arena, entities and HUD with an Avalonia `DrawingContext`.
  - `Entities/`: `Arena` (outer wall and central obstacle), `PlayerShip`, `Enemy`, `Bullet`, `Explosion`, all
    derived from `Entity`.
  - `Physics/CollisionDetection.cs`: hit tests and wall bounces.
  - `Input/InputHandler.cs`: a set of held `Key`s, read as rotate, thrust and fire; P, R and Escape raise events.
- `src/AVARace/Services/SoundService.cs`: writes three WAV files into a temp folder (`AVARace_Sounds`) and plays them
  with NetCoreAudio, which uses `aplay` on Linux.
- `src/AVARace/Views/`: `GameCanvas` runs a 60 Hz `DispatcherTimer`; `MainWindow` forwards keys and saves Ctrl+S
  screenshots to `Pictures/AVARace`.

## Rules that keep it working

- **Variable time step.** The engine is advanced with the real elapsed time each tick (capped at 0.1 s), not in fixed
  steps. Movement and timers must scale by `deltaTime`.
- **Unseeded randomness.** `GameEngine` uses its own `new Random()`, so games are not reproducible. Sound synthesis
  uses fixed seeds so the effects are always the same.
- **Engine and drawing stay apart.** Gameplay goes in `Game/`; `GameRenderer` only reads state and draws.
- **Input goes through `InputHandler`.** Add new keys there and expose them as properties or events, rather than
  reading keys in the engine.
- **Nothing is saved.** There are no high scores or settings files; only screenshots are written.

## Making a change

- Build and play the change, since there are no tests.
- Update the controls and gameplay tables in `README.md` to match.

## CI and releases

`.github/workflows/build.yml` builds (no tests) on Ubuntu for every push and pull request, then publishes
self-contained single-file builds for win-x64/x86/arm64, linux-x64/arm64/arm and osx-x64/arm64 as workflow
artifacts. The default branch here is `master`: pushes to it update the `latest` pre-release, and a `v*` tag makes a
proper release. A release also asks the portfolio site to rebuild.
