#if UNITY_EDITOR

using UnityEditor;

namespace AutoLocalization.Editor
{
    public static class AutoLocalizationMenuItems
    {
        [MenuItem("Auto Localization/Create New Language")]
        private static void CreateNewLanguageFromMenu() {
            LocalizationJsonEditor editorWindow =
                EditorWindow.GetWindow<LocalizationJsonEditor>("Localization JSON Editor");
            NewLanguageWindow.Open(editorWindow);
        }

        [MenuItem("Auto Localization/Clear Translation Cache")]
        private static void ClearTranslationCache() {
            if (EditorUtility.DisplayDialog("Clear Translation Cache",
                    "Are you sure you want to clear all cached translations?",
                    "Clear", "Cancel"
                )) {
                Translator.AutoTranslator.ClearCache();
                EditorUtility.DisplayDialog("Cache Cleared",
                    "Translation cache has been cleared successfully.", "OK"
                );
            }
        }

        [MenuItem("Auto Localization/Validate Languages")]
        private static void ValidateLanguages() {
            EditorUtility.DisplayDialog("Validation",
                "This feature will be implemented in the next version.", "OK"
            );
        }
    }
}

#endif