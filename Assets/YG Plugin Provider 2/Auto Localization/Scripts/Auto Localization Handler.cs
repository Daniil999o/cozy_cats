using System;
using UnityEngine;
using System.Linq;
using AutoLocalization.Json;
using System.Collections.Generic;

namespace AutoLocalization
{
    public static class AutoLocalizationHandler
    {
        private static string LocalizationPath => "Languages";

        public static string CurrentLanguage {
            get => PlayerPrefs.GetString("LOCALIZATION_LANG", PreferredLanguage);
            set => PlayerPrefs.SetString("LOCALIZATION_LANG", value);
        }

        private static string[] CurrentLanguagesFiles {
            get {
                try {
                    TextAsset[] assets = Resources.LoadAll<TextAsset>(LocalizationPath);

                    return assets.Select(x => x.name.ToLower()).ToArray();
                } catch (Exception e) {
                    Debug.LogError($"Localization resources load error: {e}");

                    return null;
                }
            }
        }
        
        public const string DefaultLangCode = "en";

        internal static string[] CurrentLanguages => CurrentLanguagesFiles;

        internal static string CurrentLanguageName => GetLocalizedText("localization_name");

        internal static void NextLanguage() {
            string curr = CurrentLanguage;

            if (!CurrentLanguages.Contains(curr))
                return;
            
            
            int currentLang = CurrentLanguages.ToList().IndexOf(curr);
            currentLang += 1;

            if (currentLang >= CurrentLanguages.Length) currentLang = 0;
            
            SetLanguage(currentLang);
        }

        internal static void SetLanguage(int index) {
            if (index < 0 || index >= CurrentLanguages.Length) return;

            CurrentLanguage = CurrentLanguages[index];
            
            UpdateLocalization();
        }

        private static string PreferredLanguage {
            get {
                string langCode = Application.systemLanguage.ToString().Substring(0, 2).ToLower();
                
                if (CurrentLanguagesFiles.Contains(langCode)) return langCode;
                
                return DefaultLangCode;
            }
        }

        private static Dictionary<string, string> _languagesKeys = new Dictionary<string, string>();

        internal static Action OnUpdateLocalization;

        private static bool _firstUpdate = true;

        public static string[] LocalizedQualityNames { get; set; } = null;

        public static void UpdateLocalization(bool invokeAfterUpdate = true) {
            string myLanguageFile = $"{LocalizationPath}/{CurrentLanguage}";

            _languagesKeys = JsonLoader.LoadJsonToDictionary(myLanguageFile);
            
            if (!invokeAfterUpdate) return;
                
            OnUpdateLocalization?.Invoke();
            
            LocalizedQualityNames = QualitySettings.names.Select(GetLocalizedText).ToArray();
        }

        public static string GetLocalizedText(string key) {
            key = key.ToLower();
            
            if (_firstUpdate) {
                _firstUpdate = false;
                UpdateLocalization();
            }
            
            if (_languagesKeys.TryGetValue(key, out string value)) return value;

            Debug.LogError($"Current localization file '{CurrentLanguageName}' not contains key {key}");

            return "LOCALIZATION ERROR";
        }
        
        public static string GetLocalizedText(string key, string lang) {
            key = key.ToLower();
            
            string myLanguageFile = $"{LocalizationPath}/{CurrentLanguage}";

            Dictionary<string, string> langKeys = JsonLoader.LoadJsonToDictionary(myLanguageFile);
            
            if (langKeys.TryGetValue(key, out string value)) return value;

            Debug.LogError($"Current localization file '{lang}' not contains key {key}");

            return "LOCALIZATION ERROR";
        }
    }
}