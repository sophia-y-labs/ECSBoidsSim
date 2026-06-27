using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Makes each Boid rotate to face its velocity direction with smooth slerp and banking.
/// Must run after BoidSystem has updated positions / velocities.
/// </summary>
[BurstCompile]
[UpdateAfter(typeof(BoidSystem))]
public partial struct FaceDirectionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidSettings>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var settings = SystemAPI.GetSingleton<BoidSettings>();

        state.Dependency = new FaceDirectionJob
        {
            DeltaTime       = SystemAPI.Time.DeltaTime,
            RotationSpeed   = settings.RotationSpeed,
            MaxBankAngle    = settings.MaxBankAngle,
            BankingStrength = settings.BankingStrength,
        }.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct FaceDirectionJob : IJobEntity
    {
        public float DeltaTime;
        public float RotationSpeed;
        public float MaxBankAngle;
        public float BankingStrength;

        const float k_MinSpeed = 0.1f;

        [BurstCompile]
        private void Execute(BoidAspect boid)
        {
            float3 dir = boid.Velocity.ValueRO.Value;
            if (math.lengthsq(dir) <= k_MinSpeed * k_MinSpeed)
                return;

            dir = math.normalize(dir);

            quaternion currentRot = boid.Rotation;
            float3 currentForward = math.forward(currentRot);

            // Handle extreme up/down Y-axis cases (avoid gimbal lock)
            float3 up = math.abs(dir.y) > 0.99f
                ? new float3(0, 0, 1)
                : new float3(0, 1, 0);

            quaternion targetRot = quaternion.LookRotation(dir, up);
            float t = math.saturate(RotationSpeed * DeltaTime);
            quaternion baseRot = math.slerp(currentRot, targetRot, t);

            float bankAngle = math.clamp(
                math.cross(currentForward, dir).y * BankingStrength,
                -MaxBankAngle,
                MaxBankAngle);

            if (math.abs(bankAngle) > 1e-5f)
            {
                float3 forward = math.forward(baseRot);
                baseRot = math.mul(quaternion.AxisAngle(forward, bankAngle), baseRot);
            }

            boid.Rotation = baseRot;
        }
    }
}
