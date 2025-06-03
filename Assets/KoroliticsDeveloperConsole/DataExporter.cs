using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Services.Korolitics.DeveloperConsole
{
    public static class DataExporter
    {
        public static void ExportDataToCSV(List<Dictionary<string, object>> tableContent, string name)
        {
            if (tableContent == null || tableContent.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Failed", "No table content to export.", "OK");
                return;
            }
            StringBuilder csv = new StringBuilder();

            // Write header
            var headers = tableContent[0].Keys.ToList();
            csv.AppendLine(string.Join(",", headers.Select(h => $"{h}")));
            string rowData;
            foreach (var row in tableContent)
            {
                rowData = string.Empty;
                foreach (var column in row)
                {
                    if (column.Value is not Dictionary<string, object> customParams)
                    {
                        rowData += $"{column.Value},";
                        continue;
                    }
                    foreach (var customParam in customParams)
                    {
                        rowData += $"{customParam.Value},";
                    }
                }
                csv.AppendLine(rowData);
            }

            // Get Downloads path
            string downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/Downloads";
            string fileName = $"{name}.csv";
            string fullPath = System.IO.Path.Combine(downloadsPath, fileName);

            try
            {
                System.IO.File.WriteAllText(fullPath, csv.ToString(), Encoding.UTF8);
                EditorUtility.DisplayDialog("Export Successful", $"CSV exported to:\n{fullPath}", "OK");
                EditorUtility.RevealInFinder(fullPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to export CSV: {ex.Message}");
                EditorUtility.DisplayDialog("Export Failed", $"Could not write file:\n{fullPath}\n\n{ex.Message}", "OK");
            }
        }
    }
}
