#if UNITY_EDITOR

using TMPro;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using AutoLocalization.UI;
using System.Collections.Generic;
using AutoLocalization.Localization;

namespace AutoLocalization.Editor
{
    public class AutoLocalizationEditor : EditorWindow
    {
        private List<LocalizedObject> localizedObjects = new List<LocalizedObject>();
        private Dictionary<string, bool> groupFoldouts = new Dictionary<string, bool>();
        private Dictionary<LocalizedObject, bool> objectFoldouts = new Dictionary<LocalizedObject, bool>();
        private string searchFilter = "";
        private Vector2 scrollPos;

        private List<string> availableLanguages = new List<string>();
        private int selectedLanguageIndex = 0;

        [MenuItem("Auto Localization/Scene Manager")]
        private static void OpenWindow() {
            AutoLocalizationEditor wnd = GetWindow<AutoLocalizationEditor>("Auto Localization Manager");
            wnd.minSize = new Vector2(640, 450);
        }

        private void OnEnable() {
            RefreshLanguages();
            RefreshList();
        }

        private void RefreshLanguages() {
            availableLanguages.Clear();
            
            TextAsset[] languageAssets = Resources.LoadAll<TextAsset>("Languages");
            
            foreach (TextAsset asset in languageAssets) {
                string fileName = asset.name;
                if (!availableLanguages.Contains(fileName)) {
                    availableLanguages.Add(fileName);
                }
            }

            if (availableLanguages.Count == 0) availableLanguages.Add("en");

            string currentLang = AutoLocalizationHandler.CurrentLanguage;
            selectedLanguageIndex = availableLanguages.IndexOf(currentLang);
            if (selectedLanguageIndex < 0) selectedLanguageIndex = 0;
        }

        private void RefreshList() {
            localizedObjects = FindObjectsByType<LocalizedObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(o => o != null && o is not UILanguageChanger)
                .OrderBy(GetGroupName)
                .ThenBy(o => o.name)
                .ToList();

            groupFoldouts.Clear();
            objectFoldouts.Clear();
        }

        private void OnGUI() {
            DrawToolbar();
            GUILayout.Space(5);
            DrawList();
        }

#region Toolbar

        private void DrawToolbar() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUIStyle searchStyle =
                GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.toolbarTextField;
            searchFilter = GUILayout.TextField(searchFilter, searchStyle);

            EditorGUI.BeginChangeCheck();
            selectedLanguageIndex = EditorGUILayout.Popup(selectedLanguageIndex, availableLanguages.ToArray(),
                GUILayout.Width(100)
            );
            if (EditorGUI.EndChangeCheck()) {
                AutoLocalizationHandler.CurrentLanguage = availableLanguages[selectedLanguageIndex];
                AutoLocalizationHandler.UpdateLocalization();
            }

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70))) {
                RefreshLanguages();
                RefreshList();
            }

            EditorGUILayout.EndHorizontal();
        }

#endregion

#region Main list

        private void DrawList() {
            if (localizedObjects == null || localizedObjects.Count == 0) {
                EditorGUILayout.HelpBox("No LocalizedObject components found in the current scene.", MessageType.Info);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            IEnumerable<IGrouping<string, LocalizedObject>> grouped = localizedObjects.GroupBy(GetGroupName);
            HashSet<string> usedKeys = new HashSet<string>();

            foreach (IGrouping<string, LocalizedObject> group in grouped) {
                if (!groupFoldouts.ContainsKey(group.Key)) groupFoldouts[group.Key] = true;

                groupFoldouts[group.Key] = EditorGUILayout.Foldout(groupFoldouts[group.Key],
                    group.Key, true, EditorStyles.foldoutHeader
                );

                if (!groupFoldouts[group.Key]) continue;

                EditorGUI.indentLevel++;
                foreach (LocalizedObject obj in group) {
                    if (!PassesFilter(obj)) continue;
                    DrawLocalizedObject(obj, usedKeys);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawLocalizedObject(LocalizedObject obj, HashSet<string> usedKeys) {
            if (obj == null) return;

            string key = obj.localizationKey ?? "";
            bool hasKey = !string.IsNullOrWhiteSpace(key);
            bool duplicate = hasKey && !usedKeys.Add(key);

            Color bg = !hasKey ? new Color(1f, .5f, .5f, .3f)
                : duplicate ? new Color(1f, 1f, .4f, .3f)
                : new Color(.4f, 1f, .4f, .25f);

            GUI.backgroundColor = bg;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            if (!objectFoldouts.ContainsKey(obj)) objectFoldouts[obj] = false;

            EditorGUILayout.BeginHorizontal();
            objectFoldouts[obj] = EditorGUILayout.Foldout(objectFoldouts[obj], GetObjectPath(obj), true);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select Object", GUILayout.Width(110))) {
                Selection.activeObject = obj.gameObject;
                EditorGUIUtility.PingObject(obj.gameObject);
            }

            EditorGUILayout.EndHorizontal();

            if (objectFoldouts[obj]) {
                EditorGUI.indentLevel++;

                string displayText = GetComponentDisplayedText(obj);
                GUIStyle txtStyle = new GUIStyle(EditorStyles.label) {
                    wordWrap = true,
                    fontStyle = FontStyle.Italic,
                    normal = {
                        textColor = Color.white
                    },
                };
                
                EditorGUILayout.LabelField("Current UI Text:", txtStyle, GUILayout.Width(100));
                EditorGUILayout.SelectableLabel(displayText, txtStyle, GUILayout.ExpandWidth(true));
                
                EditorGUILayout.Space(4);

                EditorGUI.BeginChangeCheck();
                obj.localizationKey = EditorGUILayout.TextField("Localization Key", obj.localizationKey);
                obj.beforeText = EditorGUILayout.TextField("Before Text", obj.beforeText);
                obj.afterText = EditorGUILayout.TextField("After Text", obj.afterText);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(obj, "Edit Localization");
                    EditorUtility.SetDirty(obj);
                }

                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();

                if (string.IsNullOrWhiteSpace(obj.localizationKey)) {
                    if (GUILayout.Button("Generate AutoKey", GUILayout.Width(140))) {
                        Undo.RecordObject(obj, "Generate AutoKey");
                        obj.localizationKey = GenerateAutoKey(obj);
                        EditorUtility.SetDirty(obj);
                    }
                }

                if (GUILayout.Button("Preview Translation", GUILayout.Width(160))) ShowPreview(obj);

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(4);
        }

#endregion

#region Helpers

        private void ShowPreview(LocalizedObject obj) {
            if (string.IsNullOrWhiteSpace(obj.localizationKey)) {
                EditorUtility.DisplayDialog("Preview", "This object has no localization key.", "OK");
                return;
            }

            string language = availableLanguages[selectedLanguageIndex];
            try {
                string val = AutoLocalizationHandler.GetLocalizedText(obj.localizationKey, language);
                if (string.IsNullOrEmpty(val)) val = $"Key '{obj.localizationKey}' not found in {language}.";
                EditorUtility.DisplayDialog($"[{language}]", val, "Close");
            } catch {
                EditorUtility.DisplayDialog("Preview Error",
                    $"Unable to show translation for '{obj.localizationKey}' in language '{language}'.", "OK"
                );
            }
        }

        private string GetGroupName(LocalizedObject obj) {
            Canvas canvas = obj.GetComponentInParent<Canvas>();
            return canvas ? canvas.name : "Scene Root";
        }

        private string GetObjectPath(LocalizedObject obj) {
            if (!obj.transform.parent) return obj.name;
            return $"{obj.transform.parent.name}/{obj.name}";
        }

        private string GetComponentDisplayedText(LocalizedObject obj) {
            TMP_Text tmp = obj.GetComponent<TMP_Text>();
            if (tmp) return tmp.text;

            Text txt = obj.GetComponent<Text>();
            if (txt) return txt.text;

            return "[No Text Component]";
        }

        private string GenerateAutoKey(LocalizedObject obj) {
            string parent = obj.transform.parent ? obj.transform.parent.name.ToLower() : "root";
            string objName = obj.name.ToLower();
            return $"{parent}_{objName}";
        }

        private bool PassesFilter(LocalizedObject obj) {
            if (string.IsNullOrEmpty(searchFilter)) return true;
            string f = searchFilter.ToLower();
            return obj.name.ToLower().Contains(f)
                   || obj.localizationKey.ToLower().Contains(f)
                   || GetComponentDisplayedText(obj).ToLower().Contains(f);
        }

#endregion
    }
}

#endif