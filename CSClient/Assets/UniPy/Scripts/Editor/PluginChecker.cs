using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;

namespace AillieoUtils.UniPy
{
    [InitializeOnLoad]
    internal class PluginChecker
    {
        private static readonly string sessionKey = "gRPCPluginCheck";
        private static readonly string downloadURL = "https://packages.grpc.io/archive/2021/09/4cb907457bcb11b19f9338ddd965aa5fef03b60f-31f03539-d36d-4476-b673-59110f18dbfe/csharp/grpc_unity_package.2.41.0-dev202109150953.zip";
        private static readonly string pluginDirectory = "Plugins/Grpc.Core/runtimes";
        private static readonly string pluginFilename = "grpc";

        static PluginChecker()
        {
            if (!SessionState.GetBool(sessionKey, false))
            {
                SessionState.SetBool(sessionKey, true);

                CheckAndAskToDownload();
            }
        }

        public static bool HasPluginInstalled()
        {
            bool match(string assetPath)
            {
                string dir = Path.GetDirectoryName(assetPath).Replace("\\", "/");
                string file = Path.GetFileName(assetPath);
                return dir.IndexOf(pluginDirectory, StringComparison.OrdinalIgnoreCase) >= 0
                    && file.IndexOf(pluginFilename, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            PluginImporter[] importers = PluginImporter.GetImporters(EditorUserBuildSettings.activeBuildTarget);
            return importers.Any(i => match(i.assetPath));
        }

        [MenuItem("AillieoUtils/UniPy/Check gRPG plugin")]
        public static void CheckAndAskToDownload()
        {
            if (HasPluginInstalled())
            {
                return;
            }

            int id = EditorUtility.DisplayDialogComplex("Message", "gRPG plugin not fully installed", "Download and install", "Ingore", "Download");
            switch (id)
            {
                case 0:
                    DownloadAndInstall();
                    break;
                case 2:
                    Application.OpenURL(downloadURL);
                    break;
                default:
                    break;
            }
        }

        private static void DownloadAndInstall()
        {
            if (installationTask == null)
            {
                installationTask = new InstallationTask(downloadURL, Path.Combine(Application.dataPath, "UniPy"));
                installationTask.Start();

                EditorApplication.update -= OnUpdate;
                EditorApplication.update += OnUpdate;
            }
        }

        private static InstallationTask installationTask;

        private static void OnUpdate()
        {
            if (installationTask != null)
            {
                if (installationTask.phase == InstallationTask.TaskPhase.Completed || installationTask.phase == InstallationTask.TaskPhase.Faulted)
                {
                    installationTask = null;
                    EditorApplication.update -= OnUpdate;
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    bool cancelRequested = EditorUtility.DisplayCancelableProgressBar("Installing gRPG plugin", $"Current phase: {installationTask.phase}", installationTask.GetProgress());
                    if (cancelRequested)
                    {
                        installationTask.Cancel();
                    }
                }
            }
            else
            {
                EditorApplication.update -= OnUpdate;
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
