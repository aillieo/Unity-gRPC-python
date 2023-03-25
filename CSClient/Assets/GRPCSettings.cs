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


        internal class Provider : SettingsProvider
        {
            private static readonly string projectSettingAssetsFolder = "ProjectSettings/";
            private ScriptableObject asset;
            private Editor editor;

            private Provider()
                : base("AillieoUtils/gRPC", SettingsScope.Project)
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

                this.asset = LoadAsset();

                if (this.editor == null || this.editor.target != this.asset)
                {
                    this.editor = Editor.CreateEditor(this.asset);
                }

                EditorGUI.BeginChangeCheck();

                this.editor.OnInspectorGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    SaveAsset(this.asset);
                }
            }

            private static ScriptableObject LoadAsset()
            {
                var path = Path.Combine(projectSettingAssetsFolder, $"{nameof(GRPCSettings)}.asset");
                UnityEngine.Object[] objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                ScriptableObject asset = null;
                if (objs != null && objs.Length > 0)
                {
                    asset = objs[0] as ScriptableObject;
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
        }
    }
}
