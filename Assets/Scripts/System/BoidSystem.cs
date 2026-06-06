using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Core Boid system: runs Separation / Alignment / Cohesion every frame.
/// </summary>
[BurstCompile]
public partial struct BoidSystem : ISystem
{
    EntityQuery m_BoidQuery;

    public void OnCreate(ref SystemState state)
    {
        // Query all Boids: requires LocalTransform + BoidVelocity + BoidTag
        m_BoidQuery = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, BoidVelocity, BoidTag>()
            .Build();

        state.RequireForUpdate(m_BoidQuery);
        state.RequireForUpdate<BoidSettings>();   // Must have global settings to run
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. Read global settings
        var settings = SystemAPI.GetSingleton<BoidSettings>();

        // 2. Gather all Boid positions and velocities into NativeArrays for neighbor lookup
        var boidEntities = m_BoidQuery.ToEntityArray(Allocator.TempJob);
        var boidPositions = m_BoidQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var boidVelocities = m_BoidQuery.ToComponentDataArray<BoidVelocity>(Allocator.TempJob);
        var boidCount = boidEntities.Length;

        if (boidCount == 0)
        {
            boidEntities.Dispose();
            boidPositions.Dispose();
            boidVelocities.Dispose();
            return;
        }

        // 3. Compute new velocity for each Boid, stored in a NativeArray
        var newVelocities = new NativeArray<float3>(boidCount, Allocator.TempJob);

        var job = new BoidFlockingJob
        {
            Positions = boidPositions,
            Velocities = boidVelocities,
            NewVelocities = newVelocities,
            Settings = settings,
            BoidCount = boidCount,
            DeltaTime = SystemAPI.Time.DeltaTime,
        };

        // Schedule parallel job — one thread per Boid
        var jobHandle = job.Schedule(boidCount, 64, state.Dependency);
        jobHandle.Complete();

        // 4. Write results back to entities
        for (int i = 0; i < boidCount; i++)
        {
            var entity = boidEntities[i];

            // Update velocity
            state.EntityManager.SetComponentData(entity, new BoidVelocity
            {
                Value = newVelocities[i]
            });

            // Update position
            var transform = boidPositions[i];
            transform.Position += newVelocities[i] * SystemAPI.Time.DeltaTime;
            state.EntityManager.SetComponentData(entity, transform);
        }

        // 5. Clean up memory
        newVelocities.Dispose();
        boidVelocities.Dispose();
        boidPositions.Dispose();
        boidEntities.Dispose();
    }
}

/// <summary>
/// Parallel job: each Boid independently computes its flocking force.
/// IJobParallelFor schedules one thread per index (per Boid).
/// </summary>
[BurstCompile]
struct BoidFlockingJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<LocalTransform> Positions;
    [ReadOnly] public NativeArray<BoidVelocity> Velocities;
    public NativeArray<float3> NewVelocities;

    public BoidSettings Settings;
    public int BoidCount;
    public float DeltaTime;

    public void Execute(int index)
    {
        var pos = Positions[index].Position;
        var vel = Velocities[index].Value;

        // --- Initialize three accumulators ---
        float3 separation = float3.zero;
        float3 alignment  = float3.zero;
        float3 cohesion   = float3.zero;

        int separationCount = 0;
        int neighborCount   = 0;

        // Iterate over all other Boids to find neighbors
        for (int i = 0; i < BoidCount; i++)
        {
            if (i == index) continue;

            float3 otherPos = Positions[i].Position;
            float3 diff = pos - otherPos;
            float dist = math.length(diff);

            // --- Separation: push apart when too close ---
            if (dist < Settings.SeparationRadius && dist > 0.01f)
            {
                // Closer distance = stronger push (inverse of distance)
                separation += math.normalize(diff) / dist;
                separationCount++;
            }

            // --- Alignment & Cohesion: within perception range ---
            if (dist < Settings.PerceptionRadius)
            {
                alignment += Velocities[i].Value;
                cohesion  += otherPos;
                neighborCount++;
            }
        }

        // --- Compute final steering force ---
        float3 steer = float3.zero;

        // Separation
        if (separationCount > 0)
        {
            separation /= separationCount;
            separation = math.normalize(separation) * Settings.MaxSpeed - vel;
            steer += separation * Settings.SeparationWeight;
        }

        // Alignment
        if (neighborCount > 0)
        {
            alignment /= neighborCount;
            alignment = math.normalize(alignment) * Settings.MaxSpeed - vel;
            steer += alignment * Settings.AlignmentWeight;
        }

        // Cohesion
        if (neighborCount > 0)
        {
            cohesion /= neighborCount;
            cohesion = math.normalize(cohesion - pos) * Settings.MaxSpeed - vel;
            steer += cohesion * Settings.CohesionWeight;
        }

        // --- Boundary avoidance ---
        float3 halfBounds = Settings.BoundsSize * 0.5f;
        if (math.abs(pos.x) > halfBounds.x)
            steer.x += math.sign(halfBounds.x - pos.x) * Settings.BoundaryAvoidanceFactor;
        if (math.abs(pos.y) > halfBounds.y)
            steer.y += math.sign(halfBounds.y - pos.y) * Settings.BoundaryAvoidanceFactor;
        if (math.abs(pos.z) > halfBounds.z)
            steer.z += math.sign(halfBounds.z - pos.z) * Settings.BoundaryAvoidanceFactor;

        // --- Clamp steering force ---
        float steerMag = math.length(steer);
        if (steerMag > Settings.MaxSteerForce)
            steer = steer / steerMag * Settings.MaxSteerForce;

        // --- Apply steering force, update velocity ---
        vel += steer;
        float speed = math.length(vel);
        if (speed > Settings.MaxSpeed)
            vel = vel / speed * Settings.MaxSpeed;
        // Prevent coming to a complete stop
        if (speed < 0.1f)
            vel = math.normalize(new float3(1, 0, 0)) * Settings.MaxSpeed;

        NewVelocities[index] = vel;
    }
}
