using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Renders the simulation boundary as a wire cube. Visual only; does not affect simulation.
/// Requires a child <see cref="LineRenderer"/> (e.g. BoundsWireframe).
/// Lives in the main scene; reads bounds from <see cref="BoidSettingsAuthoring"/> in edit mode
/// or the ECS <see cref="BoidSettings"/> singleton at runtime.
/// </summary>
[ExecuteAlways]
public class BoidBoundsAuthoring : MonoBehaviour
{
    [SerializeField] BoidSettingsAuthoring m_Settings;
    [SerializeField] LineRenderer m_LineRenderer;
    [SerializeField] Material m_LineMaterial;
    [SerializeField] Color m_LineColor = new Color(0.15f, 0.55f, 0.85f, 0.75f);
    [SerializeField] float m_LineWidth = 0.04f;
    [SerializeField] float3 m_FallbackBoundsSize = new float3(30f, 30f, 30f);

    static readonly (int a, int b)[] k_Edges =
    {
        (0, 1), (1, 2), (2, 3), (3, 0),
        (4, 5), (5, 6), (6, 7), (7, 4),
        (0, 4), (1, 5), (2, 6), (3, 7),
    };

    void OnEnable() => Rebuild();

    void OnValidate() => Rebuild();

    void Start()
    {
        if (Application.isPlaying)
            Rebuild();
    }

    void Rebuild()
    {
        if (m_LineRenderer == null)
            m_LineRenderer = GetComponentInChildren<LineRenderer>(true);

        if (m_LineRenderer == null)
            return;

        ApplyLineRendererSettings();
        ResolveBounds(out var center, out var size);
        UpdateWireframe(center, size);
    }

    void ResolveBounds(out Vector3 center, out Vector3 size)
    {
        if (Application.isPlaying && TryGetEcsBounds(out center, out size))
            return;

        if (m_Settings == null)
            m_Settings = FindAnyObjectByType<BoidSettingsAuthoring>();

        center = m_Settings != null ? m_Settings.transform.position : transform.position;
        size = m_Settings != null
            ? new Vector3(m_Settings.BoundsSize.x, m_Settings.BoundsSize.y, m_Settings.BoundsSize.z)
            : new Vector3(m_FallbackBoundsSize.x, m_FallbackBoundsSize.y, m_FallbackBoundsSize.z);
    }

    static bool TryGetEcsBounds(out Vector3 center, out Vector3 size)
    {
        center = Vector3.zero;
        size = Vector3.zero;

        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
            return false;

        var em = world.EntityManager;
        using var query = em.CreateEntityQuery(ComponentType.ReadOnly<BoidSettings>());
        if (query.IsEmptyIgnoreFilter)
            return false;

        var settings = query.GetSingleton<BoidSettings>();
        center = Vector3.zero;
        size = new Vector3(settings.BoundsSize.x, settings.BoundsSize.y, settings.BoundsSize.z);
        return true;
    }

    void ApplyLineRendererSettings()
    {
        m_LineRenderer.useWorldSpace = true;
        m_LineRenderer.loop = false;
        m_LineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        m_LineRenderer.receiveShadows = false;
        m_LineRenderer.lightProbeUsage = LightProbeUsage.Off;
        m_LineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        m_LineRenderer.textureMode = LineTextureMode.Stretch;
        m_LineRenderer.alignment = LineAlignment.View;
        m_LineRenderer.startColor = m_LineColor;
        m_LineRenderer.endColor = m_LineColor;
        m_LineRenderer.startWidth = m_LineWidth;
        m_LineRenderer.endWidth = m_LineWidth;
        m_LineRenderer.positionCount = k_Edges.Length * 2;
        m_LineRenderer.numCornerVertices = 4;
        m_LineRenderer.numCapVertices = 4;

        if (m_LineMaterial == null)
            return;

        m_LineRenderer.sharedMaterial = m_LineMaterial;

        var material = Application.isPlaying ? m_LineRenderer.material : m_LineRenderer.sharedMaterial;
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", m_LineColor);
    }

    void UpdateWireframe(Vector3 center, Vector3 size)
    {
        var half = size * 0.5f;

        var corners = new Vector3[8];
        corners[0] = center + new Vector3(-half.x, -half.y, -half.z);
        corners[1] = center + new Vector3(half.x, -half.y, -half.z);
        corners[2] = center + new Vector3(half.x, -half.y, half.z);
        corners[3] = center + new Vector3(-half.x, -half.y, half.z);
        corners[4] = center + new Vector3(-half.x, half.y, -half.z);
        corners[5] = center + new Vector3(half.x, half.y, -half.z);
        corners[6] = center + new Vector3(half.x, half.y, half.z);
        corners[7] = center + new Vector3(-half.x, half.y, half.z);

        var index = 0;
        for (var i = 0; i < k_Edges.Length; i++)
        {
            var (a, b) = k_Edges[i];
            m_LineRenderer.SetPosition(index++, corners[a]);
            m_LineRenderer.SetPosition(index++, corners[b]);
        }
    }
}
