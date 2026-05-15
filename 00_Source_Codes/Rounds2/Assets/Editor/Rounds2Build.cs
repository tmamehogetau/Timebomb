using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Rounds2.Editor
{
    public static class Rounds2Build
    {
        private static readonly string[] Scenes =
        {
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/Arena01.unity"
        };

        public static void BuildAllWindows()
        {
            BuildWindowsServer();
            BuildWindowsClient();
        }

        public static void BuildWindowsServer()
        {
            BuildWindows(
                "Builds/Server/Rounds2Server.exe",
                StandaloneBuildSubtarget.Server);
        }

        public static void BuildWindowsClient()
        {
            BuildWindows(
                "Builds/Client/Rounds2Client.exe",
                StandaloneBuildSubtarget.Player);
        }

        private static void BuildWindows(string outputPath, StandaloneBuildSubtarget subtarget)
        {
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            BuildPlayerOptions options = new()
            {
                scenes = Scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                subtarget = (int)subtarget,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Build failed for {outputPath}: {report.summary.result}");
            }
        }
    }
}
