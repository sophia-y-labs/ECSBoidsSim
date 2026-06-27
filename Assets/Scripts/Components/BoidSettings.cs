using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Global settings component: parameters controlling Boid flocking behavior.
/// Attached as a singleton on a single entity.
/// </summary>
public struct BoidSettings : IComponentData
{
    // ========== Three rule weights ==========
    public float SeparationWeight;   // Separation rule weight
    public float AlignmentWeight;    // Alignment rule weight
    public float CohesionWeight;     // Cohesion rule weight

    // ========== Perception ranges ==========
    public float SeparationRadius;   // Separation perception radius (close range)
    public float PerceptionRadius;   // Overall perception radius (long range)

    // ========== Movement limits ==========
    public float MaxSpeed;           // Maximum movement speed
    public float MaxSteerForce;      // Maximum steering force

    // ========== Scene boundary ==========
    public float3 BoundsSize;        // Boundary dimensions (XYZ)
    public float BoundaryAvoidanceFactor; // Boundary avoidance strength

    // ========== Rotation / banking ==========
    public float RotationSpeed;      // Slerp rate (t = RotationSpeed * DeltaTime)
    public float MaxBankAngle;       // Max roll angle in radians
    public float BankingStrength;    // Turn-to-roll multiplier

    /// <summary>
    /// Returns a sensible default configuration.
    /// </summary>
    public static BoidSettings Default => new BoidSettings
    {
        SeparationWeight = 2.0f,
        AlignmentWeight  = 1.0f,
        CohesionWeight   = 1.0f,

        SeparationRadius = 3.0f,
        PerceptionRadius = 8.0f,

        MaxSpeed         = 5.0f,
        MaxSteerForce    = 0.5f,

        BoundsSize       = new float3(30f, 30f, 30f),
        BoundaryAvoidanceFactor = 3.0f,

        RotationSpeed    = 8.0f,
        MaxBankAngle     = math.radians(15f),
        BankingStrength  = 2.0f,
    };
}
