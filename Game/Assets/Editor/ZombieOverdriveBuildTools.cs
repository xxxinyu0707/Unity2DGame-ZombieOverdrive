using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class ZombieOverdriveBuildTools
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string ExeName = "ZombieOverdrive.exe";

    [MenuItem("Zombie Overdrive/Build Windows Player")]
    public static void BuildWindows64()
    {
        string outputPath = GetCommandLineValue("-buildOutput");
        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = GetDefaultWindowsOutputPath();
        }

        BuildWindows64(outputPath);
    }

    private static void BuildWindows64(string outputPath)
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
        {
            throw new InvalidOperationException("Standalone Windows 64-bit build support is not installed.");
        }

        string normalizedOutputPath = Path.GetFullPath(outputPath);
        string buildDirectory = Path.GetDirectoryName(normalizedOutputPath);
        if (string.IsNullOrEmpty(buildDirectory))
        {
            throw new InvalidOperationException("Build output path is invalid: " + outputPath);
        }

        Directory.CreateDirectory(buildDirectory);

        EnsureSceneReady();

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        PlayerSettings.companyName = "Course Assignment";
        PlayerSettings.productName = "Zombie Overdrive";

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = normalizedOutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        string infoPath = Path.Combine(buildDirectory, "build-info.txt");
        File.WriteAllText(
            infoPath,
            "Zombie Overdrive Windows Build" + Environment.NewLine +
            "Result: " + summary.result + Environment.NewLine +
            "Errors: " + summary.totalErrors + Environment.NewLine +
            "Warnings: " + summary.totalWarnings + Environment.NewLine +
            "Size: " + summary.totalSize + " bytes" + Environment.NewLine +
            "Output: " + normalizedOutputPath + Environment.NewLine);

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException("Windows build failed: " + summary.result);
        }

        Debug.Log("Zombie Overdrive Windows build succeeded: " + normalizedOutputPath);
    }

    private static string GetDefaultWindowsOutputPath()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string repoRoot = Directory.GetParent(projectRoot).FullName;
        return Path.Combine(repoRoot, "Builds", "Windows", ExeName);
    }

    private static void EnsureSceneReady()
    {
        if (!File.Exists(ScenePath))
        {
            ZombieOverdriveSceneBuilder.BuildPrototypeScene();
        }

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        ZombieOverdriveSceneBuilder.ValidatePrototypeScene();
    }

    private static string GetCommandLineValue(string key)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
