using System;
using System.IO;
using UnityEngine;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoLocalization.Translator
{
    public static class AutoTranslator
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private static Dictionary<string, string> _translationCache = new Dictionary<string, string>();
        private static string _cacheFilePath;

        static AutoTranslator() {
            _cacheFilePath = Path.Combine(Application.dataPath, "..", "Library", "AutoTranslationCache.json");
            LoadCache();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.quitting += SaveCache;
#endif
        }

        /// <summary>
        /// Переводит текст с языка sourceLang (например "en") на язык targetLang (например "ru").
        /// Если sourceLang == "auto", язык определяется автоматически.
        /// </summary>
        public static async Task<string> TranslateAsync(string text, string sourceLang, string targetLang) {
            if (string.IsNullOrEmpty(text)) return "";

            string cacheKey = $"{sourceLang}_{targetLang}_{text}";
            if (_translationCache.TryGetValue(cacheKey, out string cached)) {
                Debug.Log(
                    $"[AutoTranslator] Using cached translation for: '{text.Substring(0, Math.Min(50, text.Length))}'"
                );
                return cached;
            }

            try {
                Debug.Log($"[AutoTranslator] Translating: '{text.Substring(0, Math.Min(50, text.Length))}'");

                string url =
                    $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLang}&tl={targetLang}&dt=t&q={Uri.EscapeDataString(text)}";

                HttpResponseMessage response = await httpClient.GetAsync(url);
                string result = await response.Content.ReadAsStringAsync();

                string translated = ParseGoogleResult(result);

                _translationCache[cacheKey] = translated;

                Debug.Log(
                    $"[AutoTranslator] Translated: '{text.Substring(0, Math.Min(50, text.Length))}' -> '{translated.Substring(0, Math.Min(50, translated.Length))}'"
                );
                return translated;
            } catch (Exception e) {
                Debug.LogError($"[AutoTranslator] Translation error: {e.Message}");
                return $"[TRANSLATION ERROR: {e.Message}]";
            }
        }

        public static string Translate(string text, string sourceLang, string targetLang) {
            try {
                string cacheKey = $"{sourceLang}_{targetLang}_{text}";
                if (_translationCache.TryGetValue(cacheKey, out string cached)) return cached;

                Task<string> task = Task.Run(() => TranslateAsync(text, sourceLang, targetLang));
                task.Wait();
                return task.Result;
            } catch (Exception e) {
                Debug.LogError($"[AutoTranslator] Sync translation failed: {e.Message}");
                return $"[SYNC TRANSLATION ERROR: {e.Message}]";
            }
        }

        private static string ParseGoogleResult(string json) {
            if (string.IsNullOrEmpty(json)) return "";

            try {
                JToken token = JsonConvert.DeserializeObject<JToken>(json);

                JArray translationArray = token[0] as JArray;

                if (translationArray != null) {
                    List<string> translations = new List<string>();

                    foreach (JToken segment in translationArray) {
                        JArray segmentArray = segment as JArray;
                        if (segmentArray != null && segmentArray.Count > 0) {
                            string translatedText = segmentArray[0].ToString();
                            translations.Add(translatedText);
                        }
                    }

                    string result = string.Join(" ", translations);

                    result = Regex.Replace(result, @"\s+", " ");

                    result = result.Replace("\\n", "\n")
                        .Replace("\\\"", "\"")
                        .Replace("\\'", "'");

                    return result;
                }
            } catch (Exception e) {
                Debug.LogError($"[AutoTranslator] JSON parsing error: {e.Message}");
            }

            Match match = Regex.Match(json, "\\[\\[\\[\\s*\"(.*?)\"");
            if (match.Success) {
                string val = match.Groups[1].Value;
                val = val.Replace("\\n", "\n").Replace("\\\"", "\"");
                return val;
            }

            return json;
        }

        private static void LoadCache() {
            if (File.Exists(_cacheFilePath)) {
                try {
                    string json = File.ReadAllText(_cacheFilePath);
                    _translationCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                                        ?? new Dictionary<string, string>();
                    Debug.Log($"[AutoTranslator] Loaded {_translationCache.Count} cached translations");
                } catch (Exception e) {
                    Debug.LogWarning($"[AutoTranslator] Failed to load translation cache: {e.Message}");
                    _translationCache = new Dictionary<string, string>();
                }
            }
        }

        private static void SaveCache() {
            try {
                string json = JsonConvert.SerializeObject(_translationCache, Formatting.Indented);
                File.WriteAllText(_cacheFilePath, json);
                Debug.Log($"[AutoTranslator] Saved {_translationCache.Count} translations to cache");
            } catch (Exception e) {
                Debug.LogWarning($"[AutoTranslator] Failed to save translation cache: {e.Message}");
            }
        }

        public static void ClearCache() {
            _translationCache.Clear();
            if (File.Exists(_cacheFilePath)) File.Delete(_cacheFilePath);
            Debug.Log("[AutoTranslator] Translation cache cleared");
        }
    }
}