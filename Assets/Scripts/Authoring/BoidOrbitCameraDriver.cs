using Cinemachine;
using UnityEngine;

/// <summary>
/// Drives automatic horizontal orbit on a Cinemachine FreeLook rig.
/// Visual only; no ECS dependency.
/// </summary>
[ExecuteAlways]
public class BoidOrbitCameraDriver : MonoBehaviour
{
    [SerializeField] CinemachineFreeLook m_FreeLook;
    [SerializeField] float m_OrbitSpeed = 12f;

    void Reset()
    {
        m_FreeLook = GetComponent<CinemachineFreeLook>();
    }

    void Update()
    {
        if (!Application.isPlaying || m_FreeLook == null)
            return;

        m_FreeLook.m_XAxis.Value += m_OrbitSpeed * Time.deltaTime;
    }
}
