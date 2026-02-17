using System.Collections.Generic;

namespace AutoLocalization.Misc
{
    public static class LanguageCodes
    {
        public static readonly Dictionary<string, string> LanguageNames = new Dictionary<string, string> {
            {
                "en", "English"
            }, {
                "es", "Spanish"
            }, {
                "kk", "Kazah"
            }, {
                "be", "Belorussian"
            }, {
                "uz", "Uzbek"
            }, {
                "ky", "Kirgiz"
            }, {
                "fr", "French"
            }, {
                "de", "German"
            }, {
                "it", "Italian"
            }, {
                "pt", "Portuguese"
            }, {
                "ru", "Russian"
            }, {
                "zh", "Chinese"
            }, {
                "ja", "Japanese"
            }, {
                "ko", "Korean"
            }, {
                "ar", "Arabic"
            }, {
                "hi", "Hindi"
            }, {
                "bn", "Bengali"
            }, {
                "tr", "Turkish"
            }, {
                "nl", "Dutch"
            }, {
                "sv", "Swedish"
            }, {
                "pl", "Polish"
            }, {
                "cs", "Czech"
            }, {
                "el", "Greek"
            }, {
                "hu", "Hungarian"
            }, {
                "ro", "Romanian"
            }, {
                "da", "Danish"
            }, {
                "fi", "Finnish"
            }, {
                "sk", "Slovak"
            }, {
                "bg", "Bulgarian"
            }, {
                "uk", "Ukrainian"
            }, {
                "vi", "Vietnamese"
            }, {
                "th", "Thai"
            }, {
                "id", "Indonesian"
            }, {
                "ms", "Malay"
            }
        };

        public static readonly string[] AllCodes = {
            "en", "es", "kk", "be", "uz", "ky", "fr", "de", "it", "pt", "ru", "zh", "ja", "ko", "ar", "hi", "bn", "tr", "nl", "sv", "pl", "cs",
            "el", "hu", "ro", "da", "fi", "sk", "bg", "uk", "vi", "th", "id", "ms"
        };

        public static string GetLanguageName(string code) =>
            LanguageNames.TryGetValue(code, out string name) ? $"{code} - {name}" : code;

        public static string[] GetAvailableLanguages(string[] existingLanguages) {
            List<string> available = new List<string>();
            HashSet<string> existingSet = new HashSet<string>(existingLanguages);

            foreach (string code in AllCodes) {
                if (!existingSet.Contains(code)) available.Add(GetLanguageName(code));
            }

            return available.ToArray();
        }

        public static string GetCodeFromDisplayName(string displayName) {
            if (displayName.Length >= 2) return displayName.Substring(0, 2);
            
            return displayName;
        }
    }
}