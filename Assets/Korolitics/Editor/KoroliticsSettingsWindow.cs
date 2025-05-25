using Services.Korolitics.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Services.Korolitics.Editor
{
    public class KoroliticsSettingsWindow : EditorWindow
    {
        private KoroliticsConfig _configFile;

        [MenuItem("Korolitics/Settings")]
        public static void ShowWindow()
        {
            GetWindow<KoroliticsSettingsWindow>("Korolitics Settings");
        }
        private void OnEnable()
        {
            _configFile = Resources.Load<KoroliticsConfig>("KoroliticsConfigFile");
            if(_configFile == null) Debug.LogError("Korolitics Config File not found!");
        }
        private void OnGUI()
        {
            var labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };
            if(_configFile == null)
            {

                GUILayout.Label("Korolitics Settings", labelStyle);
                EditorGUILayout.HelpBox("Korolitics Config File not found! Add one to continue", MessageType.Error);
                return;
            }
            GUILayout.Label("Korolitics Settings", labelStyle);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Server Api URL", EditorStyles.label);
            EditorGUI.BeginChangeCheck();
            _configFile.ApiUrl = GUILayout.TextField(_configFile.ApiUrl, GUILayout.Width(300f));
            if (EditorGUI.EndChangeCheck())
            {
                if(_configFile.ApiUrl.StartsWith("http://")) _configFile.ApiUrl = _configFile.ApiUrl.Remove(0, 7);
                if(_configFile.ApiUrl.StartsWith("https://")) _configFile.ApiUrl = _configFile.ApiUrl.Remove(0, 8);
                EditorUtility.SetDirty(_configFile);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Client role name", EditorStyles.label);
            EditorGUI.BeginChangeCheck();
            _configFile.ClientRoleName = GUILayout.TextField(_configFile.ClientRoleName,  GUILayout.Width(300f));
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_configFile);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Client role password", EditorStyles.label);
            EditorGUI.BeginChangeCheck();
            _configFile.ClientRolePassword = GUILayout.TextField(_configFile.ClientRolePassword,  GUILayout.Width(300f));
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_configFile);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Client id", EditorStyles.label);
            GUI.enabled = false;
            GUILayout.TextField(_configFile.ClientID,  GUILayout.Width(300f));
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("App Name", EditorStyles.label);
            EditorGUI.BeginChangeCheck();
            _configFile.AppName = GUILayout.TextField(_configFile.AppName,  GUILayout.Width(300f));
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_configFile);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Switching toggle will cause a recompilation of the project!", MessageType.Warning);
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Enable GPS Collection", "Will add scripting define symbol into your project settings"), EditorStyles.label);
            EditorGUI.BeginChangeCheck();
            _configFile.EnableGPSCollection = EditorGUILayout.Toggle(_configFile.EnableGPSCollection);
            if (EditorGUI.EndChangeCheck())
            {
                if (_configFile.EnableGPSCollection)
                {
                    AddDefineSymbol("KOROLITICS_GPS_ACCESS");
                }
                else
                {
                    RemoveDefineSymbol("KOROLITICS_GPS_ACCESS");
                }
                EditorUtility.SetDirty(_configFile);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Session legth update frequency", EditorStyles.label);
            EditorGUI.BeginChangeCheck();
            _configFile.UpdateSessionLengthFrequency = EditorGUILayout.FloatField(_configFile.UpdateSessionLengthFrequency);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_configFile);
            }
            GUILayout.EndHorizontal();

            if(_configFile.DebugMode) EditorGUILayout.HelpBox("Uncheck before publishing!", MessageType.Error);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Debug mode", EditorStyles.label);
            EditorGUI.BeginChangeCheck();
            _configFile.DebugMode = EditorGUILayout.Toggle(_configFile.DebugMode);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_configFile);
            }
            GUILayout.EndHorizontal();
        }
        

        private void AddDefineSymbol(string define)
        {
            // Get the current build target
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(buildTarget));
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

            if (System.Array.IndexOf(defines, define) == -1)
            {
                System.Array.Resize(ref defines, defines.Length + 1);
                defines[defines.Length - 1] = define;
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
            }
            else
            {
                Debug.Log(define + " already exists in scripting defines.");
            }
        }
        private void RemoveDefineSymbol(string define)
        {
            // Get the current build target
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(buildTarget));
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

            int removedCopies = 0;
            for(int i = 0; i < defines.Length; i++)
            {
                if(defines[i] == define)
                {
                    removedCopies ++;
                }
                else if(removedCopies > 0)
                {
                    defines[i - removedCopies] = defines[i];
                }
            }
            if(removedCopies > 0)
            {
                System.Array.Resize(ref defines, defines.Length - removedCopies);
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
            }
            else
            {
                Debug.Log(define + " does not exist in scripting defines.");
            }
        }
    }
}
