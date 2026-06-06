using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Makes each Boid rotate to face its velocity direction.
/// Must run after BoidSystem has updated positions / velocities.
/// </summary>
[BurstCompile]
[UpdateAfter(typeof(BoidSystem))]
public partial struct FaceDirectionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var boid in SystemAPI.Query<BoidAspect>())
        {
            float3 dir = boid.Velocity.ValueRO.Value;
            float speed = math.length(dir);

            if (speed > 0.1f)
            {
                dir = math.normalize(dir);

                // Handle extreme up/down Y-axis cases (avoid gimbal lock)
                float3 up = math.abs(dir.y) > 0.99f
                    ? new float3(0, 0, 1)   // Nearly vertical — use Z as up
                    : new float3(0, 1, 0);

                boid.Rotation = quaternion.LookRotation(dir, up);
            }
        }
    }
}
