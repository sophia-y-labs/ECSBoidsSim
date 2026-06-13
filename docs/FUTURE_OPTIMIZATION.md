# Future Optimization

Tracked improvements for ECSBoidsSim. Items are grouped by **Visual** and **Performance**, ordered by priority within each group.

Current baseline: ~200 boids, URP High Fidelity, O(n²) flocking, hybrid ECS + Entities Graphics rendering.

---

## Progress

| Phase | Status | Delivered |
|-------|--------|-----------|
| Visual Phase 1 — Atmosphere & Material | **Done** | Underwater fog, skybox, post-processing, fish material, dual-light setup |
| Visual Phase 2 — Camera & Space | **Partial** | Bounds wireframe via `BoidBoundsAuthoring`; camera & environment pending |
| Visual Phase 3 — Motion Polish | Pending | — |
| Performance | Pending | Not needed at current scale |

### Phase 1 deliverables (2025-06)

- **Fog** — Linear fog (cool blue-green) in `BiodsScene` / `BoidsSubScene`; end distance ~28–45
- **Skybox** — `mat_SkyUnderwater` + gradient textures (`tex_SkyUnderwater*`)
- **Post-processing** — Global Volume with `vol_UnderwaterPost` (White Balance, Bloom, Vignette); camera `Render Post Processing` enabled
- **Fish material** — `mat_Fish` (URP Lit + clownfish texture) on `pre_Fish`; retired `mat_Boid`
- **Lighting** — Warm directional + cool fill light (`Fill Light`); `light_Underwater.lighting` preset

### Phase 2 partial deliverables

- **Bounds visualization** — `BoidBoundsAuthoring` + `mat_BoundsWire` + `BoundsWireframe` LineRenderer; syncs to `BoundsSize` at edit/runtime

### Not done in Phase 1 / 2

- Cinemachine orbit / follow camera
- URP TAA on camera; MSAA 4x → 2x on High Fidelity preset
- Boid cast-shadow reduction
- Sea floor, bubble particles, corner props (optional)
- Phase 3 motion polish (Slerp, banking, variants)

---

## Visual

### Phase 1 — Atmosphere & Material ✅

**Goal:** Immediate thematic improvement with scene and asset changes only.

| Item | Status | Description | Key files |
|------|--------|-------------|-----------|
| Underwater fog | ✅ | Linear fog (cool blue-green), match `BoundsSize` | `Assets/Scenes/BiodsScene/*.unity` |
| Skybox | ✅ | Deep-blue gradient sky material | `mat_SkyUnderwater.mat`, `tex_SkyUnderwater*.png` |
| Post-processing Volume | ✅ | White Balance, Bloom, Vignette; enabled on Main Camera | `vol_UnderwaterPost.asset`, `BiodsScene.unity` |
| Fish material | ✅ | URP Lit + clownfish texture on `pre_Fish`; retired `mat_Boid` | `mat_Fish.mat`, `pre_Fish.prefab` |
| Lighting | ✅ | Warm top light + cool fill light | `BiodsScene.unity`, `light_Underwater.lighting` |
| URP tuning | ⏳ | SSAO kept; MSAA 4x → 2x and TAA on camera not yet applied | `URP-HighFidelity.asset`, Main Camera |

**Acceptance:** Play mode clearly reads as an underwater fish swarm with depth and readable textures. **Met.**

---

### Phase 2 — Camera & Space (minimal Authoring) — partial

**Goal:** Make flock behavior visible without manual camera work.

| Item | Status | Description | Key files |
|------|--------|-------------|-----------|
| Dynamic camera | ⏳ | Cinemachine FreeLook (orbit) or follow flock centroid | Scene, optional package |
| Bounds visualization | ✅ | Wireframe cube sized to `BoundsSize` via LineRenderer | `BoidBoundsAuthoring.cs`, `mat_BoundsWire.mat` |
| Environment (optional) | ⏳ | Sea floor plane, corner props, slow bubble particles | Scene prefabs |

**Acceptance:** Flock stays in frame; simulation volume is visible. **Partial** — bounds visible; camera still static.

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
Visual Phase 1 ✅  →  Visual Phase 2 (partial)  →  Visual Phase 3
         ↓                      ↓
   [camera, URP tune]    [motion polish]
                              ↓
              Performance (when boid count grows)
```

1. ~~**Visual Phase 1**~~ — done.
2. **Visual Phase 2 (remaining)** — Cinemachine camera; optional environment; finish URP TAA / MSAA tune.
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
