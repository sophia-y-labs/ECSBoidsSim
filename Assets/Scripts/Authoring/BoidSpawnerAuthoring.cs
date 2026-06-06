using Unity.Entities;
using UnityEngine;

/// <summary>
/// Boid spawner.
/// Attached to a GameObject in the scene to specify how many Boids to spawn.
/// </summary>
public class BoidSpawnerAuthoring : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject BoidPrefab;        // Drag the Boid prefab here
    public int SpawnCount = 100;          // Number of Boids to spawn
    public float SpawnRadius = 10f;       // Random distribution radius
}

/// <summary>
/// Baker: converts spawner data into an ECS component.
/// </summary>
public class BoidSpawnerBaker : Baker<BoidSpawnerAuthoring>
{
    public override void Bake(BoidSpawnerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        // Get the Entity reference for the Boid prefab
        // Note: BoidPrefab itself must also be baked (i.e. have BoidAuthoring attached)
        var prefabEntity = GetEntity(authoring.BoidPrefab, TransformUsageFlags.Dynamic);

        AddComponent(entity, new BoidSpawner
        {
            Prefab      = prefabEntity,
            SpawnCount  = authoring.SpawnCount,
            SpawnRadius = authoring.SpawnRadius,
        });
    }
}

/// <summary>
/// ECS component: stores spawn parameters.
/// </summary>
public struct BoidSpawner : IComponentData
{
    public Entity Prefab;         // The prefab Entity to instantiate
    public int SpawnCount;        // Number of instances
    public float SpawnRadius;     // Distribution radius
}
