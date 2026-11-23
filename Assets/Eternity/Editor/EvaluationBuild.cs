using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
internal static class EvaluationBuild
{
    const string Request = "WindowsBuild.request";
    static EvaluationBuild() => EditorApplication.update += CheckRequest;

    static void CheckRequest()
    {
        if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Request);
        BuildWindows();
    }

    [MenuItem("Eternity/Build Windows game")]
    public static void BuildWindows()
    {
        Directory.CreateDirectory("Builds/Windows");
        try
        {
            var options = new BuildPlayerOptions {
                scenes = new[] { "Assets/Eternity/HeartOfEternity_Temple.unity" },
                locationPathName = "Builds/Windows/HeartOfEternity.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var summary = BuildPipeline.BuildPlayer(options).summary;
            File.WriteAllText("Builds/WindowsBuild.txt", $"{summary.result}, {summary.totalErrors} errors, {summary.totalSize} bytes");
        }
        catch (Exception error)
        {
            File.WriteAllText("Builds/WindowsBuild.txt", error.ToString());
            Debug.LogException(error);
        }
    }
}
