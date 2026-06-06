# ECSBoidsSim

A high-performance **Boids flocking simulation** built with Unity **ECS (DOTS)**. Implements classic separation, alignment, and cohesion rules using Burst-compiled `ISystem`, `IJobParallelFor`, and `IAspect`.

## Features

- **Classic Boids rules** — Separation, Alignment, Cohesion with configurable weights
- **Boundary avoidance** — Keeps boids within a defined volume
- **Burst + Job System** — `IJobParallelFor` for O(n²) neighbor traversal, one thread per boid
- **ECS architecture** — `ISystem`, `IAspect`, Authoring + Baker pipeline
- **Runtime spawning** — Batch instantiate boids from a prefab at play start
- **Face direction** — Boids rotate to match their velocity

## Tech Stack

| | |
|---|---|
| Unity | 2022.3.62f3c1 |
| Render Pipeline | URP 14 |
| ECS | Entities 1.3+ |
| Burst | Enabled on all systems and jobs |

## Architecture

```
Assets/Scripts/
├── Aspects/       # IAspect — combined component views
├── Authoring/     # MonoBehaviour + Baker (Inspector → ECS)
├── Components/    # IComponentData — pure data structs
└── System/        # ISystem — Burst-compiled logic
```

### System Execution Order

```
BoidSpawnSystem  →  BoidSystem  →  FaceDirectionSystem
     │                   │                  │
  Instantiate         Flocking           Rotate to
  all boids           (S/A/C)            velocity
```

### Flocking Pipeline (BoidSystem)

1. Read global `BoidSettings` singleton
2. Gather all boid positions and velocities into `NativeArray`
3. Schedule `BoidFlockingJob` (`IJobParallelFor`) — one thread per boid
4. Write updated velocities and positions back to entities
5. Dispose all temp arrays in the same frame

## Getting Started

### Requirements

- [Unity 2022.3 LTS](https://unity.com/releases/editor/whats-new/2022.3.0) (2022.3.62f3c1 or compatible)
- Windows / macOS / Linux

### Run in Editor

1. Clone the repository:
   ```bash
   git clone https://github.com/sophia-y-labs/ECSBoidsSim.git
   ```
2. Open the project in Unity Hub.
3. Open scene `Assets/Scenes/BiodsScene/BiodsScene.unity`.
4. Press **Play**.

### Scene Setup

The SubScene contains three authoring objects:

| GameObject | Component | Purpose |
|---|---|---|
| **Boid Spawner** | `BoidSpawnerAuthoring` | Prefab reference, spawn count, spawn radius |
| **Boid Settings** | `BoidSettingsAuthoring` | Flocking weights, perception radii, speed limits, bounds |
| **Boid Prefab** | `BoidAuthoring` | Boid entity template (baked with `BoidTag`, `BoidVelocity`) |

Adjust parameters in the Inspector before entering Play mode. Settings are baked into ECS components automatically.

### Key Parameters

| Parameter | Default | Description |
|---|---|---|
| `SeparationWeight` | 2.0 | Push apart when too close |
| `AlignmentWeight` | 1.0 | Match neighbor heading |
| `CohesionWeight` | 1.0 | Move toward neighbor center |
| `SeparationRadius` | 3.0 | Close-range detection |
| `PerceptionRadius` | 8.0 | Neighbor detection range |
| `MaxSpeed` | 5.0 | Speed cap |
| `BoundsSize` | (30, 30, 30) | Simulation volume |
| `SpawnCount` | 100 | Number of boids |

## Build

A headless build script is included at `Assets/Editor/BuildTools.cs`.

**Windows:**
```bash
Unity.exe -quit -batchmode -logFile - \
  -projectPath "/path/to/ECSBoidsSim" \
  -executeMethod ESCBoidsSim.BuildTools.PerformBuild \
  -buildTarget windows
```

Output: `Builds/Windows/ESCBoidsSim.exe`

**Linux:**
```bash
Unity.exe -quit -batchmode -logFile - \
  -projectPath "/path/to/ECSBoidsSim" \
  -executeMethod ESCBoidsSim.BuildTools.PerformBuild \
  -buildTarget linux
```

Output: `Builds/Linux/ESCBoidsSim.x86_64`

## Project Structure

```
ECSBoidsSim/
├── Assets/
│   ├── Art/                    # Models and prefabs
│   ├── Editor/                 # BuildTools
│   ├── Scenes/BiodsScene/      # Main scene + SubScene
│   ├── Scripts/
│   │   ├── Aspects/            # BoidAspect
│   │   ├── Authoring/          # BoidAuthoring, BoidSpawnerAuthoring, BoidSettingsAuthoring
│   │   ├── Components/         # BoidTag, BoidVelocity, BoidSettings
│   │   └── System/             # BoidSpawnSystem, BoidSystem, FaceDirectionSystem
│   └── Settings/               # URP render pipeline assets
├── Packages/manifest.json
├── ProjectSettings/
├── LICENSE                     # MIT
└── .gitignore
```

## Future Work

Visual polish and performance scaling plans are tracked in [docs/FUTURE_OPTIMIZATION.md](docs/FUTURE_OPTIMIZATION.md).

## License

This project is licensed under the [MIT License](LICENSE) — free to use, modify, and distribute.
