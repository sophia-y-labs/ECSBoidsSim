using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Spawn system: creates all Boids at game start, then destroys itself.
/// Must run before BoidSystem so that Boids exist on the first frame.
/// </summary>
[BurstCompile]
[UpdateBefore(typeof(BoidSystem))]
public partial struct BoidSpawnSystem : ISystem
{
    /// <summary>
    /// Flag to ensure spawning runs only once.
    /// </summary>
    private bool m_HasSpawned;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (m_HasSpawned) return;

        // Step 1: Find the Spawner entity and read its data
        // IMPORTANT: collect data first, then do structural changes AFTER the loop!
        Entity spawnerEntity = Entity.Null;
        var prefab   = Entity.Null;
        int count    = 0;
        float radius = 0;

        foreach (var (spawner, entity) in
                 SystemAPI.Query<RefRO<BoidSpawner>>()
                     .WithEntityAccess())
        {
            spawnerEntity = entity;
            prefab  = spawner.ValueRO.Prefab;
            count   = spawner.ValueRO.SpawnCount;
            radius  = spawner.ValueRO.SpawnRadius;
        }

        if (spawnerEntity == Entity.Null) return;

        // Step 2: Spawn all Boids (structural changes: Instantiate)
        var random = Random.CreateFromIndex((uint)spawnerEntity.Index);

        for (int i = 0; i < count; i++)
        {
            float3 randomPos = random.NextFloat3() * radius * 2 - new float3(radius);
            float3 randomVel = math.normalize(random.NextFloat3() - new float3(0.5f));

            var boid = state.EntityManager.Instantiate(prefab);
            state.EntityManager.SetComponentData(boid, LocalTransform.FromPosition(randomPos));
            state.EntityManager.SetComponentData(boid, new BoidVelocity
            {
                Value = randomVel * radius
            });
        }

        // Step 3: Destroy the Spawner entity (structural change)
        // This is safe now because the foreach loop has already completed!
        state.EntityManager.DestroyEntity(spawnerEntity);

        m_HasSpawned = true;
    }
}
