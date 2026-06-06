# ESCBoidsSim — ECS Boids Simulation

Unity 2022.3.62f3c1 · URP · Entities 1.0+ (DOTS)

A Boids flocking simulation built on Unity ECS (Entities 1.0+), using ISystem + IJobParallelFor + IAspect architecture.

---

## Architecture

```
Assets/Scripts/
├── Aspects/          # IAspect: combined component views
├── Authoring/        # MonoBehaviour + Baker (Inspector → ECS components)
├── Components/       # IComponentData: pure data structs
└── System/           # ISystem: BurstCompiled logic
```

## Code Conventions

### Naming
| Category | Rule | Example |
|------|------|------|
| Component (IComponentData) | `PascalCase` + Noun | `BoidVelocity`, `BoidSettings` |
| Tag Component | `PascalCase` + `Tag` | `BoidTag` |
| System (ISystem) | `PascalCase` + `System` | `BoidSystem`, `FaceDirectionSystem` |
| Aspect | `PascalCase` + `Aspect` | `BoidAspect` |
| Authoring | `PascalCase` + `Authoring` | `BoidAuthoring` |
| Baker | Nested inside Authoring class | `class Baker : Baker<BoidAuthoring>` |
| Job | `PascalCase` + `Job` | `BoidFlockingJob` |
| Private field | `m_PascalCase` | `m_HasSpawned`, `m_BoidQuery` |
| Namespace | `ESCBoidsSim.` + Category | `ESCBoidsSim.Systems` |

### File Structure
```
One "main type" per file +
one nested Baker (if Authoring class).
```

### Templates

#### IComponentData
```csharp
using Unity.Entities;
using Unity.Mathematics;

namespace ESCBoidsSim.Components
{
    public struct XxxComponent : IComponentData
    {
        public float Value;
    }
}
```

#### ISystem + IJobEntity
```csharp
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace ESCBoidsSim.Systems
{
    [BurstCompile]
    public partial struct XxxSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new XxxJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct XxxJob : IJobEntity
    {
        public float DeltaTime;

        [BurstCompile]
        private void Execute(ref XxxComponent c) { }
    }
}
```

#### IAspect
```csharp
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ESCBoidsSim.Aspects
{
    public readonly partial struct XxxAspect : IAspect
    {
        public readonly Entity Self;
        public readonly RefRW<LocalTransform> Transform;
        public readonly RefRW<XxxComponent> Component;
    }
}
```

#### Authoring + Baker
```csharp
using Unity.Entities;
using UnityEngine;

namespace ESCBoidsSim.Authoring
{
    public class XxxAuthoring : MonoBehaviour
    {
        public float Value;

        class Baker : Baker<XxxAuthoring>
        {
            public override void Bake(XxxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new XxxComponent { Value = authoring.Value });
            }
        }
    }
}
```

## Project Rules

### Workflow Governance
- **Before executing any implementation or plan, first present approach options / design directions to me for confirmation.** Do not proceed with code changes until I have explicitly approved the chosen approach.

### ECS Patterns
- **Use ISystem, not SystemBase** — unmanaged, Burst-friendly
- **IJobParallelFor for O(n²) computation** (e.g. Boid neighbor traversal), manually manage NativeArray lifetime
- **IJobEntity + ScheduleParallel** for per-entity operations (modern replacement for Entities.ForEach)
- **IAspect** for grouped component access, eliminates repetitive `RefRW/RefRO` declarations
- **Singleton** via `SystemAPI.GetSingleton<T>()`
- **Baker always uses `TransformUsageFlags`** — `Dynamic` (moving), `None` (data-only)
- **Authoring and Component are separate** — Authoring exists only at Bake time

### System Execution Order
- Use `[UpdateBefore(typeof(...))]` / `[UpdateAfter(typeof(...))]` to control ordering
- Current order: `BoidSpawnSystem → BoidSystem → FaceDirectionSystem`

### Job Parallelism
- BoidFlockingJob uses `IJobParallelFor` — one thread per boid
- All parallel Jobs must be `[BurstCompile]`
- Separate read/write: mark read-only NativeArrays with `[ReadOnly]`

### Resource Management
- NativeArray uses `Allocator.TempJob`, **must Dispose in same frame**
- Release immediately after manual `Complete()`, no cross-frame dependencies
- Never use `EntityManager` inside Jobs (not thread-safe)

### Comments
- Public types and fields: `/// <summary>` XML docs
- Complex logic: `// --- Section Title ---` separator blocks
- All comments in English

---

## Common Commands

```bash
# Build Windows standalone
D:\Dev\2022.3.62f3c1\Editor\Unity.exe -quit -batchmode -logFile - -projectPath "i:\ESCBoidsSim" -executeMethod ESCBoidsSim.BuildTools.PerformBuild -buildTarget windows

# Open project in Unity Editor
D:\Dev\2022.3.62f3c1\Editor\Unity.exe -projectPath "i:\ESCBoidsSim"
```
