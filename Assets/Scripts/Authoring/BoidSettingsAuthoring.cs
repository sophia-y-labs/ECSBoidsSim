using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Attached to an empty GameObject in the scene.
/// Its Inspector panel provides direct control over Boid flocking parameters.
/// </summary>
public class BoidSettingsAuthoring : MonoBehaviour
{
    // ========== Three rule weights ==========
    [Header("Flocking Weights")]
    public float SeparationWeight = 2.0f;
    public float AlignmentWeight  = 1.0f;
    public float CohesionWeight   = 1.0f;

    // ========== Perception ranges ==========
    [Header("Perception")]
    public float SeparationRadius = 3.0f;
    public float PerceptionRadius = 8.0f;

    // ========== Movement limits ==========
    [Header("Movement")]
    public float MaxSpeed      = 5.0f;
    public float MaxSteerForce = 0.5f;

    // ========== Scene boundary ==========
    [Header("Boundary")]
    public float3 BoundsSize = new float3(30, 30, 30);
    public float BoundaryAvoidanceFactor = 3.0f;
}

public class BoidSettingsBaker : Baker<BoidSettingsAuthoring>
{
    public override void Bake(BoidSettingsAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new BoidSettings
        {
            SeparationWeight        = authoring.SeparationWeight,
            AlignmentWeight         = authoring.AlignmentWeight,
            CohesionWeight          = authoring.CohesionWeight,
            SeparationRadius        = authoring.SeparationRadius,
            PerceptionRadius        = authoring.PerceptionRadius,
            MaxSpeed                = authoring.MaxSpeed,
            MaxSteerForce           = authoring.MaxSteerForce,
            BoundsSize              = authoring.BoundsSize,
            BoundaryAvoidanceFactor = authoring.BoundaryAvoidanceFactor,
        });
    }
}
