using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Velocity component: stores the Boid's current velocity (direction and speed).
/// </summary>
public struct BoidVelocity : IComponentData
{
    public float3 Value;
}
