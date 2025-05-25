using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Services.Korolitics.DeveloperConsole
{
    public static class BSonParser
    {
        public static List<Dictionary<string, object>> Parse(string bsonText)
        {
            if(bsonText.Equals("null"))
            {
                return new List<Dictionary<string, object>>();
            }
            bsonText = bsonText.Trim();
 
            if (!bsonText.StartsWith("[") || !bsonText.EndsWith("]"))
            {
                throw new ArgumentException("Invalid BSON format: Must start with '[' and end with ']'.");
            }
            

            bsonText = bsonText.Substring(1, bsonText.Length - 2); // Remove brackets
            
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            string[] bsonObjects = SplitBsonObjects(bsonText);
            
            foreach (string bsonObject in bsonObjects)
            {
                result.Add(ParseBsonObject(bsonObject.Trim()));
            }       

            return result;
        }
        public static List<string> BsonAsPopUpContent(Dictionary<string, object> keyValuePairs)
        {
            List<string> result = new List<string>();
            foreach (var pair in keyValuePairs)
            {
                if (pair.Value is Dictionary<string, object> bsonObject)
                {
                    result.Add(pair.Key);
                    result.AddRange(BsonAsPopUpContent(bsonObject));
                }
                else
                {
                    result.Add($"{pair.Key}: {pair.Value}");
                }
            }
            return result;
        }
        private static string[] SplitBsonObjects(string bsonText)
        {
            List<string> objects = new List<string>();
            int bracketCount = 0;
            int startIndex = 0;
            

            for (int i = 0; i < bsonText.Length; i++)
            {
                if (bsonText[i] == '{')
                {
                    bracketCount++;
                }
                else if (bsonText[i] == '}')
                {
                    bracketCount--;
                }
                else if (bsonText[i] == ',' && bracketCount == 0)
                {
                    objects.Add(bsonText.Substring(startIndex, i - startIndex));
                    startIndex = i + 1;
                }
            }
            objects.Add(bsonText.Substring(startIndex));
            return objects.ToArray();
        }
        private static Dictionary<string, object> ParseBsonObject(string bsonObject)
        {
            if (!bsonObject.StartsWith("{") || !bsonObject.EndsWith("}"))
            {
            throw new ArgumentException("Invalid BSON object format: Must start with '{' and end with '}'.");
            }
    
            bsonObject = bsonObject.Substring(1, bsonObject.Length - 2); // Remove curly braces
        
            Dictionary<string, object> result = new Dictionary<string, object>();
            string[] keyValuePairs = SplitKeyValuePairs(bsonObject);
        
            foreach (string keyValuePair in keyValuePairs)
            {
                string[] parts = keyValuePair.Split(new[] { ':' }, 2);
                if (parts.Length != 2)
                {
                continue; // Skip invalid key-value pairs
                }

                string key = parts[0].Trim().Trim('"');
                string value = parts[1].Trim();
            
                result[key] = ParseValue(value);
            }
        
            return result;
        }
        private static string[] SplitKeyValuePairs(string bsonObject)
        {
            List<string> keyValuePairs = new List<string>();
            int bracketCount = 0;
            int startIndex = 0;
        

            for (int i = 0; i < bsonObject.Length; i++)
            {
                if (bsonObject[i] == '{')
                {
                    bracketCount++;
                }
                else if (bsonObject[i] == '}')
                {
                    bracketCount--;
                }
                else if (bsonObject[i] == ',' && bracketCount == 0)
                {
                    keyValuePairs.Add(bsonObject.Substring(startIndex, i - startIndex));
                    startIndex = i + 1;
                }
            }

            keyValuePairs.Add(bsonObject.Substring(startIndex));
            return keyValuePairs.ToArray();
        }
        private static object ParseValue(string value)
        {
            value = value.Trim();
        
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                return ParseBsonObject(value);
            }
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                return value.Substring(1, value.Length - 2);
            }
            if (int.TryParse(value, out int intValue))
            {
                return intValue;
            }
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
            {
                return doubleValue;
            }
            if (bool.TryParse(value.ToLower(), out bool boolValue))
            {
                return boolValue;
            }
            if (DateTime.TryParse(value, out DateTime dateTimeValue))
            {
                return dateTimeValue;
            }

            return value; // Return as string if no other type matches
        }

        public class BSon
        {
            
        }
        public class BSonObject
        {
            public bool IsSimpleValue { get; private set; }
            private BSonValue _simpleValue;
            private BSonObject _objectValue;

            public string Value => IsSimpleValue ? _simpleValue.Value : _objectValue.Value;
            public BSonObject(string key, string value)
            {
                _simpleValue = new BSonValue(key, value);
                IsSimpleValue = true;
            }
            private BSonObject()
            {
                IsSimpleValue = false;
            }
            public BSonObject(string bson)
            {
                _objectValue = new BSonObject();
                //parse bson
            }
            public IEnumerator<BSonValue> GetEnumerator()
            {
                if (IsSimpleValue)
                {
                    yield return _simpleValue;
                }
                else
                {
                    foreach (var value in _objectValue)
                    {
                        yield return value;
                    }
                }
            }

        }
        public class BSonValue
        {
            public string Key { get; private set; }
            public string Value { get; private set; }

            public BSonValue(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
