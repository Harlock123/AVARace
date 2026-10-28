# AVARace

A clone of the classic 1981 arcade game **Omega Race** built with Avalonia UI and .NET 9, featuring authentic wireframe vector graphics rendering.

## About

Omega Race was a unique arcade game featuring vector graphics where players pilot a ship in an arena, destroying enemy droids while bouncing off walls. This clone recreates that experience with:

- Classic green-on-black wireframe vector graphics
- Authentic physics with wall bouncing
- Progressive difficulty with waves of enemies
- Retro synthesized sound effects
- Full MVVM architecture

## Screenshots

![Start Screen](Images/SCREENSHOT1.png)

![Gameplay](Images/SCREENSHOT2.png)

![Explosion Effects](Images/SCREENSHOT3.png)

## Download and run

Ready-to-run builds for Windows, macOS and Linux, on both Intel/AMD and ARM, are on the
[Releases page](https://github.com/Harlock123/AVARace/releases). **Latest build** is rebuilt on
every push to `master`. Each download is one self-contained executable: no .NET install is
needed.

```bash
tar -xzf AVARace-linux-arm64.tar.gz      # or linux-x64, linux-arm, osx-arm64, osx-x64
./AVARace
```

On Windows, unzip `AVARace-win-x64.zip` (or `-win-arm64`) and run `AVARace.exe`. On macOS, if
Gatekeeper refuses to open it, run `xattr -d com.apple.quarantine AVARace` once.

### Linux: sound

- **Sound** needs `aplay`, from the `alsa-utils` package. Without it the game runs but is
  silent. Minimal installs (Arch, Omarchy, some Raspberry Pi images) don't include it:

  ```bash
  sudo apt install alsa-utils             # Debian / Ubuntu / Raspberry Pi OS
  sudo pacman -S --needed alsa-utils      # Arch / Omarchy
  ```

## Requirements

- .NET 9 SDK
- Compatible with Windows, macOS, and Linux

## Building and Running

```bash
# Build the project
dotnet build

# Run the game
dotnet run --project src/AVARace
```

## Controls

| Key | Action |
|-----|--------|
| **Left Arrow** / **A** | Rotate left |
| **Right Arrow** / **D** | Rotate right |
| **Up Arrow** / **W** | Thrust |
| **Space** | Fire |
| **P** | Pause |
| **R** | Restart (when game over) |
| **Ctrl+S** | Save screenshot |
| **Escape** | Exit |

## Gameplay

- Pilot your ship around the arena
- Destroy all enemy droids to advance to the next wave
- Avoid colliding with enemies
- Bounce off the outer walls and central obstacle
- Each wave brings more and faster enemies
- You have 3 lives - game over when all are lost

### Spawn Protection

- **Invulnerability**: When you spawn or respawn, your ship is invulnerable for 2 seconds (indicated by blinking)
- **Safe Spawning**: Enemies always spawn at a safe distance from the player, preventing unfair instant deaths

### Enemy Types

| Enemy | Points | Behavior |
|-------|--------|----------|
| **Droid** | 100 | Basic enemy, moves randomly |
| **Fast Droid** | 200 | Faster movement, appears from wave 2 |
| **Hunter** | 300 | Actively chases the player, appears from wave 3 |

## Sound Effects

The game features retro synthesized sound effects:

- **Shooting**: Descending pitch laser sound
- **Thruster**: Low rumble while thrusting
- **Explosion**: Deep bass rumble when enemies are destroyed

## Architecture

The project uses a full MVVM (Model-View-ViewModel) architecture with dependency injection:

- **Views**: Avalonia UI windows and custom canvas control
- **ViewModels**: Game state bindings and commands
- **Game Engine**: Core game loop, entity management, physics
- **Services**: DI-registered interfaces for testability
  - `IGameEngine` - Game logic and state management
  - `IInputHandler` - Keyboard input
  - `ISoundService` - Synthesized audio generation

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
