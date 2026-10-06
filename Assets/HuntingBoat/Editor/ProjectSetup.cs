using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace HuntingBoat.Editor
{
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/HuntingBoat/Scenes/Ocean.unity";
        [MenuItem("Hunting Boat/Open Ocean Scene")]
        public static void OpenScene() { EditorSceneManager.OpenScene(ScenePath); }
        [MenuItem("Hunting Boat/Build Windows Player")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory("Builds/Windows");
            var result = BuildPipeline.BuildPlayer(new[] { ScenePath }, "Builds/Windows/HuntingBoat.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Player build failed. Inspect Unity's build report.");
        }
        [MenuItem("Hunting Boat/Build macOS Player")]
        public static void BuildMac()
        {
            Directory.CreateDirectory("Builds/macOS");
            var result = BuildPipeline.BuildPlayer(new[] { ScenePath }, "Builds/macOS/HuntingBoat.app", BuildTarget.StandaloneOSX, BuildOptions.None);
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Player build failed.");
        }
    }
}
