using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AillieoUtils
{
    public static class PythonHelper
    {
        [MenuItem("AillieoUtils/gRPC/Start python server")]
        private static void StartPythonServer()
        {
            RunPython("main.py");
        }

        public static void RunPython(string script)
        {
            string directory = Path.GetDirectoryName(script);
            string filemame = Path.GetFileName(script);

            string executableFile = FindPython();

            if (string.IsNullOrEmpty(executableFile))
            {
                UnityEngine.Debug.LogError("找不到匹配的Python目录");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = executableFile;
            startInfo.Arguments = filemame;

            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = false;

            startInfo.WorkingDirectory = directory;
            startInfo.UseShellExecute = false;

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        UnityEngine.Debug.Log(result);
                    }
                }

                using (StreamReader reader = process.StandardError)
                {
                    string result = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        UnityEngine.Debug.LogError(result);
                    }
                }

                process.WaitForExit();
                process.Close();
            }
        }

        private static string executableName
        {
            get
            {
                switch (Application.platform)
                {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "python.exe";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "python";
                default:
                    throw new NotImplementedException();
                }
            }
        }

        private static char environmentVariableSeparator
        {
            get
            {
                switch (Application.platform)
                {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return ';';
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return ':';
                default:
                    throw new NotImplementedException();
                }
            }
        }

        private static string environmentVariableName
        {
            get
            {
                switch (Application.platform)
                {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Path";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "PATH";
                default:
                    throw new NotImplementedException();
                }
            }
        }

        internal static string FindPython()
        {
            foreach (EnvironmentVariableTarget target in Enum.GetValues(typeof(EnvironmentVariableTarget)))
            {
                string pathValue = Environment.GetEnvironmentVariable(environmentVariableName, target);
                if (!string.IsNullOrEmpty(pathValue))
                {
                    string[] pathEntries = pathValue.Split(new char[] { environmentVariableSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var path in pathEntries)
                    {
                        try
                        {
                            string fullPath = Path.Combine(path, executableName);

                            if (File.Exists(fullPath))
                            {
                                return fullPath;
                            }
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.Log(e);
                        }
                    }
                }
            }

            return null;
        }
    }
}
