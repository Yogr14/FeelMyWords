using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Services.Korolitics.DeveloperConsole
{
    public class DBTables : ContentWindow
    {
        private const string c_errorMessage = "No data. Please refresh...";
        private bool isHandlingRequest;
        private List<string> _tables;
        int _selectedTableIndex = 0;
        private bool _currentProjectRegistered = false;
        private GUIStyle _redButtonStyle;
        private GUIStyle _tableColumnStyle;
        private int _rowsCountToLoad = 10;
        private int _rowsToLoadStartIndex = 1;
        private List<Dictionary<string, object>> _tableContent;
        private Dictionary<string, float> _tableColumnWidths;
        private Vector2 _tableContentPosition;
        private string _projectName;
        private string[] _filterOperations = new string[]{"=", "!=", "contains"};
        private int _filterOperationIndex = 0;
        private string _filterOriginColumn = string.Empty;
        private string _filterValue = string.Empty;
        public DBTables(HttpClient httpClient, Config config, bool authentificationPassed) : base(httpClient, config, authentificationPassed)
        {
            _tables = new List<string>(){c_errorMessage};
            _redButtonStyle = new GUIStyle(GUI.skin.button);
            _redButtonStyle.normal.background = new Texture2D(1,1);
            for(int x = 0; x < 1; x++)
            {
                for(int y = 0; y < 1; y++)
                {
                    _redButtonStyle.normal.background.SetPixel(x, y, _redButtonStyle.normal.background.GetPixel(x,y) * Color.red);
                }
            }
            _redButtonStyle.normal.background.Apply();

            _tableColumnStyle = new GUIStyle();
            _tableColumnStyle.normal.background = MakeTex(1, 1, new Color(0.0f, 0.0f, 0.0f, 0.1f));

            _tableColumnWidths = new Dictionary<string, float>();
            _projectName = Application.productName.ToLower().Replace(' ', '_');
        }
        internal override void Draw()
        {
            var labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };
            GUILayout.Label("Database Tables", labelStyle);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Select table:");
            _selectedTableIndex = EditorGUILayout.Popup(_selectedTableIndex, _tables.ToArray());
            if(isHandlingRequest) GUI.enabled = false;
            if (GUILayout.Button("Refresh"))
            {
                RefreshTablesList();
            }
            if(_currentProjectRegistered && _tables.Count > 0 && _tables[_selectedTableIndex] == _projectName)
            {
                if (GUILayout.Button("Clear", _redButtonStyle))
                {
                    OpenTableClearingWindow();
                }
                if (GUILayout.Button("Delete", _redButtonStyle))
                {
                    OpenTableDeleteWindow();
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            if(!_currentProjectRegistered && _tables.Count > 0 && _tables[0] != c_errorMessage)
            {
                EditorGUILayout.HelpBox("This project is not registered in the database. Click \"Connect this project\" button to create a new table.", MessageType.Warning);
            }
            if(isHandlingRequest) GUI.enabled = false;
            if(!_currentProjectRegistered && _tables.Count > 0 && _tables[0] != c_errorMessage)
            {
                if (GUILayout.Button("Connect this project"))
                {
                    OpenTableCreationWindow();
                }
            }
            GUI.enabled = true;
            if(_tables.Count > 0 && _tables[0] != c_errorMessage)
            {
                if(isHandlingRequest) GUI.enabled = false;
                EditorGUILayout.BeginHorizontal();
                _rowsCountToLoad = EditorGUILayout.IntField("Rows to load:", _rowsCountToLoad);
                _rowsToLoadStartIndex = EditorGUILayout.IntField("Start from row:", _rowsToLoadStartIndex);
                if (GUILayout.Button("Load table content"))
                {
                    LoadTableContent(_rowsToLoadStartIndex, _rowsCountToLoad);
                }
                EditorGUILayout.EndHorizontal();
                if(_tableContent == null || _tableContent.Count == 0) EditorGUILayout.HelpBox("No content from table loaded. Or requested content does not exist.", MessageType.Info);
                else
                {
                    _tableContentPosition = EditorGUILayout.BeginScrollView(_tableContentPosition);
                    //draw column labels
                    EditorGUILayout.BeginHorizontal();
                    foreach(var kv in _tableContent[0]) EditorGUILayout.LabelField(kv.Key, GUILayout.Width(_tableColumnWidths[kv.Key]));
                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = false;
                    //draw rows
                    foreach (var row in _tableContent)
                    {
                        EditorGUILayout.BeginHorizontal();
                        foreach (var cell in row)
                        {
                            if(cell.Value is Dictionary<string, object> inheritedValuesSet)
                            {
                                GUI.enabled = true;
                                EditorGUILayout.Popup(0, BSonParser.BsonAsPopUpContent(inheritedValuesSet).ToArray(), GUILayout.Width(_tableColumnWidths[cell.Key]));
                                GUI.enabled = false;
                            }
                            else
                            {
                                EditorGUILayout.TextField(cell.Value.ToString(), GUILayout.Width(_tableColumnWidths[cell.Key]));
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(1); //spacing between rows
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndScrollView();

                    //draw filter
                    EditorGUILayout.BeginHorizontal();
                    _filterOriginColumn = EditorGUILayout.TextField(string.Format("Filter {0} columns:", _tableContent.Count), _filterOriginColumn);
                    _filterOperationIndex = EditorGUILayout.Popup(_filterOperationIndex, _filterOperations);
                    _filterValue = EditorGUILayout.TextField(_filterValue);
                    if(GUILayout.Button("Filter"))
                    {
                        ApplyFilterOnTableContent();
                    }
                    if(GUILayout.Button("Reset"))
                    {
                        _filterOriginColumn = string.Empty;
                        _filterOperationIndex = 0;
                        _filterValue = string.Empty;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }   
        }

        private async void RefreshTablesList()
        {
            isHandlingRequest = true;
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Config.DeveloperRoleName}:{Config.DeveloperRolePassword}"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            try
            {       
                // Отправляем POST запрос к API, без тела т.к. функция без параметров
                string url = $"https://{Config.ApiUrl}/rpc/get_all_table_names";
                HttpResponseMessage response = await HttpClient.PostAsync(url, null);
                response.EnsureSuccessStatusCode(); // Проверяем, что ответ успешный

                // Получаем и возвращаем JSON строку из ответа
                var jsonResponse = await response.Content.ReadAsStringAsync();
                JArray jsonArray = JArray.Parse(jsonResponse);

                // Convert the JSON array to a List<string>
                _tables = jsonArray.Select(item => (string)item).ToList();
                _currentProjectRegistered = _tables.Contains(_projectName);
            }
            catch(Exception e)
            {
                Debug.LogError($"Error refreshing tables: {e.Message}");
                _tables = new List<string>(){c_errorMessage};
            }
            isHandlingRequest = false;
        }
        private async void OpenTableCreationWindow()
        {
            if (EditorUtility.DisplayDialog("Create New Table", $"Are you sure you want to create a new table with the name of this project \"{_projectName}\"?", "Create", "Cancel"))
            {
                isHandlingRequest = true;
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Config.DeveloperRoleName}:{Config.DeveloperRolePassword}"));
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                try
                {  
                    var requestBody = new StringContent(
                        JsonConvert.SerializeObject(new { table_name = _projectName }),
                        Encoding.UTF8,
                        "application/json"
                    );

                    // Отправляем POST запрос к API
                    string url = $"https://{Config.ApiUrl}/rpc/create_table";
                    HttpResponseMessage response = await HttpClient.PostAsync(url, requestBody);
                    response.EnsureSuccessStatusCode(); // Проверяем, что ответ успешный
                    
                    Debug.Log("Table created successfully");
                    _tables.Add(_projectName);
                    _currentProjectRegistered = true;
                }
                catch(Exception e)
                {
                    Debug.LogError($"Error creating new table: {e.Message}");
                }
                isHandlingRequest = false;
            }
        }
        private async void OpenTableDeleteWindow()
        {
            string tableName = _tables[_selectedTableIndex];
            if (EditorUtility.DisplayDialog("Delete Table", $"Are you sure you want to delete table with the name \"{tableName}\"?", "Delete", "Cancel"))
            {
                isHandlingRequest = true;
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Config.DeveloperRoleName}:{Config.DeveloperRolePassword}"));
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                try
                {  
                    var requestBody = new StringContent(
                        JsonConvert.SerializeObject(new { table_name = tableName }),
                        Encoding.UTF8,
                        "application/json"
                    );

                    // Отправляем POST запрос к API
                    string url = $"https://{Config.ApiUrl}/rpc/delete_table";
                    HttpResponseMessage response = await HttpClient.PostAsync(url, requestBody);
                    response.EnsureSuccessStatusCode(); // Проверяем, что ответ успешный
                    
                    Debug.Log("Table deleted successfully");
                    _tables.RemoveAt(_selectedTableIndex);
                    _selectedTableIndex = 0;
                    _currentProjectRegistered = _tables.Contains(_projectName);
                }
                catch(Exception e)
                {
                    Debug.LogError($"Error deleteing table: {e.Message}");
                }
                isHandlingRequest = false;
            }
        }
        private async void OpenTableClearingWindow()
        {
            string tableName = _tables[_selectedTableIndex];
            if (EditorUtility.DisplayDialog("Clear Table Content", $"Are you sure you want to erase content of table with the name \"{tableName}\"?", "Erase", "Cancel"))
            {
                isHandlingRequest = true;
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Config.DeveloperRoleName}:{Config.DeveloperRolePassword}"));
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                try
                {  
                    var requestBody = new StringContent(
                        JsonConvert.SerializeObject(new { table_name = tableName }),
                        Encoding.UTF8,
                        "application/json"
                    );

                    // Отправляем POST запрос к API
                    string url = $"https://{Config.ApiUrl}/rpc/clear_table";
                    HttpResponseMessage response = await HttpClient.PostAsync(url, requestBody);
                    response.EnsureSuccessStatusCode(); // Проверяем, что ответ успешный
                    
                    Debug.Log("Table cleared successfully");
                }
                catch(Exception e)
                {
                    Debug.LogError($"Error clearing table: {e.Message}");
                }
                isHandlingRequest = false;
            }
        }
        private async void LoadTableContent(int start_row, int rows_count)
        {
            string tableName = _tables[_selectedTableIndex];
            isHandlingRequest = true;
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Config.DeveloperRoleName}:{Config.DeveloperRolePassword}"));
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            try
            {  
                var requestBody = new StringContent(
                    JsonConvert.SerializeObject(new { table_name = tableName, start_row = start_row, num_rows = rows_count }),
                    Encoding.UTF8,
                    "application/json"
                );

                // Отправляем POST запрос к API
                string url = $"https://{Config.ApiUrl}/rpc/load_table_content";
                HttpResponseMessage response = await HttpClient.PostAsync(url, requestBody);
                response.EnsureSuccessStatusCode(); // Проверяем, что ответ успешный
                
                Debug.Log("Table content loaded successfully");
                var jsonb = await response.Content.ReadAsStringAsync();

                _tableContent = BSonParser.Parse(jsonb);
                string valueAsString;
                foreach(var row in _tableContent)
                {
                    foreach(var cell in row)
                    {
                        if(!_tableColumnWidths.ContainsKey(cell.Key)) _tableColumnWidths[cell.Key] = cell.Key.Length * 10;
                        if(cell.Value is not Dictionary<string, object>)
                        {
                            valueAsString = cell.Value.ToString();
                            if(valueAsString.Length * 10 > _tableColumnWidths[cell.Key]) _tableColumnWidths[cell.Key] = valueAsString.Length * 10;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                if(e.Message.StartsWith("Length cannot be less than zero.")) Debug.LogError("Overflow. No content to load");
                else Debug.LogError($"Error loading table content: {e.Message}");
            }
            isHandlingRequest = false;
        }
        private void ApplyFilterOnTableContent()
        {
            if(_tableContent == null || _tableContent.Count == 0) return;
            if(!_tableContent[0].ContainsKey(_filterOriginColumn)) return;
            if(string.IsNullOrEmpty(_filterOriginColumn) && string.IsNullOrEmpty(_filterValue)) return;

            List<Dictionary<string, object>> filteredContent = new List<Dictionary<string, object>>();
            foreach(var row in _tableContent)
            {
                if(_filterOperationIndex == 0)
                {
                    if(row[_filterOriginColumn].ToString().Equals(_filterValue)) filteredContent.Add(row);
                }
                else if(_filterOperationIndex == 1)
                {
                    if(!row[_filterOriginColumn].ToString().Equals(_filterValue)) filteredContent.Add(row);
                }
                else if(_filterOperationIndex == 2)
                {
                    if(row[_filterOriginColumn].ToString().Contains(_filterValue)) filteredContent.Add(row);
                }
            }
            _tableContent = filteredContent;
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
