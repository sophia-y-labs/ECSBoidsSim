using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// BoidAspect: combines Boid-related components into a queryable "view".
/// Declares read/write permissions via ReadOnly / ReadWrite.
/// </summary>
public readonly partial struct BoidAspect : IAspect
{
    // ========== Entity reference ==========
    public readonly Entity Self;

    // ========== Component references (public required for source-gen) ==========
    public readonly RefRW<LocalTransform> Transform;   // Position + rotation (read-write)
    public readonly RefRW<BoidVelocity>   Velocity;    // Velocity (read-write)
    public readonly RefRO<BoidTag>        Tag;         // Tag (read-only)

    // ========== Convenience properties ==========
    public float3 Position
    {
        get => Transform.ValueRO.Position;
        set => Transform.ValueRW.Position = value;
    }

    public quaternion Rotation
    {
        get => Transform.ValueRO.Rotation;
        set => Transform.ValueRW.Rotation = value;
    }
}
