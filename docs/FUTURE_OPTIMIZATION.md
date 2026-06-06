# Future Optimization

Tracked improvements for ECSBoidsSim. Items are grouped by **Visual** and **Performance**, ordered by priority within each group.

Current baseline: ~200 boids, URP High Fidelity, O(n²) flocking, hybrid ECS + Entities Graphics rendering.

---

## Visual

### Phase 1 — Atmosphere & Material (no code)

**Goal:** Immediate thematic improvement with scene and asset changes only.

| Item | Description | Key files |
|------|-------------|-----------|
| Underwater fog | Enable linear fog (cool blue-green), match `BoundsSize` (~40–50 end distance) | `Assets/Scenes/BiodsScene/BiodsScene.unity` |
| Skybox | Replace default procedural sky with deep-blue gradient or cubemap | Scene lighting settings |
| Post-processing Volume | Color Adjustments, Bloom, Vignette; enable on Main Camera | New Volume Profile, scene |
| Fish material | URP Lit + `clownfish.png`, apply to `pre_Fish` prefab; retire unused `mat_Boid` | `Assets/Art/Models/pre_Fish.prefab`, `Assets/Materials/` |
| Lighting | Warm top light + weak cool fill; consider disabling boid cast shadows | Scene |
| URP tuning | Keep SSAO; MSAA 4x → 2x; enable TAA on camera | `Assets/Settings/URP-HighFidelity.asset` |

**Acceptance:** Play mode clearly reads as an underwater fish swarm with depth and readable textures.

---

### Phase 2 — Camera & Space (minimal Authoring)

**Goal:** Make flock behavior visible without manual camera work.

| Item | Description | Key files |
|------|-------------|-----------|
| Dynamic camera | Cinemachine FreeLook (orbit) or follow flock centroid | Scene, optional package |
| Bounds visualization | Semi-transparent or wireframe cube sized to `BoundsSize` | New `BoidBoundsAuthoring` (visual only) |
| Environment (optional) | Sea floor plane, corner props, slow bubble particles | Scene prefabs |

**Acceptance:** Flock stays in frame; simulation volume is visible.

---

### Phase 3 — Motion Polish (ECS code)

**Goal:** Fish feel like they swim, not slide.

| Item | Description | Key files |
|------|-------------|-----------|
| Smooth rotation | Replace instant `LookRotation` with `math.slerp` + `RotationSpeed` in settings | `FaceDirectionSystem.cs`, `BoidSettings.cs` |
| Banking | Roll on turns based on heading change | `FaceDirectionSystem.cs` |
| Variation (optional) | Per-boid scale / color tint at spawn | `BoidSpawnSystem.cs`, new `BoidVariant` component |
| Tail motion (optional) | Sin-driven shader vertex offset or simple animator | Shader or new system |

**Acceptance:** Turning has inertia; individuals are distinguishable.

---

## Performance

Relevant when scaling beyond ~500–1000 boids. Not required at current ~200 count.

### Simulation (CPU)

| Item | Current | Target | Impact |
|------|---------|--------|--------|
| Neighbor search | O(n²) all-pairs in `BoidFlockingJob` | Uniform grid / spatial hash | High — primary scaling bottleneck |
| Distance check | `math.length` per pair | `math.lengthsq` vs squared radii | Low — easy micro-optimization |
| Data gather | `ToEntityArray` + `ToComponentDataArray` every frame | Persistent buffers or `IJobEntity` direct access | Medium |
| Writeback | Main-thread `EntityManager.SetComponentData` loop | Jobified scatter or `SystemAPI` parallel write | Medium |
| Job sync | `jobHandle.Complete()` same frame | Async schedule where dependencies allow | Low–Medium |
| Face direction | Single-threaded `foreach` | `IJobEntity` + `ScheduleParallel` | Low at 200; grows with count |

### Rendering (GPU)

| Item | Current | Target | Impact |
|------|---------|--------|--------|
| Draw path | ~N individual mesh draws via Entities Graphics | BRG / GPU instancing / shared material variants | High at 1000+ |
| Shadows | All boids cast/receive | Disable caster on boids or reduce shadow distance | Medium |
| URP quality | High Fidelity (4x MSAA, 4K shadows, SSAO) | Balanced preset for perf builds | Medium |

### Spawn (one-time)

| Item | Current | Target | Impact |
|------|---------|--------|--------|
| Instantiation | `EntityManager.Instantiate` per boid in loop | ECB batch spawn or `Instantiate` burst | Low — runs once |

---

## Suggested Order

```
Visual Phase 1  →  Visual Phase 2  →  Visual Phase 3
                                              ↓
                              Performance (when boid count grows)
```

1. **Visual Phase 1** — highest visual ROI, zero simulation risk.
2. **Visual Phase 2** — camera and bounds; optional environment.
3. **Visual Phase 3** — motion polish when flocking feel is the priority.
4. **Performance** — invest when profiling shows CPU/GPU limits at target scale.

---

## Out of Scope (for now)

- Multithreaded structural changes during simulation frames
- Netcode / multiplayer flocking
- ML-driven steering behaviors

---

## Related Docs

- [README](../README.md) — project overview and getting started
