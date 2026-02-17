using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoLocalization.Json
{
    public static class JsonLoader
    {
        public static Dictionary<string, string> LoadJsonToDictionary(string filePath) {
            TextAsset textAsset;

            try {
                textAsset = Resources.Load($"{filePath}") as TextAsset;
            } catch (System.Exception ex) {
                Debug.LogError($"JSON load error: {ex.Message}");
                return new Dictionary<string, string>();
            }
            
            if (!textAsset) {
                Debug.LogError($"JSON not found: {filePath}");
                return new Dictionary<string, string>();
            }

            try {
                string jsonText = textAsset.text;

                return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
            } catch (System.Exception ex) {
                Debug.LogError($"JSON read error: {ex.Message}");

                return new Dictionary<string, string>();
            }
        }
    }
}