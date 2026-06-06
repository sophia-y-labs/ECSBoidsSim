using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// MonoBehaviour attached to the Boid prefab.
/// It does not run any logic at runtime — its sole purpose is to
/// tell ECS which components to create during Baking.
/// </summary>
public class BoidAuthoring : MonoBehaviour
{
    /// <summary>
    /// Initial velocity direction (adjustable in the Inspector).
    /// </summary>
    public float3 InitialVelocity = new float3(0, 0, 1);
}

/// <summary>
/// Baker: converts authoring data into ECS components.
/// Whenever a GameObject with BoidAuthoring is placed in a SubScene,
/// Unity automatically invokes this Baker.
/// </summary>
public class BoidBaker : Baker<BoidAuthoring>
{
    public override void Bake(BoidAuthoring authoring)
    {
        // GetEntity: retrieves the Entity corresponding to this GameObject
        // TransformUsageFlags.Dynamic indicates this entity's position can change
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        // AddComponent: attach ECS components to this Entity
        // These components are read and modified by Systems at runtime
        AddComponent<BoidTag>(entity);
        AddComponent<Prefab>(entity);  // ← 标记为预制体模板，才能被 Instantiate

        AddComponent(entity, new BoidVelocity
        {
            Value = authoring.InitialVelocity
        });
    }
}
