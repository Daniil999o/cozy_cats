#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AutoLocalization.Editor
{
    public class TranslateLanguageWindow : EditorWindow
    {
        private LocalizationJsonEditor parentWindow;
        private int sourceLanguageIndex = 0;
        private List<string> availableLanguages = new List<string>();
        private string targetLanguageCode;

        private bool translateAllKeys = false;

        private bool isTranslating = false;
        private string statusMessage = "";
        private float progress = 0f;
        private int totalKeys = 0;
        private int processedKeys = 0;

        public static void Open(LocalizationJsonEditor parent, string targetLang) {
            TranslateLanguageWindow window = GetWindow<TranslateLanguageWindow>(true, "Translate Language");
            window.parentWindow = parent;
            window.targetLanguageCode = targetLang;
            window.minSize = new Vector2(450, 230);
            window.maxSize = new Vector2(450, 230);
            window.Initialize();
        }

        private void Initialize() {
            // Получаем все языки через Resources
            TextAsset[] languageAssets = Resources.LoadAll<TextAsset>("Languages");
            
            availableLanguages = languageAssets
                .Select(asset => asset.name)
                .Where(l => l != targetLanguageCode)
                .Distinct()
                .ToList();

            if (availableLanguages.Count == 0) availableLanguages.Add("en");
            
            sourceLanguageIndex = 0;
        }

        private void OnGUI() {
            EditorGUILayout.Space(10);

            if (isTranslating) {
                DrawTranslatingProgress();
                return;
            }

            EditorGUILayout.LabelField($"Translate Language: {targetLanguageCode}", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            if (availableLanguages.Count > 0) {
                sourceLanguageIndex = EditorGUILayout.Popup("Translate from:",
                    sourceLanguageIndex, availableLanguages.ToArray()
                );

                EditorGUILayout.Space(5);

                translateAllKeys = EditorGUILayout.ToggleLeft("Translate ALL keys (overwrite existing translations)",
                    translateAllKeys
                );

                EditorGUILayout.Space(10);

                Dictionary<string, string> sourceKeys = GetLanguageKeys(availableLanguages[sourceLanguageIndex]);
                Dictionary<string, string> targetKeys = GetLanguageKeys(targetLanguageCode);

                int missingKeys = 0;
                int existingKeys = 0;

                foreach (string key in sourceKeys.Keys) {
                    if (!targetKeys.ContainsKey(key) || string.IsNullOrEmpty(targetKeys[key]))
                        missingKeys++;
                    else
                        existingKeys++;
                }

                if (translateAllKeys) {
                    EditorGUILayout.HelpBox(
                        $"Will translate ALL {sourceKeys.Count} keys from '{availableLanguages[sourceLanguageIndex]}' to '{targetLanguageCode}'.\n" +
                        $"Existing translations ({existingKeys} keys) will be OVERWRITTEN!",
                        MessageType.Warning
                    );
                } else {
                    EditorGUILayout.HelpBox(
                        $"Will translate {missingKeys} missing/empty keys from '{availableLanguages[sourceLanguageIndex]}' to '{targetLanguageCode}'.\n" +
                        $"Existing translations ({existingKeys} keys) will be preserved.",
                        MessageType.Info
                    );
                }

                EditorGUILayout.Space(15);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Cancel", GUILayout.Width(80))) Close();

                if (GUILayout.Button("Translate", GUILayout.Width(80))) {
                    if (EditorUtility.DisplayDialog("Confirm Translation",
                            translateAllKeys
                                ? $"Translate ALL keys from '{availableLanguages[sourceLanguageIndex]}' to '{targetLanguageCode}'?\nExisting translations will be overwritten!"
                                : $"Translate missing keys from '{availableLanguages[sourceLanguageIndex]}' to '{targetLanguageCode}'?",
                            "Yes", "Cancel"
                        ))
                        StartTranslation();
                }

                EditorGUILayout.EndHorizontal();
            } else {
                EditorGUILayout.HelpBox("No source languages found.", MessageType.Error);
                if (GUILayout.Button("Close")) Close();
            }
        }

        private Dictionary<string, string> GetLanguageKeys(string langCode) {
            TextAsset textAsset = Resources.Load<TextAsset>($"Languages/{langCode}");
            if (textAsset == null) return new Dictionary<string, string>();

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text)
                   ?? new Dictionary<string, string>();
        }

        private void DrawTranslatingProgress() {
            EditorGUILayout.Space(40);
            EditorGUILayout.LabelField(statusMessage, EditorStyles.centeredGreyMiniLabel);

            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(rect, progress, $"{processedKeys}/{totalKeys} keys");

            if (processedKeys >= totalKeys) {
                EditorGUILayout.Space(20);
                EditorGUILayout.HelpBox("Translation completed!", MessageType.Info);

                if (GUILayout.Button("OK", GUILayout.Height(30))) {
                    if (parentWindow != null) parentWindow.RefreshCurrentLanguage();
                    Close();
                }
            }

            Repaint();
        }

        private async void StartTranslation() {
            isTranslating = true;
            statusMessage = "Starting translation...";
            progress = 0f;
            processedKeys = 0;

            string sourceLang = availableLanguages[sourceLanguageIndex];
            Dictionary<string, string> sourceKeys = GetLanguageKeys(sourceLang);
            Dictionary<string, string> targetKeys = GetLanguageKeys(targetLanguageCode);

            List<string> keysToTranslate;

            if (translateAllKeys) {
                keysToTranslate = sourceKeys.Keys
                    .Where(key => !string.IsNullOrEmpty(sourceKeys[key]))
                    .ToList();
            } else {
                keysToTranslate = sourceKeys.Keys
                    .Where(key => !string.IsNullOrEmpty(sourceKeys[key]) &&
                                  (!targetKeys.ContainsKey(key) || string.IsNullOrEmpty(targetKeys[key]))
                    )
                    .ToList();
            }

            totalKeys = keysToTranslate.Count;

            if (totalKeys == 0) {
                statusMessage = translateAllKeys
                    ? "No keys with text to translate in source language."
                    : "No missing keys to translate.";
                processedKeys = totalKeys;
                progress = 1f;
                Repaint();
                return;
            }

            Dictionary<string, string> updatedTargetKeys = new Dictionary<string, string>(targetKeys);

            for (int i = 0; i < keysToTranslate.Count; i++) {
                string key = keysToTranslate[i];
                string sourceText = sourceKeys[key];

                processedKeys = i + 1;
                progress = (float)processedKeys / totalKeys;
                statusMessage = $"Translating key {processedKeys}/{totalKeys}: '{key}'";

                Repaint();

                try {
                    string translated = await Translator.AutoTranslator.TranslateAsync(
                        sourceText, sourceLang, targetLanguageCode
                    );

                    updatedTargetKeys[key] = translated;

                    if (i % 10 == 0 || i == keysToTranslate.Count - 1)
                        SaveLanguageFile(targetLanguageCode, updatedTargetKeys);
                } catch (System.Exception e) {
                    Debug.LogError($"Translation failed for key '{key}': {e.Message}");
                    updatedTargetKeys[key] = $"[TRANSLATION ERROR: {sourceText}]";
                }

                await Task.Delay(200);
            }

            SaveLanguageFile(targetLanguageCode, updatedTargetKeys);
            statusMessage = "Translation completed!";

            AssetDatabase.Refresh();
        }

        private void SaveLanguageFile(string langCode, Dictionary<string, string> data) {
            string path = $"Assets/Resources/Languages/{langCode}.json";
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }
    }
}

#endif