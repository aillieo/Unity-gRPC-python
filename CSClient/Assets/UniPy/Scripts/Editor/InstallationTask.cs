using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;
using System.Diagnostics;

namespace AillieoUtils.UniPy
{
    internal class InstallationTask
    {

        private readonly string remoteURL;
        private readonly string localDirectory;
        private readonly string filename;
        private readonly string cachePath;

        public TaskPhase phase { get; private set; } = TaskPhase.NotInitialized;

        private UnityWebRequestAsyncOperation asyncOperation;

        public enum TaskPhase
        {
            NotInitialized = 0,
            Download,
            Unzip,
            Faulted,
            Completed,
        }

        public InstallationTask(string remoteURL, string localDirectory)
        {
            this.remoteURL = remoteURL;
            this.localDirectory = localDirectory;

            this.filename = Path.GetFileName(remoteURL);
            this.cachePath = Path.Combine(Application.temporaryCachePath, filename);
        }

        public void Start()
        {
            if (phase == TaskPhase.NotInitialized)
            {
                this.CheckAndDownload();
            }
        }

        public void Cancel()
        {
            if (asyncOperation != null)
            {
                asyncOperation.webRequest.Abort();
            }

            this.phase = TaskPhase.Faulted;
        }

        public float GetProgress()
        {
            float downloadWeight = 0.9f;

            switch (this.phase)
            {
                case TaskPhase.NotInitialized:
                    return 0;
                case TaskPhase.Download:
                    return asyncOperation.progress * downloadWeight;
                case TaskPhase.Unzip:
                    return downloadWeight;
                case TaskPhase.Completed:
                    return 1;
                default:
                    return 0;
            }
        }

        private void CheckAndDownload()
        {
            if (this.asyncOperation != null)
            {
                return;
            }

            if (!File.Exists(cachePath))
            {
                this.phase = TaskPhase.Download;

                UnityEngine.Debug.Log("Begin download: " + remoteURL);
                var unityWebRequest = UnityWebRequest.Get(remoteURL);
                unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                this.asyncOperation = unityWebRequest.SendWebRequest();
                this.asyncOperation.completed += _ =>
                {
                    if (unityWebRequest.result == UnityWebRequest.Result.Success)
                    {
                        byte[] bytes = unityWebRequest.downloadHandler.data;
                        string cacheDir = Path.GetDirectoryName(cachePath);

                        if (!Directory.Exists(cacheDir))
                        {
                            Directory.CreateDirectory(cacheDir);
                        }

                        File.WriteAllBytes(cachePath, bytes);
                        UnityEngine.Debug.Log("Download succeed: " + cachePath);

                        CheckAndUnzip();
                    }
                    else
                    {
                        this.phase = TaskPhase.Faulted;
                        UnityEngine.Debug.LogError(unityWebRequest.error);
                    }
                };
            }
            else
            {
                CheckAndUnzip();
            }
        }

        private void CheckAndUnzip()
        {
            if (File.Exists(cachePath))
            {
                phase = TaskPhase.Unzip;

                string command = default;
                string commandArgs = default;

                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                        command = "unzip";
                        commandArgs = $"-o {cachePath} -d {localDirectory}";
                        break;
                    case RuntimePlatform.WindowsEditor:
                        command = "tar";
                        commandArgs = $"-xf {cachePath} -C {localDirectory}";
                        break;
                    default:
                        throw new PlatformNotSupportedException();
                }

                UnityEngine.Debug.Log("Begin unzip: " + cachePath + " -> " + localDirectory);

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = command;
                startInfo.Arguments = commandArgs;

                startInfo.UseShellExecute = true;
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process process = new Process();

                process.StartInfo = startInfo;
                process.EnableRaisingEvents = true;

                process.ErrorDataReceived += (s, e) => UnityEngine.Debug.LogError(e.Data);
                process.Exited += (s, e) =>
                {
                    if (process.ExitCode == 0)
                    {
                        UnityEngine.Debug.Log("Unzip succeed");
                        phase = TaskPhase.Completed;
                    }
                    else
                    {
                        phase = TaskPhase.Faulted;
                    }

                    process.Close();
                    process.Dispose();
                };

                process.Start();
            }
            else
            {
                phase = TaskPhase.Faulted;
                UnityEngine.Debug.LogError("Zip file not found");
            }
        }
    }
}
