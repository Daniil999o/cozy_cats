#if UNITY_EDITOR

using AutoLocalization.Misc;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using AutoLocalization.Translator;
using System.Collections.Generic;

namespace AutoLocalization.Editor
{
    public class TranslatorEditor : EditorWindow
    {
        private string inputText = "";
        private string translatedText = "";
        private int sourceIndex = 0;
        private int targetIndex = 1;
        private bool autoDetect = true;
        private bool isTranslating = false;

        [MenuItem("Auto Localization/Translator")]
        private static void Open() {
            TranslatorEditor wnd = GetWindow<TranslatorEditor>("Translator");
            wnd.minSize = new Vector2(500, 380);
        }

        private void OnGUI() {
            GUILayout.Space(10);

            DrawLanguagesBar();

            GUILayout.Space(10);
            DrawInputArea();

            GUILayout.Space(10);
            DrawTranslateButton();

            GUILayout.Space(10);
            DrawOutputArea();
        }

#region UI Sections

        private void DrawLanguagesBar() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(autoDetect);

            string[] languages = GetAllLanguages();
            
            sourceIndex = EditorGUILayout.Popup(sourceIndex, languages, GUILayout.Width(90));
            EditorGUI.EndDisabledGroup();

            autoDetect = EditorGUILayout.ToggleLeft("Auto", autoDetect, GUILayout.Width(60));
            GUILayout.Space(10);

            GUIContent swapIcon = new GUIContent("⇄", "Swap languages and texts");
            if (GUILayout.Button(swapIcon, GUILayout.Width(40), GUILayout.Height(24))) SwapLanguagesAndTexts();

            GUILayout.Space(10);

            targetIndex = EditorGUILayout.Popup(targetIndex, languages, GUILayout.Width(90));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private string[] GetAllLanguages() {
            List<string> languages = new List<string>();
            languages.Add("auto");
            languages.AddRange(LanguageCodes.AllCodes);
            
            return languages.ToArray();
        }

        private void DrawInputArea() {
            EditorGUILayout.LabelField("Input Text:", EditorStyles.boldLabel);
            inputText = EditorGUILayout.TextArea(inputText, GUILayout.MinHeight(100));
        }

        private void DrawTranslateButton() {
            EditorGUI.BeginDisabledGroup(isTranslating);
            GUIStyle btnStyle = new GUIStyle(EditorStyles.miniButtonMid) {
                fontSize = 13, alignment = TextAnchor.MiddleCenter
            };
            if (GUILayout.Button(isTranslating ? "Translating..." : "Translate", btnStyle, GUILayout.Height(30)))
                _ = DoTranslateAsync();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawOutputArea() {
            EditorGUILayout.LabelField("Result:", EditorStyles.boldLabel);
            GUIStyle resStyle = new GUIStyle(EditorStyles.textArea) {
                wordWrap = true
            };
            translatedText = EditorGUILayout.TextArea(translatedText, resStyle, GUILayout.MinHeight(100));
        }

#endregion

#region Logic

        private async Task DoTranslateAsync() {
            isTranslating = true;
            translatedText = "Translating...";
            Repaint();

            string[] languages = GetAllLanguages();

            string src = autoDetect ? "auto" : languages[sourceIndex];
            string tgt = languages[targetIndex];

            translatedText = await AutoTranslator.TranslateAsync(inputText, src, tgt);

            isTranslating = false;
            Repaint();
        }

        private void SwapLanguagesAndTexts() {
            autoDetect = false;

            int tmpIndex = sourceIndex;
            sourceIndex = targetIndex;
            targetIndex = tmpIndex;

            string tmpText = inputText;
            inputText = translatedText;
            translatedText = tmpText;

            Repaint();
        }

#endregion
    }
}

#endif