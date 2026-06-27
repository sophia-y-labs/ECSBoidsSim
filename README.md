# ECSBoidsSim

A high-performance **underwater Boids flocking demo** built with Unity **ECS (DOTS)**. Classic separation, alignment, and cohesion rules run on Burst-compiled `ISystem` and `IJobParallelFor`, with URP atmosphere, Cinemachine orbit camera, and motion polish.

**Current scene baseline:** 2000 boids in a 15×15×15 volume (`BoidsSubScene`).

![Play mode — underwater boids flocking with auto-orbit camera](docs/media/play_demo.gif)

## Features

### Simulation

- **Classic Boids rules** — Separation, Alignment, Cohesion with configurable weights
- **Boundary avoidance** — Keeps boids within a defined volume
- **Burst + Job System** — `IJobParallelFor` for O(n²) neighbor traversal, one thread per boid
- **ECS architecture** — `ISystem`, `IAspect`, Authoring + Baker pipeline
- **Runtime spawning** — Batch instantiate boids from a prefab at play start
- **Motion polish** — Smooth `math.slerp` rotation, turn banking, per-boid scale variation (0.85–1.15)

### Visual Presentation

- **Underwater atmosphere** — Linear fog, gradient skybox, global post-processing (White Balance, Bloom, Vignette)
- **Fish rendering** — URP Lit clownfish material on `pre_Fish`
- **Dual-light setup** — Warm directional + cool fill light
- **Orbit camera** — Cinemachine FreeLook with auto-orbit (`BoidOrbitCameraDriver`, 12°/s)
- **Bounds wireframe** — LineRenderer cube synced to `BoundsSize` via `BoidBoundsAuthoring`
- **URP tuning** — High Fidelity preset with 2× MSAA and Main Camera TAA

## Tech Stack

| | |
|---|---|
| Unity | 2022.3.62f3c1 |
| Render Pipeline | URP 14 |
| ECS | Entities 1.3+ |
| Cinemachine | 2.9.7 |
| Burst | Enabled on all systems and jobs |

## Architecture

```
Assets/Scripts/
├── Aspects/       # IAspect — combined component views
├── Authoring/     # MonoBehaviour + Baker (Inspector → ECS)
├── Components/    # IComponentData — pure data structs
├── Editor/        # Editor utilities (orbit camera setup)
└── System/        # ISystem — Burst-compiled logic
```

### System Execution Order

```
BoidSpawnSystem  →  BoidSystem  →  FaceDirectionSystem
     │                   │                  │
  Instantiate         Flocking           Slerp + bank
  all boids           (S/A/C)            toward velocity
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
4. Press **Play** — the camera orbits automatically; no manual camera input needed.

### Scene Setup

The SubScene contains three authoring objects:

| GameObject | Component | Purpose |
|---|---|---|
| **Boid Spawner** | `BoidSpawnerAuthoring` | Prefab reference, spawn count, spawn radius |
| **Boid Settings** | `BoidSettingsAuthoring` | Flocking weights, perception radii, speed limits, bounds, rotation/banking |
| **Boid Prefab** | `BoidAuthoring` | Boid entity template (baked with `BoidTag`, `BoidVelocity`) |

Adjust parameters in the Inspector before entering Play mode. Settings are baked into ECS components automatically.

### Key Parameters (current scene)

Values below match `Assets/Scenes/BiodsScene/BoidsSubScene.unity`. Authoring components may define different code defaults.

| Parameter | Scene value | Description |
|---|---|---|
| `SpawnCount` | 2000 | Number of boids |
| `SpawnRadius` | 6 | Initial spawn spread |
| `BoundsSize` | (15, 15, 15) | Simulation volume |
| `SeparationWeight` | 1.5 | Push apart when too close |
| `AlignmentWeight` | 1.0 | Match neighbor heading |
| `CohesionWeight` | 1.5 | Move toward neighbor center |
| `SeparationRadius` | 1.5 | Close-range detection |
| `PerceptionRadius` | 4.5 | Neighbor detection range |
| `MaxSpeed` | 7.0 | Speed cap |
| `MaxSteerForce` | 0.6 | Steering force limit |
| `RotationSpeed` | 8 | Slerp rate toward velocity heading |
| `MaxBankAngleDegrees` | 15 | Max roll on turns (Inspector; baked as radians) |
| `BankingStrength` | 2.0 | Turn-to-roll multiplier |

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
│   ├── Art/                    # Models, textures, prefabs (pre_Fish)
│   ├── Editor/                 # BuildTools
│   ├── Materials/              # mat_Fish, mat_SkyUnderwater, mat_BoundsWire, …
│   ├── Scenes/BiodsScene/      # Main scene + SubScene
│   ├── Scripts/
│   │   ├── Aspects/            # BoidAspect
│   │   ├── Authoring/          # BoidAuthoring, BoidSpawnerAuthoring, BoidSettingsAuthoring,
│   │   │                       # BoidBoundsAuthoring, BoidOrbitCameraDriver
│   │   ├── Components/         # BoidTag, BoidVelocity, BoidSettings
│   │   ├── Editor/             # BoidOrbitCameraSetup
│   │   └── System/             # BoidSpawnSystem, BoidSystem, FaceDirectionSystem
│   └── Settings/               # URP render pipeline assets
├── Packages/manifest.json
├── ProjectSettings/
├── LICENSE                     # MIT
└── .gitignore
```

## Future Work

At 2000 boids, O(n²) flocking is the main CPU cost. Likely next steps when scaling further:

- **Spatial hash / uniform grid** — replace all-pairs neighbor search
- **Jobified writeback** — reduce main-thread `SetComponentData` in `BoidSystem`
- **Rendering** — BRG / instancing if draw calls become a bottleneck
- **Optional polish** — per-boid color tint, tail motion, reduced boid shadow casting

## License

This project is licensed under the [MIT License](LICENSE) — free to use, modify, and distribute.
