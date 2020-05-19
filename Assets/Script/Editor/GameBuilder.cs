using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class GameBuilder
{
    const string kFileName = "konane";
    const string kRevisionPath = "Assets/Revision.cs";

    static readonly Dictionary<string, string> CommandLindArgs = new Dictionary<string, string>();

    static GameBuilder()
    {
        var args = Environment.GetCommandLineArgs();

        CommandLindArgs.Clear();
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--"))
                continue;

            var key = args[i];
            var value = string.Empty;

            if (i + 1 < args.Length)
            {
                if (!args[i + 1].StartsWith("--"))
                    value = args[++i];
            }

            CommandLindArgs[key] = value;
        }
    }

    static bool GetCmdArg(string key, out string value)
    {
        var r = CommandLindArgs.TryGetValue(key, out value);
        Debug.Log($"!!!!!!!!!!!!!! pass-in {key}: {value}");
        return r;
    }

    [UsedImplicitly]
    static void BuildViaCommandLine()
    {
        GetCmdArg("--buildingVersion", out var version);
        GetCmdArg("--buildingFolder", out var outputPath);

        RefreshRevision(version);

        var target = GetCurrentTarget();
        var group = BuildTargetToGroup(target);
        var success = TryBuild(target, group, outputPath);
        EditorApplication.Exit(success ? 0 : -1);
    }

    [MenuItem("Tools/Build/Android")]
    static void BuildAndroid()
    {
        Debug.Log($"BuildAndroid");
        var target = BuildTarget.Android;
        var group = BuildTargetToGroup(target);
        var success = EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        if (!success)
            return;

        var outputPath = Application.dataPath;
        outputPath = outputPath.Replace("/Assets", "/Output");
        TryBuild(target, group, outputPath);
    }

    [MenuItem("Tools/Build/Standalone(Windows)")]
    static void BuildStandalone()
    {
        var target = BuildTarget.StandaloneWindows;
        var group = BuildTargetToGroup(target);
        var success = EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        if (!success)
            return;

        var outputPath = Application.dataPath;
        outputPath = outputPath.Replace("/Assets", "/Output");
        TryBuild(target, group, outputPath);
    }


    [MenuItem("Tools/Build/All")]
    static void BuildAll()
    {
        var outputPath = Application.dataPath;
        outputPath = outputPath.Replace("/Assets", "/Output");

        var target = GetCurrentTarget();
        switch (target)
        {
            default:
            case BuildTarget.Android:
                {
                    BuildConsequent(BuildTarget.Android, BuildTarget.StandaloneWindows, outputPath);
                    break;
                }

            case BuildTarget.StandaloneWindows:
                {
                    BuildConsequent(BuildTarget.StandaloneWindows, BuildTarget.Android, outputPath);
                    break;
                }
        }
    }

    static void BuildConsequent(BuildTarget firstTarget, BuildTarget secondTarget, string outputPath)
    {
        var target = firstTarget;
        var group = BuildTargetToGroup(target);
        var success = EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        if (!success)
            return;

        success = TryBuild(target, group, outputPath);
        if (!success)
            return;

        target = secondTarget;
        group = BuildTargetToGroup(target);
        success = EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        if (!success)
            return;
        TryBuild(target, group, outputPath);
    }


    static bool TryBuild(BuildTarget target, BuildTargetGroup group, string outputPath)
    {
        Debug.Log($"TryBuild {target}, {group}, {outputPath}");
        string buildError;
        switch (target)
        {
            case BuildTarget.Android:
                {
                    buildError = Build(target, group,
                                        $@"{outputPath}/{target}/",
                                        BuildOptions.StrictMode);
                    break;
                }
            case BuildTarget.StandaloneWindows:
                {
                    buildError = Build(target, group,
                                        $@"{outputPath}/{target}/",
                                        BuildOptions.StrictMode);
                    break;
                }
            default:
                buildError = "No fit playform";
                break;
        }

        return FinalCheck(buildError);
    }

    static string Build(BuildTarget target, BuildTargetGroup targetGroup, string outputPath, BuildOptions opts = BuildOptions.None)
    {
        var errorText = string.Empty;

        Debug.Log($"Building starts @ {DateTime.Now:yyyy-MM-dd HH:mm.ss.fff}");
        Debug.Log($"OutputPath {outputPath}");

        var originalDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        do
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                errorText = "Output path is empty!";
                break;
            }

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var fullPath = string.Empty;

            switch (target)
            {
                case BuildTarget.Android:
                    {
                        //EditorUserBuildSettings.androidCreateSymbolsZip = ((opts & BuildOptions.Development) != 0) ? false : true;
                        var filename = $"{kFileName}.apk";
                        fullPath = Path.Combine(outputPath, filename);
                    }
                    break;

                case BuildTarget.StandaloneWindows:
                    {
                        var filename = $"{kFileName}.exe";
                        fullPath = Path.Combine(outputPath, filename);
                    }
                    break;
            }

            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetOutputScenesStrAry();
            buildPlayerOptions.locationPathName = fullPath;
            buildPlayerOptions.target = target;
            buildPlayerOptions.targetGroup = targetGroup;
            buildPlayerOptions.options = opts;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report == null)
            {
                errorText = "report == null!!";
                break;
            }

            PrintBuildReport(report);

            if (report.summary.result != BuildResult.Succeeded)
                errorText = report.summary.result.ToString();
        }
        while (false);

        // 恢復設定
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, originalDefines);

        return errorText;
    }

    [MenuItem("Tools/Create Reversion File")]
    static void CreateReversionFile()
    {
        RefreshRevision("0.0.0");
    }

    static void RefreshRevision(string svnRevision)
    {
        if (Directory.Exists(kRevisionPath))
            AssetDatabase.DeleteAsset(kRevisionPath);

        using (var sw = new StreamWriter($"{Directory.GetCurrentDirectory()}/{kRevisionPath}", false, new UTF8Encoding(true)))
        {
            sw.WriteLine(
                "namespace TrainingProject {{ " +
                    "public sealed class Version" +
                    "{{ " +
                        "public const string VERSION = \"{0}\";" +
                    " }}" +
                " }}", svnRevision);
            sw.Close();
        }

        AssetDatabase.Refresh();
    }

    static bool FinalCheck(string buildError)
    {
        if (string.IsNullOrEmpty(buildError))
        {
            Debug.Log("build succeed.");
            return true;
        }
        else
        {
            Debug.Log($"build failed, error:{buildError}");
            return false;
        }
    }

    static void PrintBuildReport(BuildReport report)
    {
        var result = "---------------- report Start ----------------\n";
        result += $"start building at {report.summary.buildStartedAt:HH:mm:ss}\n";
        result += $"end building at {report.summary.buildEndedAt:HH:mm:ss}\n";
        result += $"total time: {report.summary.totalTime}\n";
        result += $"total errors: {report.summary.totalErrors}\n";
        result += $"result: {report.summary.result}\n";

        #region step
        result += "===step top===\n";
        foreach (var step in report.steps)
        {
            result += $"{step.ToString()}\n";

            foreach (var msg in step.messages)
            {
                switch (msg.type)
                {
                    case LogType.Error:
                    case LogType.Exception:
                        result += $"\t[{msg.type}] {msg.content}\n";
                        break;
                }
            }
        }
        result += "===step end===\n";
        #endregion

        #region strippingInfo
        result += "===strippingInfo top===\n";
        if (report.strippingInfo != null)
        {
            foreach (var module in report.strippingInfo.includedModules)
            {
                result += $"module [{module}] reasons:\n";
                foreach (var reason in report.strippingInfo.GetReasonsForIncluding(module))
                    result += $"{reason}\n";
            }
        }
        result += "===strippingInfo end===\n";
        #endregion

        result += "---------------- report EOF ----------------\n";

        Debug.Log(result);
    }

    static BuildTarget GetCurrentTarget()
    {
        var target = BuildTarget.NoTarget;
#if UNITY_ANDROID
        target = BuildTarget.Android;
#elif UNITY_STANDALONE
        target = BuildTarget.StandaloneWindows;
#endif

        return target;
    }

    static BuildTargetGroup BuildTargetToGroup(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneOSX:
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneLinux64:
                return BuildTargetGroup.Standalone;

            case BuildTarget.iOS:
            case BuildTarget.tvOS:
                return BuildTargetGroup.iOS;

            case BuildTarget.Android:
                return BuildTargetGroup.Android;

            case BuildTarget.WebGL:
                return BuildTargetGroup.WebGL;

            case BuildTarget.WSAPlayer:
                return BuildTargetGroup.WSA;

            case BuildTarget.PS4:
                return BuildTargetGroup.PS4;

            case BuildTarget.XboxOne:
                return BuildTargetGroup.XboxOne;

            case BuildTarget.Switch:
                return BuildTargetGroup.Switch;

            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    static string[] GetOutputScenesStrAry()
    {
        var scenes = EditorBuildSettings.scenes;
        var output = new List<string>();

        foreach (var scene in scenes)
        {
            if (scene.enabled)
                output.Add(scene.path);
        }

        return output.ToArray();
    }
}
