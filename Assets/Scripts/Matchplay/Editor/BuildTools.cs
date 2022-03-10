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
        const string relativeBuildFolder = "Builds";
        const string winClientFolder = "Matchplay-WIN";
        const string osxClientFolder = "Matchplay-OSX";
        const string linuxServerFolder = "Matchplay-networkServer";

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

        static BuildReport SquadBuildPlatform(BuildPlatforms platform, bool server = false)
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = AllBuildScenePaths();
            var locationPath = relativeBuildFolder;
            var timeStamp = DateTime.Now.ToFileTimeUtc();
            switch (platform)
            {
                case BuildPlatforms.WIN:
                {
                    locationPath = Path.Combine(locationPath, winClientFolder + $"_{timeStamp}", "Matchplay.exe");
                    buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
                    break;
                }
                case BuildPlatforms.Linux:
                {
                    locationPath = Path.Combine(locationPath, linuxServerFolder + $"_{timeStamp}", "Matchplay.x86_64");
                    buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
                    break;
                }
                case BuildPlatforms.OSX:
                    locationPath = Path.Combine(locationPath, osxClientFolder + $"_{timeStamp}", "Matchplay.app");
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

        [MenuItem("BuildTools/WIN networkClient")]
        public static void BuildWINClient()
        {
            var summary = SquadBuildPlatform(BuildPlatforms.WIN).summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Win networkClient Build succeeded: " + summary.totalTime + " seconds");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        [MenuItem("BuildTools/Linux networkServer")]
        public static void BuildLinuxServer()
        {
            var summary = SquadBuildPlatform(BuildPlatforms.Linux, true).summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Linux networkServer Build succeeded: " + summary.totalTime + " seconds");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        [MenuItem("BuildTools/OSX networkClient")]
        public static void BuildOSXClient()
        {
            var summary = SquadBuildPlatform(BuildPlatforms.OSX).summary;

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
