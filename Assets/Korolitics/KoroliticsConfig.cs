using UnityEngine;

namespace Services.Korolitics.Core
{
    public class KoroliticsConfig : ScriptableObject
    {
        [SerializeField] private string _apiUrl = "http://localhost:3000";
        [SerializeField] private string _clientRoleName = "client_user";
        [SerializeField] private string _clientRolePassword = "client_password"; 
        private string _clientID;
        [SerializeField] private string _appName;
        [SerializeField] private bool _enableGPSCollectuion;
        [SerializeField, Tooltip("-1 - no updates")] private float _updateSessionLengthFrequency = 60f;
        [SerializeField] private bool _debugMode;
        public string AppName 
        {
            get => _appName;
            #if UNITY_EDITOR
            set => _appName = value;
            #endif
        }
        public bool EnableGPSCollection
        {
            get => _enableGPSCollectuion;
            #if UNITY_EDITOR
            set => _enableGPSCollectuion = value;
            #endif
        }
        public string ApiUrl {
            get => _apiUrl;
            #if UNITY_EDITOR
            set => _apiUrl = value;
            #endif
        }
        public string ClientRoleName {
            get => _clientRoleName;
            #if UNITY_EDITOR
            set => _clientRoleName = value;
            #endif
        }
        public string ClientRolePassword {
            get => _clientRolePassword;
            #if UNITY_EDITOR
            set => _clientRolePassword = value;
            #endif
        }
        public string ClientID
        {
            get
            {
                if(string.IsNullOrEmpty(_clientID))
                {
                    if(PlayerPrefs.HasKey("korolitics-client-id"))
                    {
                        _clientID = PlayerPrefs.GetString("korolitics-client-id");
                    }
                    else
                    {
                        _clientID = SystemInfo.deviceUniqueIdentifier.Substring(0, SystemInfo.deviceUniqueIdentifier.Length/2) + System.DateTime.Now.GetHashCode();
                        PlayerPrefs.SetString("korolitics-client-id", _clientID);
                    }
                }
                return _clientID;
            }
        }
        public bool DebugMode
        {
            get => _debugMode;
            #if UNITY_EDITOR
            set => _debugMode = value;
            #endif
        }
        public float UpdateSessionLengthFrequency
        {
            get => _updateSessionLengthFrequency;
            #if UNITY_EDITOR
            set => _updateSessionLengthFrequency = value;
            #endif
        }

        public bool IsValid()
        {
            if(string.IsNullOrEmpty(ApiUrl))
            {
                Debug.LogError("API URL is not set");
                return false;
            }
            if(string.IsNullOrEmpty(ClientRoleName))
            {
                Debug.LogError("Client Role Name is not set");
                return false;
            }
            if(string.IsNullOrEmpty(ClientRolePassword))
            {
                Debug.LogError("Client Role Password is not set");
                return false;
            }
            if(string.IsNullOrEmpty(AppName))
            {
                Debug.LogError("App Name is not set");
                return false;
            }
            return true;
        }
    }
}
