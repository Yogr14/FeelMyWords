using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Net.Http.Headers;

namespace Services.Korolitics.Core
{
    public class KorolitcsManager : MonoBehaviour
    {
        private const string c_session_length_event = "session_length";
        private static KoroliticsConfig _config;
        private static string _clientLocation;
        private static string _sessionID;

        private static bool _initialized = false;
        private static KorolitcsManager _instance;
        private HttpClient _httpClient;
        private UnityWebRequest _eventRequest;
        private UnityWebRequest _sTimerRequest;

        public static IEnumerator Initialize()
        {
            if (!_initialized)
            {
                _config = Resources.Load<KoroliticsConfig>("KoroliticsConfigFile");
                if(_config == null) {
                    Debug.LogError("Korolitics Config File not found! Initialization failed.");
                    yield break;
                }
                else
                {
                    if(!_config.IsValid())
                    {
                        Debug.LogError("Korolitics Config File is not valid! Initialization failed.");
                        yield break;
                    }
                }
                #if KOROLITICS_GPS_ACCESS
                yield return LoadClientLocation();
                #else
                _clientLocation = "unknown";
                #endif
                _sessionID = DateTime.Now.GetHashCode().ToString() + SystemInfo.deviceUniqueIdentifier.Substring(0, SystemInfo.deviceUniqueIdentifier.Length/2);
                _instance = new GameObject("KoroliticsManager").AddComponent<KorolitcsManager>();
                DontDestroyOnLoad(_instance.gameObject);
                _instance.LocalInitialize();
                _initialized = true;
                if(_config.DebugMode) Debug.Log("Korolitics initialized.");
            }
            else
            {
                yield return null;
                if(_config.DebugMode) Debug.LogWarning("Korolitics is already initialized.");
            }
        }
        private void LocalInitialize()
        {
            _httpClient = new HttpClient();
            StartCoroutine(PlayTimer());
        }
        private void OnDisable()
        {
            _httpClient?.Dispose();
            if(_eventRequest != null) _eventRequest.Dispose();
            if(_sTimerRequest != null) _sTimerRequest.Dispose();
        }

        public static void ReportEvent(string eventName, Dictionary<string, object> eventData = null)
        {   
            if(!_initialized)
            {
                Debug.LogError("Korolitics is not initialized. Please call Initialize() before reporting events.");
                return;
            }
            if(eventName.Equals(c_session_length_event))
            {
                Debug.LogError("Event name 'session_length' is reserved. Please use another name.");
                return;
            }
            if(_config.DebugMode) 
            {
                Debug.Log("Reporting event: \n" + GetEntryAsJson(eventName, ConvertDictionaryToJson(eventData)));
            }

            #if UNITY_WEBGL && !UNITY_EDITOR
            if(_config.DebugMode)  Debug.Log("WebGL platform detected. Sending event with coroutine.");
            string paramsJson = ConvertDictionaryToJson(eventData);
            _instance.StartCoroutine(_instance.ReportEventCoroutine(GetEntryAsJson(eventName, paramsJson)));
            #else
            if(_config.DebugMode)  Debug.Log("Non-WebGL platform or UnityEditor detected. Sending event with async.");
            _instance.ReportEventAsync(eventName, eventData);
            #endif
        }
        [ContextMenu("Ping")]
        public void Ping()
        {
            PingServer(null);
        }
        public static void PingServer(Action callback)
        {
            if(!_initialized)
            {
                Debug.LogError("Korolitics is not initialized. Please call Initialize() before reporting events.");
                return;
            }

            #if UNITY_WEBGL && !UNITY_EDITOR
            if(_config.DebugMode)  Debug.Log("WebGL platform detected. Sending event with coroutine.");
            _instance.StartCoroutine(_instance.ReportEventCoroutine(GetEntryAsJson("ping", null), callback));
            #else
            if(_config.DebugMode)  Debug.Log("Non-WebGL platform or UnityEditor detected. Sending event with async.");
            _instance.ReportEventAsync("ping", null, callback);
            #endif
        }
        /// <summary>
        /// This method will be called automatically according to config frequency value, but you may also call it manually 
        /// </summary>
        public static void UpdateSessionTimer()
        {   
            if(!_initialized)
            {
                Debug.LogError("Korolitics is not initialized. Please call Initialize() before reporting events.");
                return;
            }
            if(_config.DebugMode) 
            {
                Debug.Log("Updating session timer.");
            }

            #if UNITY_WEBGL  && !UNITY_EDITOR
            if(_config.DebugMode)  Debug.Log("WebGL platform detected. Sending event with coroutine.");
            _instance.StartCoroutine(_instance.UpdateSessionTimerCoroutine());
            #else
            if(_config.DebugMode)  Debug.Log("Non-WebGL platform or UnityEditor detected. Sending event with async.");
            _instance.UpdateSessionTimerAsync();
            #endif
        }
        private async void ReportEventAsync(string eventName, Dictionary<string, object> eventData, Action callback = null)
        {
            string paramsJson = ConvertDictionaryToJson(eventData);
            var metaData = new StringContent(GetEntryAsJson(eventName, paramsJson), Encoding.UTF8, "application/json");

            try
            {
                //authenticate
                var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_config.ClientRoleName}:{_config.ClientRolePassword}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var requestUrl = $"https://{_config.ApiUrl}/{_config.AppName}";
                if(_config.DebugMode) 
                {
                    Debug.Log("Sending event to: " + requestUrl);
                    Debug.Log("Event data: " + GetEntryAsJson(eventName, paramsJson));
                }
                var response = await _httpClient.PostAsync(requestUrl, metaData);
                

                // if table is not registered in the database
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Debug.LogError("Application is not registered in the database. Please register the application first.");
                }

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    if(_config.DebugMode)  Debug.Log("Event sent successfully: " + responseBody);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    if(_config.DebugMode) Debug.LogError($"Error: {errorResponse}; Status code: {response.StatusCode}");
                }
            }
            catch(Exception e)
            {
                if(_config.DebugMode) Debug.LogError($"Error while sending event:  {e.Message}");
            }

            metaData.Dispose();
            callback?.Invoke();
        }
        private IEnumerator ReportEventCoroutine(string eventData, Action callback = null)
        {   
            _eventRequest = new UnityWebRequest($"https://{_config.ApiUrl}/{_config.AppName}", "POST");

            // Convert the event data to a byte array
            byte[] bodyRaw = Encoding.UTF8.GetBytes(eventData);
            
            // Set the upload handler to send the JSON data
            _eventRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            
            // Set the download handler to receive the response
            _eventRequest.downloadHandler = new DownloadHandlerBuffer();
            
            // Set the content type header
            _eventRequest.SetRequestHeader("Content-Type", "application/json");

            //authenticate
            var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_config.ClientRoleName}:{_config.ClientRolePassword}"));
            _eventRequest.SetRequestHeader("Authorization", $"Basic {authToken}");
            
            // Send the request and wait for it to complete
            _eventRequest.SendWebRequest();
            yield return new WaitUntil(() => _eventRequest.isDone);

            // Check for errors
            if (_eventRequest.result == UnityWebRequest.Result.ConnectionError || 
                _eventRequest.result == UnityWebRequest.Result.ProtocolError)
            {
               if(_config.DebugMode) Debug.LogError("Error while sending event: " + _eventRequest.error);
            }
            else
            {
                // Log the successful response
                if(_config.DebugMode)  Debug.Log("Event sent successfully: " + _eventRequest.downloadHandler.text);
            }

            callback?.Invoke();
        }
        private async void UpdateSessionTimerAsync()
        {
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ClientRoleName}:{_config.ClientRolePassword}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            try
            {
                var data = new Dictionary<string, object>
                {
                    { "table_name", _config.AppName },
                    { "clientid", _config.ClientID },
                    { "debugmode", GetDebugModeState() },
                    { "appversion", Application.version },
                    { "sessionid", _sessionID },
                    { "platform", Application.platform },
                    { "devicetype", SystemInfo.deviceType },
                    { "devicemodel", SystemInfo.deviceModel },
                    { "language", Application.systemLanguage.ToString() },
                    { "location", _clientLocation },
                    { "session_length_seconds", Mathf.RoundToInt(Time.realtimeSinceStartup) }
                };
                // Создаем JSON объект с именем пользователя
                var requestBody = new StringContent(
                    ConvertDictionaryToJson(data),
                    Encoding.UTF8,
                    "application/json"
                    );
                    
                // Отправляем POST запрос к API
                string url = $"https://{_config.ApiUrl}/rpc/update_play_timer";
                HttpResponseMessage response = await _httpClient.PostAsync(url, requestBody);
                response.EnsureSuccessStatusCode(); // Проверяем, что ответ успешный        
            }
            catch(Exception e)
            {
                Debug.LogError($"Error updating session timer: {e.Message}");
            }
        }
        private IEnumerator UpdateSessionTimerCoroutine()
        {   
             _sTimerRequest = new UnityWebRequest($"https://{_config.ApiUrl}/rpc/update_play_timer", "POST");
            _sTimerRequest.SetRequestHeader("Content-Type", "application/json");
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ClientRoleName}:{_config.ClientRolePassword}"));
            _sTimerRequest.SetRequestHeader("Authorization", "Basic " + authToken);
            // Создаем объект с данными для отправки
            var data = new Dictionary<string, object>
            {
                { "table_name", _config.AppName },
                { "clientid", _config.ClientID },
                { "debugmode", GetDebugModeState() },
                { "appversion", Application.version },
                { "sessionid", _sessionID },
                { "platform", Application.platform },
                { "devicetype", SystemInfo.deviceType },
                { "devicemodel", SystemInfo.deviceModel },
                { "language", Application.systemLanguage.ToString() },
                { "location", _clientLocation },
                { "session_length_seconds", Mathf.RoundToInt(Time.realtimeSinceStartup) }
            };

            // Сериализуем объект в JSON и конвертируем в массив байтов
            string jsonData = ConvertDictionaryToJson(data);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            _sTimerRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            _sTimerRequest.downloadHandler = new DownloadHandlerBuffer();

            _sTimerRequest.SendWebRequest();
            yield return new WaitUntil(() => _sTimerRequest.isDone);

            if (_sTimerRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error updating session timer: {_sTimerRequest.error}");
            }
            else
            {
                Debug.Log("Session timer updated successfully.");
            }
        }
        private static string ConvertDictionaryToJson(Dictionary<string, object> dictionary)
        {
            if(dictionary == null)
            {
                return "{}";
            }
            string json = "{\n";
            int i = 0;
            foreach (var entry in dictionary)
            {
                json += string.Format("\"{0}\":\"{1}\"", entry.Key, entry.Value);
                i++;
                if(i < dictionary.Count)
                {
                    json += ",\n";
                }
                else
                {
                    json += "\n";
                }
            }

            json += "}";
            return json;
        }
        private static string GetEntryAsJson(string eventName, string paramsJson = null)
        {
            if(string.IsNullOrEmpty(paramsJson))
            {
                paramsJson = "{}";
            }
            string json = "{\n";

            json += string.Format("\"{0}\":\"{1}\",\n", "clientid", _config.ClientID);
            json += string.Format("\"{0}\":\"{1}\",\n", "debugmode", GetDebugModeState());
            json += string.Format("\"{0}\":\"{1}\",\n", "eventname", eventName);
            json += string.Format("\"{0}\":\"{1}\",\n", "appversion", Application.version);
            json += string.Format("\"{0}\":\"{1}\",\n", "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            json += string.Format("\"{0}\":\"{1}\",\n", "sessionid", _sessionID);
            json += string.Format("\"{0}\":\"{1}\",\n", "platform", Application.platform);
            json += string.Format("\"{0}\":\"{1}\",\n", "devicetype", SystemInfo.deviceType);
            json += string.Format("\"{0}\":\"{1}\",\n", "devicemodel", SystemInfo.deviceModel);
            json += string.Format("\"{0}\":\"{1}\",\n", "language", Application.systemLanguage.ToString());    
            json += string.Format("\"{0}\":\"{1}\",\n", "location", _clientLocation);
            json += string.Format("\"{0}\": {1}\n", "customparams", paramsJson);

            json += "}";
            return json;    
        }
        private static IEnumerator LoadClientLocation()
        {
            string uri = string.Empty;
            try
            {
                string ip = new System.Net.WebClient().DownloadString("https://api.ipify.org");
                uri = $"https://ipapi.co/{ip}/json/";
            }
            catch (System.Exception e)
            {
                if(_config.DebugMode) Debug.LogError("Error while loading client location: " + e.Message);   
                uri = string.Empty;
            }
            
            if(uri.Length > 0)
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
                {
                    yield return webRequest.SendWebRequest();
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        _clientLocation = "unknown";
                    }
                    else
                    {
                        var jsonResponse = webRequest.downloadHandler.text;
                        var jsonLines = jsonResponse.Split('\n');
                        foreach (var line in jsonLines)
                        {
                            if (line.Contains("\"country_name\""))
                            {
                                var country = line.Split(':')[1].Trim().Trim('"', ',');
                                _clientLocation = country;
                                break;
                            }
                        }
                        if(string.IsNullOrEmpty(_clientLocation))
                        {
                            if(_config.DebugMode) Debug.LogWarning("Client location not found.");
                            _clientLocation = "unknown";
                        }
                    }
                }
            }
            else
            {
                _clientLocation = "unknown";
            }
        }
        private static string GetClientLanguage()
        {
            return Application.systemLanguage.ToString();
        }
        private static bool GetDebugModeState()
        {
            #if UNITY_EDITOR
            return true;
            #else
            return _config.DebugMode;
            #endif

        }
        private IEnumerator PlayTimer()
        {
            if(_config.UpdateSessionLengthFrequency <= 0)
            {
                //do not send updates
            }
            else
            {
                while(true)
                {
                    yield return new WaitForSeconds(_config.UpdateSessionLengthFrequency);
                    UpdateSessionTimer();
                }
            }
        }
    }
}