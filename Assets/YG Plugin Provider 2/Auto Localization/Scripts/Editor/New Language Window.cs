#if UNITY_EDITOR

using UnityEditor;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using AutoLocalization.Misc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AutoLocalization.Editor
{
    public class NewLanguageWindow : EditorWindow
    {
        private LocalizationJsonEditor parentWindow;
        private string newLanguageCode = "";
        private int selectedLanguageIndex = 0;
        private int sourceLanguageIndex = 0;
        private bool autoTranslate = false;
        private List<string> availableSourceLanguages = new List<string>();

        private bool isCreating = false;
        private string statusMessage = "";
        private bool isSuccess = false;

        private float _progressBarValue;

        public static void Open(LocalizationJsonEditor parent) {
            NewLanguageWindow window = GetWindow<NewLanguageWindow>(true, "Create New Language");
            window.parentWindow = parent;
            window.minSize = new Vector2(450, 250);
            window.maxSize = new Vector2(450, 250);
            window.Initialize();
        }

        private void Initialize() {
            // Получаем список языков через Resources
            TextAsset[] languageAssets = Resources.LoadAll<TextAsset>("Languages");
            
            foreach (TextAsset asset in languageAssets) {
                string fileName = asset.name;
                if (!availableSourceLanguages.Contains(fileName)) {
                    availableSourceLanguages.Add(fileName);
                }
            }

            if (availableSourceLanguages.Count == 0) availableSourceLanguages.Add("en");
            
            sourceLanguageIndex = 0;
            autoTranslate = availableSourceLanguages.Count > 0;
            SelectFirstAvailableLanguage();
        }

        private void SelectFirstAvailableLanguage() {
            // Проверяем существующие языки
            string[] existingLangs = availableSourceLanguages.ToArray();
            
            foreach (string code in LanguageCodes.AllCodes) {
                if (!existingLangs.Contains(code)) {
                    newLanguageCode = code;
                    break;
                }
            }
        }

        private bool LanguageExists(string langCode) {
            return availableSourceLanguages.Contains(langCode);
        }

        private void OnGUI() {
            EditorGUILayout.Space(10);

            if (isCreating) {
                DrawCreatingStatus();
                return;
            }

            EditorGUILayout.LabelField("Create New Language", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Language:", GUILayout.Width(150));

            List<string> availableNewLanguages = new List<string>();
            HashSet<string> existingLanguages = new HashSet<string>(availableSourceLanguages);

            foreach (string code in LanguageCodes.AllCodes) {
                if (!existingLanguages.Contains(code)) availableNewLanguages.Add(LanguageCodes.GetLanguageName(code));
            }

            string[] languageOptions = availableNewLanguages.ToArray();

            if (languageOptions.Length == 0)
                EditorGUILayout.LabelField("All standard languages already exist", EditorStyles.helpBox);
            else {
                selectedLanguageIndex =
                    EditorGUILayout.Popup(selectedLanguageIndex, languageOptions, GUILayout.Width(200));
                newLanguageCode = LanguageCodes.GetCodeFromDisplayName(languageOptions[selectedLanguageIndex]);
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(newLanguageCode) && LanguageExists(newLanguageCode))
                EditorGUILayout.HelpBox($"Language '{newLanguageCode}' already exists!", MessageType.Error);

            if (languageOptions.Length == 0) {
                EditorGUILayout.Space(5);
                newLanguageCode = EditorGUILayout.TextField("Or enter custom code (2 letters):", newLanguageCode);

                if (!string.IsNullOrEmpty(newLanguageCode)) {
                    newLanguageCode = newLanguageCode.ToLower();
                    if (newLanguageCode.Length != 2)
                        EditorGUILayout.HelpBox("Language code should be 2 characters (e.g., 'en', 'ru')",
                            MessageType.Warning
                        );
                }
            }

            EditorGUILayout.Space(10);

            if (availableSourceLanguages.Count > 0) {
                string[] sourceLanguageOptions = availableSourceLanguages
                    .Select(code => LanguageCodes.GetLanguageName(code))
                    .ToArray();

                sourceLanguageIndex = EditorGUILayout.Popup("Translate from:",
                    sourceLanguageIndex, sourceLanguageOptions
                );

                EditorGUI.BeginDisabledGroup(availableSourceLanguages.Count == 0);
                autoTranslate = EditorGUILayout.ToggleLeft("🤖 Auto-translate from selected language", autoTranslate);
                EditorGUI.EndDisabledGroup();

                if (autoTranslate) {
                    string sourceLangName =
                        LanguageCodes.GetLanguageName(availableSourceLanguages[sourceLanguageIndex]);
                    EditorGUILayout.HelpBox($"All keys will be automatically translated from {sourceLangName}",
                        MessageType.Info
                    );
                }
            } else {
                autoTranslate = false;
                EditorGUILayout.HelpBox("No existing languages found. New language will be created empty.",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(80))) Close();

            bool canCreate = !string.IsNullOrEmpty(newLanguageCode) &&
                             (languageOptions.Length > 0 || newLanguageCode.Length == 2) &&
                             !LanguageExists(newLanguageCode);

            EditorGUI.BeginDisabledGroup(!canCreate);
            if (GUILayout.Button("Create", GUILayout.Width(80))) CreateNewLanguage();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCreatingStatus() {
            EditorGUILayout.Space(40);
            EditorGUILayout.LabelField(statusMessage, EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(10);

            if (isSuccess) {
                EditorGUILayout.HelpBox("Language created successfully!", MessageType.Info);
                EditorGUILayout.Space(20);

                if (GUILayout.Button("OK", GUILayout.Height(30))) Close();
            } else {
                Rect rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, _progressBarValue, "Creating language...");
            }
        }

        private async void CreateNewLanguage() {
            _progressBarValue = 0f;
            isCreating = true;
            statusMessage = "Creating new language...";
            Repaint();

            try {
                // Проверяем, существует ли уже язык
                if (LanguageExists(newLanguageCode)) {
                    statusMessage = $"Error: Language '{newLanguageCode}' already exists!";
                    isSuccess = false;
                    Repaint();
                    return;
                }

                Dictionary<string, string> newLanguageData = new Dictionary<string, string>();

                if (autoTranslate && availableSourceLanguages.Count > 0) {
                    string sourceLang = availableSourceLanguages[sourceLanguageIndex];
                    string sourceLangName = LanguageCodes.GetLanguageName(sourceLang);
                    
                    // Загружаем исходный язык через Resources
                    TextAsset sourceAsset = Resources.Load<TextAsset>($"Languages/{sourceLang}");
                    
                    if (sourceAsset != null) {
                        statusMessage = $"Translating from {sourceLangName}...";
                        Repaint();

                        Dictionary<string, string> sourceData =
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(sourceAsset.text)
                            ?? new Dictionary<string, string>();

                        int totalKeys = sourceData.Count;
                        int currentKey = 0;

                        foreach (KeyValuePair<string, string> kv in sourceData) {
                            currentKey++;
                            statusMessage = $"Translating key {currentKey}/{totalKeys}: {kv.Key}";

                            _progressBarValue = (float)currentKey / Mathf.Clamp(totalKeys, 1, totalKeys + 1);
                            
                            Repaint();

                            if (!string.IsNullOrEmpty(kv.Value)) {
                                try {
                                    string translated = await Translator.AutoTranslator.TranslateAsync(
                                        kv.Value, sourceLang, newLanguageCode
                                    );

                                    newLanguageData[kv.Key] = translated;
                                } catch (System.Exception e) {
                                    Debug.LogError($"Translation failed for key '{kv.Key}': {e.Message}");
                                    newLanguageData[kv.Key] = kv.Value;
                                }
                            } else
                                newLanguageData[kv.Key] = "";

                            await Task.Delay(100);
                        }
                    }
                }

                // Создаем файл нового языка
                string json = JsonConvert.SerializeObject(newLanguageData, Formatting.Indented);
                string path = $"Assets/Resources/Languages/{newLanguageCode}.json";
                
                System.IO.File.WriteAllText(path, json);
                AssetDatabase.Refresh();

                statusMessage = "Language created successfully!";
                isSuccess = true;

                if (parentWindow != null) parentWindow.RefreshFromNewLanguage(newLanguageCode);

            } catch (System.Exception e) {
                statusMessage = $"Error: {e.Message}";
                isSuccess = false;
                Debug.LogError($"Failed to create language '{newLanguageCode}': {e}");
            }

            _progressBarValue = 0f;

            Repaint();
        }
    }
}

#endif