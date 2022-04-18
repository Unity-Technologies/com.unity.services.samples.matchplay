using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Matchplay.Editor
{
    public enum BuildPlatforms
    {
        WIN,
        Linux,
        OSX
    }

    public class BuildTools
    {
        const string k_RelativeBuildFolder = "Builds";
        const string k_WinClientFolder = "Matchplay-WIN";
        const string k_OSXClientFolder = "Matchplay-OSX";
        const string k_LinuxServerFolder = "Matchplay-networkServer";

        public static string[] AllBuildScenePaths()
        {
            List<string> scenePaths = new List<string>();

            foreach (var scene in EditorBuildSettings.scenes)
            {
                Debug.Log($"Adding Scene: {scene.path} ");
                scenePaths.Add(scene.path);
            }

            return scenePaths.ToArray();
        }

        [MenuItem("BuildTools/All")]
        public static void BuildAll()
        {
            BuildWINClient();
            BuildLinuxServer();
            BuildOSXClient();
        }

        static BuildReport BuildForPlatform(BuildPlatforms platform, bool server = false)
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = AllBuildScenePaths();
            var locationPath = k_RelativeBuildFolder;
            var timeStamp = DateTime.Now.ToFileTimeUtc();
            switch (platform)
            {
                case BuildPlatforms.WIN:
                {
                    locationPath = Path.Combine(locationPath, k_WinClientFolder + $"_{timeStamp}", "Matchplay.exe");
                    buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
                    break;
                }
                case BuildPlatforms.Linux:
                {
                    locationPath = Path.Combine(locationPath, k_LinuxServerFolder + $"_{timeStamp}", "Matchplay.x86_64");
                    buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
                    break;
                }
                case BuildPlatforms.OSX:
                    locationPath = Path.Combine(locationPath, k_OSXClientFolder + $"_{timeStamp}", "Matchplay.app");
                    buildPlayerOptions.target = BuildTarget.StandaloneOSX;
                    break;
            }

            buildPlayerOptions.locationPathName = locationPath;
            buildPlayerOptions.options = BuildOptions.Development;

            if (server)
                buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;

            return BuildPipeline.BuildPlayer(buildPlayerOptions);
            ;
        }

        [MenuItem("BuildTools/WIN Client")]
        public static void BuildWINClient()
        {
            var summary = BuildForPlatform(BuildPlatforms.WIN).summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Win networkClient Build succeeded: " + summary.totalTime + " seconds");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        [MenuItem("BuildTools/Linux Server")]
        public static void BuildLinuxServer()
        {
            var summary = BuildForPlatform(BuildPlatforms.Linux, true).summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Linux networkServer Build succeeded: " + summary.totalTime + " seconds");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"Build failed for {summary.platformGroup} @ {summary.outputPath}");
            }
        }

        [MenuItem("BuildTools/OSX Client")]
        public static void BuildOSXClient()
        {
            var summary = BuildForPlatform(BuildPlatforms.OSX).summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("OSX networkClient Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }
    }
}
