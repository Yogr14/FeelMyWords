using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace Services.Korolitics.DeveloperConsole
{
    public class KoroliticsDevConsoleWindow : EditorWindow
    {
        private ContentWindow _currentContentWindow;
        private Config _config;
        private HttpClient _httpClient;
        private bool _authentificationPassed;

        [MenuItem("Korolitics/Developer Console")]
        public static void ShowWindow()
        {
            var window = GetWindow<KoroliticsDevConsoleWindow>("Korolitics Dev Console");
        }
        private void OnEnable()
        {
            if(_httpClient == null) _httpClient = new HttpClient();
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            GUIStyle bgStye = new GUIStyle();
            bgStye.normal.background = MakeTex(1, 1, new Color(0.0f, 0.0f, 0.0f, 0.1f));
            EditorGUILayout.BeginVertical(bgStye, GUILayout.Width(position.width * 0.2f), GUILayout.ExpandHeight(true));
            DrawSideBar();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawContentArea();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSideBar()
        {
            GUILayout.Space(5);
            if (GUILayout.Button("Account"))
            {
                _currentContentWindow = new Account(_httpClient, _config, _authentificationPassed);
            }
            if(!_authentificationPassed) GUI.enabled = false;
            if (GUILayout.Button("Database tables"))
            {
                _currentContentWindow = new DBTables(_httpClient, _config, _authentificationPassed);
            }
            GUI.enabled = true;
        }
        private void DrawContentArea()
        {
            if(_config == null) _config = Resources.Load<Config>("ConfigFile");
            if(_currentContentWindow == null) _currentContentWindow = new Account(_httpClient, _config, _authentificationPassed);

            if(_currentContentWindow is not Account)
            {
                if(_config == null)
                {
                    EditorGUILayout.HelpBox("Config file not found!", MessageType.Error);
                    return;
                }
                else if(!_authentificationPassed)
                {
                    EditorGUILayout.HelpBox("You are not authentificated. Go to Account tab.", MessageType.Warning);
                    return;
                }
            }
            _currentContentWindow.Draw();
            if(_currentContentWindow is Account account)
            {
                _authentificationPassed = account.AuthentificationPassed;
            }
        }
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width*height];

            for(int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}
