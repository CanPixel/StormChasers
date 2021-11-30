using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;

namespace FlatKit {
[CustomEditor(typeof(FlatKitReadme))]
public class ReadmeEditor : Editor {
    private static readonly string AssetName = "Flat Kit";

    private static readonly GUID UnityPackageUrpGuid = new GUID("41e59f562b69648719f2424c438758f3");

    private static readonly GUID UnityPackageBuiltInGuid = new GUID("f4227764308e84f89a765fbf315e2945");

    private static readonly GUID UrpPipelineAssetGuid = new GUID("ecbd363870e07455ea237f5753668d30");

    private FlatKitReadme _readme;
    private bool _showingVersionMessage;
    private string _versionLatest;

    private bool _showingClearCacheMessage;
    private bool _cacheClearedSuccessfully;

    private void OnEnable() {
        _readme = serializedObject.targetObject as FlatKitReadme;
        if (_readme == null) {
            Debug.LogError($"[{AssetName}] Readme error.");
            return;
        }

        _readme.Refresh();
        _showingVersionMessage = false;
        _showingClearCacheMessage = false;
        _versionLatest = null;

        AssetDatabase.importPackageStarted += OnImportPackageStarted;
        AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
        AssetDatabase.importPackageFailed += OnImportPackageFailed;
        AssetDatabase.importPackageCancelled += OnImportPackageCancelled;
    }

    private void OnDisable() {
        AssetDatabase.importPackageStarted -= OnImportPackageStarted;
        AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
        AssetDatabase.importPackageFailed -= OnImportPackageFailed;
        AssetDatabase.importPackageCancelled -= OnImportPackageCancelled;
    }

    public override void OnInspectorGUI() {
        {
            EditorGUILayout.LabelField(AssetName, EditorStyles.boldLabel);
            DrawUILine(Color.gray, 1, 0);
            EditorGUILayout.LabelField($"Version {_readme.FlatKitVersion}", EditorStyles.miniLabel);
            EditorGUILayout.Separator();
        }

        if (GUILayout.Button("Documentation")) {
            OpenDocumentation();
        }

        {
            if (_showingVersionMessage) {
                EditorGUILayout.Space(20);

                if (_versionLatest == null) {
                    EditorGUILayout.HelpBox($"Checking the latest version...", MessageType.None);
                } else {
                    var local = Version.Parse(_readme.FlatKitVersion);
                    var remote = Version.Parse(_versionLatest);
                    if (local >= remote) {
                        EditorGUILayout.HelpBox($"You have the latest version! {_readme.FlatKitVersion}.",
                            MessageType.Info);
                    } else {
                        EditorGUILayout.HelpBox(
                            $"Update needed. " +
                            $"The latest version is {_versionLatest}, but you have {_readme.FlatKitVersion}.",
                            MessageType.Warning);

#if !UNITY_2020_3_OR_NEWER
                        EditorGUILayout.HelpBox(
                            $"Please update Unity to 2020.3 or newer to get the latest " +
                            $"version of Flat Kit.",
                            MessageType.Error);
#endif
                    }
                }
            }

            if (GUILayout.Button("Check for updates")) {
                _showingVersionMessage = true;
                _versionLatest = null;
                CheckVersion();
            }

            if (_showingVersionMessage) {
                EditorGUILayout.Space(20);
            }
        }

        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Open support ticket")) {
                OpenSupportTicket();
            }

            if (GUILayout.Button("Copy debug info")) {
                CopyDebugInfoToClipboard();
            }

            GUILayout.EndHorizontal();
        }

        {
            if (!_readme.FlatKitInstalled) {
                EditorGUILayout.Separator();
                DrawUILine(Color.yellow, 1, 0);

                EditorGUILayout.HelpBox(
                    $"Before using {AssetName} you need to unpack it depending on your " + "project's Render Pipeline.",
                    MessageType.Warning);

                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField($"Unpack {AssetName} for", EditorStyles.label);
                if (GUILayout.Button("URP")) {
                    UnpackFlatKitUrp();
                }

                if (GUILayout.Button("Built-in RP")) {
                    UnpackFlatKitBuiltInRP();
                }

                GUILayout.EndHorizontal();
                DrawUILine(Color.yellow, 1, 0);

                return;
            }
        }

        {
            if (!string.IsNullOrEmpty(_readme.PackageManagerError)) {
                EditorGUILayout.Separator();
                DrawUILine(Color.yellow, 1, 0);
                EditorGUILayout.HelpBox($"Package Manager error: {_readme.PackageManagerError}", MessageType.Warning);
                DrawUILine(Color.yellow, 1, 0);
            }
        }

        {
            DrawUILine(Color.gray, 1, 20);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Configure project for", EditorStyles.label);

            if (GUILayout.Button("URP", EditorStyles.miniButtonLeft)) {
                ConfigureUrp();
            }

            if (GUILayout.Button("Built-in RP", EditorStyles.miniButtonLeft)) {
                ConfigureBuiltIn();
            }

            GUILayout.EndHorizontal();
        }

        {
            DrawUILine(Color.gray, 1, 20);
            EditorGUILayout.LabelField("Package Manager", EditorStyles.label);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear cache")) {
                ClearPackageCache();
            }

            if (GUILayout.Button($"Select {AssetName}")) {
                OpenPackageManager();
            }

            GUILayout.EndHorizontal();

            if (_showingClearCacheMessage) {
                if (_cacheClearedSuccessfully) {
                    EditorGUILayout.HelpBox(
                        $"Successfully removed cached packages. \n" +
                        $"Please re-download {AssetName} in the Package Manager.", MessageType.Info);
                } else {
                    EditorGUILayout.HelpBox($"Could not find or clear package cache.", MessageType.Warning);
                }
            }

            EditorGUILayout.Separator();
        }

        {
            DrawUILine(Color.gray, 1, 20);
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Debug info", EditorStyles.miniBoldLabel);

            GUILayout.BeginVertical();
            if (GUILayout.Button("Copy", EditorStyles.miniButtonLeft)) {
                CopyDebugInfoToClipboard();
                // EditorUtility.DisplayDialog(AssetName, "Debug info copied to the clipboard.", "OK");
            }

            if (EditorGUIUtility.systemCopyBuffer == GetDebugInfoString()) {
                EditorGUILayout.LabelField("Copied!", EditorStyles.miniLabel);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            var debugInfo = GetDebugInfo();
            foreach (var s in debugInfo) {
                EditorGUILayout.LabelField($"    " + s, EditorStyles.miniLabel);
            }

            EditorGUILayout.Separator();
        }
    }

    private string[] GetDebugInfo() {
        var info = new List<string> {
            $"{AssetName} version {_readme.FlatKitVersion}",
            $"Unity {_readme.UnityVersion}",
            $"Dev platform: {Application.platform}",
            $"Target platform: {EditorUserBuildSettings.activeBuildTarget}",
            $"URP installed: {_readme.UrpInstalled}, version {_readme.UrpVersionInstalled}",
            $"Render pipeline: {Shader.globalRenderPipeline}"
        };

        var qualityConfig = QualitySettings.renderPipeline == null ? "N/A" : QualitySettings.renderPipeline.name;
        info.Add($"Quality config: {qualityConfig}");

        var graphicsConfig = GraphicsSettings.currentRenderPipeline == null
            ? "N/A"
            : GraphicsSettings.currentRenderPipeline.name;
        info.Add($"Graphics config: {graphicsConfig}");

        return info.ToArray();
    }

    private string GetDebugInfoString() {
        string[] info = GetDebugInfo();
        return String.Join("\n",info);
    }

    private void CopyDebugInfoToClipboard() {
        EditorGUIUtility.systemCopyBuffer = GetDebugInfoString();
    }

    private void OpenPackageManager() {
#if UNITY_2020_1_OR_NEWER
        Client.Resolve();
#endif

        const string packageName = "Flat Kit: Toon Shading and Water";
        UnityEditor.PackageManager.UI.Window.Open(packageName);

        /*
        var request = Client.Add(packageName);
        while (!request.IsCompleted) System.Threading.Tasks.Task.Delay(100);
        if (request.Status != StatusCode.Success) Debug.LogError("Cannot import asset: " + request.Error.message);
        */
    }

    private void ClearPackageCache() {
        string path = string.Empty;
        if (Application.platform == RuntimePlatform.OSXEditor) {
            path = "~/Library/Unity/Asset Store-5.x/Dustyroom/";
        }

        if (Application.platform == RuntimePlatform.LinuxEditor) {
            path = "~/.local/share/unity3d/Asset Store-5.x/Dustyroom/";
        }

        if (Application.platform == RuntimePlatform.WindowsEditor) {
            // This wouldn't understand %APPDATA%.
            path = Application.persistentDataPath.Substring(0,
                       Application.persistentDataPath.IndexOf("AppData", StringComparison.Ordinal)) +
                   "/AppData/Roaming/Unity/Asset Store-5.x/Dustyroom";
        }

        if (path == string.Empty) return;

        _cacheClearedSuccessfully |= FileUtil.DeleteFileOrDirectory(path);
        _showingClearCacheMessage = true;
    }

    private void UnpackFlatKitUrp() {
        string path = AssetDatabase.GUIDToAssetPath(UnityPackageUrpGuid.ToString());
        if (path == null) {
            Debug.LogError($"<b>[{AssetName}]</b> Could not find the URP package.");
        } else {
            AssetDatabase.ImportPackage(path, false);
        }
    }

    private void UnpackFlatKitBuiltInRP() {
        string path = AssetDatabase.GUIDToAssetPath(UnityPackageBuiltInGuid.ToString());
        if (path == null) {
            Debug.LogError($"<b>[{AssetName}]</b> Could not find the Built-in RP package.");
        } else {
            AssetDatabase.ImportPackage(path, false);
        }
    }

    private void OnImportPackageStarted(string packageName) { }

    private void OnImportPackageCompleted(string packageName) {
        _readme.Refresh();
        Repaint();
        EditorUtility.SetDirty(this);
    }

    private void OnImportPackageFailed(string packageName, string errorMessage) {
        Debug.LogError($"<b>[{AssetName}]</b> Failed to unpack {packageName}: {errorMessage}.");
    }

    private void OnImportPackageCancelled(string packageName) {
        Debug.LogError($"<b>[{AssetName}]</b> Cancelled unpacking {packageName}.");
    }

    private void ConfigureUrp() {
        string path = AssetDatabase.GUIDToAssetPath(UrpPipelineAssetGuid.ToString());
        if (path == null) {
            Debug.LogError($"[{AssetName}] Couldn't find the URP pipeline asset. " +
                           "Have you unpacked the URP package?");
            return;
        }

        var pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
        if (pipelineAsset == null) {
            Debug.LogError($"[{AssetName}] Couldn't load the URP pipeline asset.");
            return;
        }

        Debug.Log(
            $"<b>[{AssetName}]</b> Set the render pipeline asset in the Graphics settings to the {AssetName} example.");
        GraphicsSettings.renderPipelineAsset = pipelineAsset;
        GraphicsSettings.defaultRenderPipeline = pipelineAsset;

        ChangePipelineAssetAllQualityLevels(pipelineAsset);
    }

    private void ConfigureBuiltIn() {
        GraphicsSettings.renderPipelineAsset = null;
        Debug.Log($"<b>[{AssetName}]</b> Cleared the render pipeline asset in the Graphics settings.");

        ChangePipelineAssetAllQualityLevels(null);
    }

    private void ChangePipelineAssetAllQualityLevels(RenderPipelineAsset pipelineAsset) {
        var originalQualityLevel = QualitySettings.GetQualityLevel();

        var logString = $"<b>[{AssetName}]</b> Set the render pipeline asset for the quality levels:";

        for (int i = 0; i < QualitySettings.names.Length; i++) {
            logString += $"\n\t{QualitySettings.names[i]}";
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipelineAsset;
        }

        Debug.Log(logString);

        QualitySettings.SetQualityLevel(originalQualityLevel, false);
    }

    private void CheckVersion() {
        NetworkManager.GetVersion(version => { _versionLatest = version; });
    }

    private void OpenSupportTicket() {
        Application.OpenURL("https://github.com/Dustyroom/flat-kit-doc/issues/new/choose");
    }

    private void OpenDocumentation() {
        Application.OpenURL("https://flatkit.dustyroom.com");
    }

    private static void DrawUILine(Color color, int thickness = 2, int padding = 10) {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2f;
        r.x -= 2;
        EditorGUI.DrawRect(r, color);
    }
}
}