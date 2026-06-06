using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace ESCBoidsSim
{
    public static class BuildTools
    {
        public static void PerformBuild()
        {
            var args = System.Environment.GetCommandLineArgs();
            var target = GetBuildTarget(args);

            string outputPath;
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                    outputPath = "Builds/Windows/ESCBoidsSim.exe";
                    break;
                case BuildTarget.StandaloneLinux64:
                    outputPath = "Builds/Linux/ESCBoidsSim.x86_64";
                    break;
                default:
                    UnityEngine.Debug.LogError($"Unsupported build target: {target}");
                    EditorApplication.Exit(1);
                    return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = outputPath,
                targetGroup = BuildTargetGroup.Standalone,
                target = target,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var result = report.summary.result;

            if (result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log($"Build succeeded → {outputPath}");
                EditorApplication.Exit(0);
            }
            else
            {
                UnityEngine.Debug.LogError($"Build failed: {result}");
                EditorApplication.Exit(1);
            }
        }

        private static BuildTarget GetBuildTarget(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-buildTarget" && i + 1 < args.Length)
                {
                    var target = args[i + 1].ToLower();
                    return target switch
                    {
                        "windows" or "standalonewindows64" => BuildTarget.StandaloneWindows64,
                        "linux" or "standalonelinux64" => BuildTarget.StandaloneLinux64,
                        _ => BuildTarget.StandaloneWindows64
                    };
                }
            }
            return BuildTarget.StandaloneWindows64;
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }
    }
}
