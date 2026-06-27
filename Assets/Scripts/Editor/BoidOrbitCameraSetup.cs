#if UNITY_EDITOR
using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ESCBoidsSim.Editor
{
    /// <summary>
    /// One-shot scene setup for Cinemachine orbit camera. Run via batchmode ExecuteMethod.
    /// </summary>
    public static class BoidOrbitCameraSetup
    {
        const string k_ScenePath = "Assets/Scenes/BiodsScene/BiodsScene.unity";
        const string k_OrbitPivotName = "OrbitPivot";
        const string k_FreeLookName = "CM Orbit Camera";

        [MenuItem("ESCBoidsSim/Setup Orbit Camera")]
        public static void SetupFromMenu()
        {
            PerformSetup();
        }

        /// <summary>Batchmode entry point for automated scene setup.</summary>
        public static void PerformSetup()
        {
            Setup();
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
        }

        public static void Setup()
        {
            var scene = EditorSceneManager.OpenScene(k_ScenePath, OpenSceneMode.Single);

            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[BoidOrbitCameraSetup] Main Camera not found.");
                return;
            }

            EnsureBrain(mainCamera);
            EnsureUrpTaa(mainCamera);

            var pivot = EnsureOrbitPivot();
            var freeLook = EnsureFreeLook(pivot.transform);
            EnsureDriver(freeLook);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[BoidOrbitCameraSetup] Orbit camera setup complete.");
        }

        static void EnsureBrain(Camera camera)
        {
            if (camera.GetComponent<CinemachineBrain>() == null)
                camera.gameObject.AddComponent<CinemachineBrain>();
        }

        static void EnsureUrpTaa(Camera camera)
        {
            var urp = camera.GetComponent<UniversalAdditionalCameraData>();
            if (urp == null)
                urp = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();

            urp.antialiasing = AntialiasingMode.TemporalAntiAliasing;
            urp.antialiasingQuality = AntialiasingQuality.High;
            urp.renderPostProcessing = true;
        }

        static GameObject EnsureOrbitPivot()
        {
            var existing = GameObject.Find(k_OrbitPivotName);
            if (existing != null)
                return existing;

            var pivot = new GameObject(k_OrbitPivotName);
            pivot.transform.position = Vector3.zero;
            return pivot;
        }

        static CinemachineFreeLook EnsureFreeLook(Transform pivot)
        {
            var existingGo = GameObject.Find(k_FreeLookName);
            CinemachineFreeLook freeLook;

            if (existingGo != null)
            {
                freeLook = existingGo.GetComponent<CinemachineFreeLook>();
                if (freeLook == null)
                    freeLook = existingGo.AddComponent<CinemachineFreeLook>();
            }
            else
            {
                var go = new GameObject(k_FreeLookName);
                freeLook = go.AddComponent<CinemachineFreeLook>();
            }

            freeLook.m_Follow = pivot;
            freeLook.m_LookAt = pivot;
            freeLook.m_Priority = 10;
            freeLook.m_CommonLens = true;
            freeLook.m_Lens.FieldOfView = 40f;
            freeLook.m_Lens.NearClipPlane = 0.3f;
            freeLook.m_Lens.FarClipPlane = 1000f;

            freeLook.m_BindingMode = CinemachineOrbitalTransposer.BindingMode.SimpleFollowWithWorldUp;
            freeLook.m_SplineCurvature = 0.2f;

            freeLook.m_Orbits[0] = new CinemachineFreeLook.Orbit(6f, 28f);
            freeLook.m_Orbits[1] = new CinemachineFreeLook.Orbit(0f, 26f);
            freeLook.m_Orbits[2] = new CinemachineFreeLook.Orbit(-4f, 24f);

            freeLook.m_YAxis.Value = 0.45f;
            freeLook.m_YAxis.m_MaxSpeed = 0f;
            freeLook.m_YAxis.m_InputAxisName = string.Empty;

            freeLook.m_XAxis.m_InputAxisName = string.Empty;
            freeLook.m_XAxis.m_MaxSpeed = 0f;

            freeLook.m_RecenterToTargetHeading.m_enabled = false;

            EditorUtility.SetDirty(freeLook);
            return freeLook;
        }

        static void EnsureDriver(CinemachineFreeLook freeLook)
        {
            var driver = freeLook.GetComponent<BoidOrbitCameraDriver>();
            if (driver == null)
                driver = freeLook.gameObject.AddComponent<BoidOrbitCameraDriver>();

            var so = new SerializedObject(driver);
            so.FindProperty("m_FreeLook").objectReferenceValue = freeLook;
            so.FindProperty("m_OrbitSpeed").floatValue = 12f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
