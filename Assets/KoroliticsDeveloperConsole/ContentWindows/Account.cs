using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Services.Korolitics.DeveloperConsole
{
    public class Account : ContentWindow
    {
        private bool isHandlingRequest;
        public Account(HttpClient httpClient, Config config, bool authentificationPassed) : base(httpClient, config, authentificationPassed)
        {
        }
        internal override void Draw()
        {
            var labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };
            GUILayout.Label("Acount Settings", labelStyle);

            GUILayout.BeginVertical("box");
            GUILayout.Label("API Url:");
            EditorGUI.BeginChangeCheck();
            Config.ApiUrl = GUILayout.TextField(Config.ApiUrl);
            if (EditorGUI.EndChangeCheck())
            {
                if(Config.ApiUrl.StartsWith("http://")) Config.ApiUrl = Config.ApiUrl.Remove(0, 7);
                if(Config.ApiUrl.StartsWith("https://")) Config.ApiUrl = Config.ApiUrl.Remove(0, 8);
                EditorUtility.SetDirty(Config);
            }

            GUILayout.Label("Developer role name:");
            EditorGUI.BeginChangeCheck();
            Config.DeveloperRoleName = GUILayout.TextField(Config.DeveloperRoleName);
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(Config);
            
            GUILayout.Label("Developer role password:");
            EditorGUI.BeginChangeCheck();
            Config.DeveloperRolePassword = GUILayout.TextField(Config.DeveloperRolePassword);
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(Config);

            GUI.enabled = (!string.IsNullOrEmpty(Config.ApiUrl) && !string.IsNullOrEmpty(Config.DeveloperRoleName) && !string.IsNullOrEmpty(Config.DeveloperRolePassword));
            if(!GUI.enabled) EditorGUILayout.HelpBox("Fill the credentials fields", MessageType.Warning);
            if(isHandlingRequest) GUI.enabled = false;
            if (GUILayout.Button("Check connection"))
            {
                CheckConnection();
            }
            GUI.enabled = true;
            GUILayout.EndVertical();

            if(AuthentificationPassed)
            {
                EditorGUILayout.HelpBox("You are authenticated", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("You are not authenticated", MessageType.Warning);
            }
        }

        private async void CheckConnection()
        {
            // Check connection logic here
            Debug.Log("Checking connection...");
            isHandlingRequest = true;
            var t = UserExist(Config.ApiUrl, Config.DeveloperRoleName, Config.DeveloperRolePassword);

            await Task.Run(async () =>
            {
                bool userExists = await t;
                if (userExists)
                {
                    Debug.Log("User exists");
                    AuthentificationPassed = true;
                }
                else
                {
                    Debug.LogError("User does not exist");
                    AuthentificationPassed = false;
                }
                isHandlingRequest = false;
            });
        } 
        private async Task<bool> ApiAvailable(string apiUrl)
        {
            try
            {
                var ping = new System.Net.NetworkInformation.Ping();

                var task = ping.SendPingAsync(apiUrl, 500);

                var result = await task;

                return result.Status == System.Net.NetworkInformation.IPStatus.Success;

                //var uri = $"https://{apiUrl}";
                //var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
                //return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> UserExist(string apiUrl, string developerRoleName, string developerRolePassword)
        {
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{developerRoleName}:{developerRolePassword}"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            try
            {
                // Создаем JSON объект с именем пользователя
                var requestBody = new StringContent(
                    JsonConvert.SerializeObject(new { p_username = Config.DeveloperRoleName }),
                    Encoding.UTF8,
                    "application/json"
                    );
                    
                // Отправляем POST запрос к API
                string url = $"https://{apiUrl}/rpc/check_postgres_role_credentials";
                HttpResponseMessage response = await HttpClient.PostAsync(url, requestBody);
                response.EnsureSuccessStatusCode(); // Проверяем, что ответ успешный

                // Получаем и возвращаем JSON строку из ответа
                var respond = await response.Content.ReadAsStringAsync();
                var responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(respond);
                return responseDict["success"] == "true";           
            }
            catch(Exception e)
            {
                Debug.LogError($"Error checking role existence: {e.Message}");
                return false;
            }
        }
    }
}
