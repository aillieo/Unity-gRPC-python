using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AillieoUtils
{
    internal class GRPCSettings : ScriptableObject
    {
        public int port;

        public string pythonScript;

        public string protoPath;
        public string genCSProtoPath;
        public string genPythonProtoPath;



        internal class Provider : SettingsProvider
        {
            private static new readonly string settingsPath = "AillieoUtils/UniPy";
            private static new readonly string[] keywords = new string[] { "Aillieo", "UniPy", "gRPC", "RPC", "proto", "python" };
            private static readonly string projectSettingAssetsFolder = "ProjectSettings/";
            private GRPCSettings asset;

            private Provider()
                : base(settingsPath, SettingsScope.Project, keywords)
            {
            }

            [SettingsProvider]
            public static SettingsProvider RegisterSettingsProvider()
            {
                return new Provider();
            }

            public override void OnGUI(string search)
            {
                base.OnGUI(search);

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("gRPC plugins:", EditorStyles.boldLabel);

                bool hasPlugin = PluginChecker.HasPluginInstalled();
                if (hasPlugin)
                {
                    EditorGUILayout.LabelField("Plugin installed.");
                }
                else
                {
                    if (GUILayout.Button("Download"))
                    {
                        PluginChecker.CheckAndAskToDownload();
                    }
                }

                EditorGUILayout.EndVertical();

                this.asset = LoadAsset();

                EditorGUILayout.BeginVertical("box");

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.LabelField("Python server:", EditorStyles.boldLabel);

                EditorGUILayout.IntField(nameof(asset.port), asset.port);

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("gRPC/proto path:", EditorStyles.boldLabel);

                EditorGUILayout.TextField(nameof(asset.genCSProtoPath), asset.genCSProtoPath);
                EditorGUILayout.TextField(nameof(asset.genCSProtoPath), asset.genCSProtoPath);

                if (EditorGUI.EndChangeCheck())
                {
                    SaveAsset(this.asset);
                }

                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    if (GUILayout.Button("Create sh"))
                    {

                    }
                }
                else if(Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (GUILayout.Button("Create cmd"))
                    {

                    }
                }

                EditorGUILayout.EndVertical();
            }

            private static GRPCSettings LoadAsset()
            {
                var path = Path.Combine(projectSettingAssetsFolder, $"{nameof(GRPCSettings)}.asset");
                UnityEngine.Object[] objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                GRPCSettings asset = null;
                if (objs != null && objs.Length > 0)
                {
                    asset = objs[0] as GRPCSettings;
                }

                if (asset == null)
                {
                    asset = CreateInstance<GRPCSettings>();
                }

                asset.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontUnloadUnusedAsset;

                return asset;
            }

            private static void SaveAsset(ScriptableObject asset)
            {
                var path = Path.Combine(projectSettingAssetsFolder, $"{nameof(GRPCSettings)}.asset");

                Directory.CreateDirectory(projectSettingAssetsFolder);
                InternalEditorUtility.SaveToSerializedFileAndForget(
                    new UnityEngine.Object[] { asset },
                    path,
                    true);
            }

            private static void DrawPathAndButton(string label, string button, Action<string> onSelected)
            {

            }

            [MenuItem("AillieoUtils/UniPy/Settings")]
            private static void OpenProjectSettings()
            {
                SettingsService.OpenProjectSettings(settingsPath);
            }
        }
    }
}
