#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoLocalization.Editor
{
    public class LocalizationJsonEditor : EditorWindow
    {
        [Serializable]
        private class GroupsConfig{
            public List<GroupData> groups = new List<GroupData>();

            [Serializable]
            public class GroupData{
                public string name;
                public List<string> keys = new List<string>();
                public bool expanded = true;
            }
        }

        private GroupsConfig groupsConfig;

        private string draggingKey;
        private string draggingGroup;
        private Vector2 dragStartPos;
        private Rect draggingRect;
        private bool isDragging;

        private string draggingGroupName;
        private bool isDraggingGroup;
        private Rect draggingGroupRect;

        private string dropTargetGroup;
        private string dropTargetKey;
        private bool showDropLine;
        private float dropLineY;

        private string dropTargetGroupName;
        private bool showGroupDropLine;
        private float groupDropLineY;

        private Dictionary<string, Rect> groupRects = new Dictionary<string, Rect>();
        private Dictionary<string, Rect> keyRects = new Dictionary<string, Rect>();

        private List<string> availableLanguages = new List<string>();
        private int selectedLangIndex;
        private Dictionary<string, string> currentData = new Dictionary<string, string>();
        private Vector2 scroll;
        private string search = "";
        private string newKey = "";
        private string newValue = "";
        private string newKeyGroup = "Ungrouped";

        private HashSet<string> changedKeys = new HashSet<string>();
        private HashSet<string> newKeys = new HashSet<string>();

        private double lastEditTime;
        private const float AutoSaveDelay = 1.5f;

        private bool affectAllLanguages = true;
        private bool autoTranslateNewKeys = true;
        private int autoTranslateSourceIndex = 0;

        private static Dictionary<string, Dictionary<string, string>> _languagesCache =
            new Dictionary<string, Dictionary<string, string>>();

        private bool needsRepaint;
        private string groupToDelete;
        private string keyToDelete;
        private bool shouldDeleteKey;

        [MenuItem("Auto Localization/JSON Editor")]
        private static void Open() {
            LocalizationJsonEditor wnd = GetWindow<LocalizationJsonEditor>("Localization JSON Editor");
            wnd.minSize = new Vector2(650, 500);
        }

        private void OnEnable() {
            RefreshLanguageList();
            LoadCurrentLanguage();
            EditorApplication.update += CheckAutoSave;
            UpdateAutoTranslateOptions();
            LoadGroupsConfig();
        }

        private void OnDisable() {
            if (changedKeys.Count > 0 || newKeys.Count > 0) SaveAllLanguages();
            EditorApplication.update -= CheckAutoSave;
            SaveGroupsConfig();
        }

        private void LoadGroupsConfig() {
            // Загружаем конфигурацию групп из Resources
            TextAsset groupsAsset = Resources.Load<TextAsset>("LocalizationGroups");
            if (groupsAsset != null) {
                groupsConfig = JsonUtility.FromJson<GroupsConfig>(groupsAsset.text);
            }
            
            if (groupsConfig == null) {
                CreateDefaultGroupsConfig();
            }

            SyncGroupsWithKeys();
        }

        private void CreateDefaultGroupsConfig() {
            groupsConfig = new GroupsConfig();
            groupsConfig.groups.Add(new GroupsConfig.GroupData {
                    name = "Ungrouped", expanded = true
                }
            );
            SaveGroupsConfig();
        }

        private void SaveGroupsConfig() {
            if (groupsConfig == null) return;
            
            string json = JsonUtility.ToJson(groupsConfig, true);
            string path = "Assets/Resources/LocalizationGroups.json";
            
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        private void SyncGroupsWithKeys() {
            if (groupsConfig == null || currentData == null) return;

            HashSet<string> allKeys = new HashSet<string>(currentData.Keys);
            HashSet<string> keysInGroups = new HashSet<string>();

            foreach (GroupsConfig.GroupData group in groupsConfig.groups) {
                for (int i = group.keys.Count - 1; i >= 0; i--) {
                    if (!allKeys.Contains(group.keys[i]))
                        group.keys.RemoveAt(i);
                    else
                        keysInGroups.Add(group.keys[i]);
                }
            }

            GroupsConfig.GroupData ungrouped = GetOrCreateUngrouped();
            foreach (string key in allKeys) {
                if (!keysInGroups.Contains(key) && !ungrouped.keys.Contains(key)) ungrouped.keys.Add(key);
            }
        }

        private GroupsConfig.GroupData GetOrCreateUngrouped() {
            GroupsConfig.GroupData ungrouped = groupsConfig.groups.FirstOrDefault(g => g.name == "Ungrouped");
            if (ungrouped == null) {
                ungrouped = new GroupsConfig.GroupData {
                    name = "Ungrouped", expanded = true
                };
                groupsConfig.groups.Insert(0, ungrouped);
            }

            return ungrouped;
        }

        private void CheckAutoSave() {
            if ((changedKeys.Count > 0 || newKeys.Count > 0) &&
                EditorApplication.timeSinceStartup - lastEditTime > AutoSaveDelay)
                SaveAllLanguages();
        }

        private void RefreshLanguageList() {
            availableLanguages.Clear();
            
            // Загружаем все языки через Resources
            TextAsset[] languageAssets = Resources.LoadAll<TextAsset>("Languages");
            
            foreach (TextAsset asset in languageAssets) {
                string fileName = asset.name;
                if (!availableLanguages.Contains(fileName)) {
                    availableLanguages.Add(fileName);
                }
            }

            if (availableLanguages.Count == 0) {
                availableLanguages.Add("en");
                CreateDefaultLanguageFile("en");
            }

            selectedLangIndex = Mathf.Clamp(selectedLangIndex, 0, availableLanguages.Count - 1);
            UpdateAutoTranslateOptions();
        }

        private void CreateDefaultLanguageFile(string langCode) {
            string path = $"Assets/Resources/Languages/{langCode}.json";
            System.IO.File.WriteAllText(path, "{}");
            AssetDatabase.Refresh();
        }

        public void RefreshCurrentLanguage() {
            _languagesCache.Clear();
            RefreshLanguageList();
            LoadCurrentLanguage();
            Repaint();
        }

        private void UpdateAutoTranslateOptions() {
            if (availableLanguages.Count <= 1) {
                autoTranslateNewKeys = false;
                autoTranslateSourceIndex = 0;
            } else
                autoTranslateSourceIndex = selectedLangIndex == 0 ? 1 : 0;
        }

        private void LoadCurrentLanguage() {
            currentData.Clear();
            TextAsset textAsset = Resources.Load<TextAsset>($"Languages/{availableLanguages[selectedLangIndex]}");
            
            if (textAsset != null) {
                try {
                    currentData = JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text) ??
                                  new Dictionary<string, string>();
                } catch { currentData = new Dictionary<string, string>(); }
            }

            SyncGroupsWithKeys();
            changedKeys.Clear();
            newKeys.Clear();
        }

        private void SaveAllLanguages() {
            if (currentData == null) return;
            
            if (!affectAllLanguages) {
                SaveSingleLanguage(availableLanguages[selectedLangIndex], currentData);
                changedKeys.Clear();
                newKeys.Clear();
                Repaint();
                return;
            }

            Dictionary<string, Dictionary<string, string>> all = LoadAllLanguages();
            all[availableLanguages[selectedLangIndex]] = new Dictionary<string, string>(currentData);

            foreach (string lang in availableLanguages) {
                if (lang == availableLanguages[selectedLangIndex]) continue;
                Dictionary<string, string> dict = all[lang];
                foreach (string key in currentData.Keys) {
                    if (!dict.ContainsKey(key)) dict[key] = autoTranslateNewKeys ? "" : currentData[key];
                }
            }

            foreach (string lang in availableLanguages) SaveSingleLanguage(lang, all[lang]);

            if (autoTranslateNewKeys && newKeys.Count > 0) ProcessAutoTranslateForNewKeys();

            changedKeys.Clear();
            newKeys.Clear();
            Repaint();
        }

        private Dictionary<string, Dictionary<string, string>> LoadAllLanguages() {
            Dictionary<string, Dictionary<string, string>>
                result = new Dictionary<string, Dictionary<string, string>>();
                
            foreach (string lang in availableLanguages) {
                TextAsset textAsset = Resources.Load<TextAsset>($"Languages/{lang}");
                
                if (textAsset == null)
                    result[lang] = new Dictionary<string, string>();
                else {
                    try {
                        if (_languagesCache.TryGetValue(lang, out Dictionary<string, string> cached))
                            result[lang] = new Dictionary<string, string>(cached);
                        else {
                            result[lang] = JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text) ??
                                           new Dictionary<string, string>();
                            _languagesCache[lang] = new Dictionary<string, string>(result[lang]);
                        }
                    } catch { result[lang] = new Dictionary<string, string>(); }
                }
            }

            return result;
        }

        private async void ProcessAutoTranslateForNewKeys() {
            List<string> keysToTranslate = new List<string>(newKeys);

            foreach (string key in keysToTranslate) {
                if (!currentData.ContainsKey(key)) continue;

                string sourceText = currentData[key];
                if (string.IsNullOrEmpty(sourceText)) continue;

                string sourceLang = availableLanguages[selectedLangIndex];

                foreach (string targetLang in availableLanguages) {
                    if (targetLang == sourceLang) continue;

                    try {
                        string translated = await Translator.AutoTranslator.TranslateAsync(
                            sourceText, sourceLang, targetLang
                        );

                        if (_languagesCache.ContainsKey(targetLang)) {
                            _languagesCache[targetLang][key] = translated;
                        }

                        UpdateKeyInLanguageFile(targetLang, key, translated);
                    } catch (Exception e) {
                        Debug.LogError($"Auto-translate failed for key '{key}' to '{targetLang}': {e.Message}");
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        private void UpdateKeyInLanguageFile(string lang, string key, string value) {
            TextAsset textAsset = Resources.Load<TextAsset>($"Languages/{lang}");
            
            if (textAsset == null) return;

            try {
                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text)
                                                  ?? new Dictionary<string, string>();

                data[key] = value;

                string newJson = JsonConvert.SerializeObject(data, Formatting.Indented);
                
                // Обновляем файл
                string assetPath = AssetDatabase.GetAssetPath(textAsset);
                System.IO.File.WriteAllText(assetPath, newJson);
                EditorUtility.SetDirty(textAsset);
                AssetDatabase.SaveAssets();

                if (_languagesCache.ContainsKey(lang)) _languagesCache[lang] = data;
            } catch (Exception e) { 
                Debug.LogError($"Failed to update key in language '{lang}': {e.Message}"); 
            }
        }

        private void SaveSingleLanguage(string lang, Dictionary<string, string> data) {
            string path = $"Assets/Resources/Languages/{lang}.json";
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            _languagesCache[lang] = new Dictionary<string, string>(data);
        }

        private void OnGUI() {
            ProcessDeferredOperations();

            HandleEvents();
            DrawToolbar();
            DrawTranslationSettings();
            DrawKeyValueList();
            DrawAddNewSection();

            if (needsRepaint) {
                needsRepaint = false;
                Repaint();
            }
        }

        private void ProcessDeferredOperations() {
            if (!string.IsNullOrEmpty(groupToDelete)) {
                GroupsConfig.GroupData groupToDeleteObj =
                    groupsConfig.groups.FirstOrDefault(g => g.name == groupToDelete);
                if (groupToDeleteObj != null) {
                    GroupsConfig.GroupData ungrouped = GetOrCreateUngrouped();
                    foreach (string key in groupToDeleteObj.keys) {
                        if (!ungrouped.keys.Contains(key)) ungrouped.keys.Add(key);
                    }

                    groupsConfig.groups.Remove(groupToDeleteObj);
                    SaveGroupsConfig();
                }

                groupToDelete = null;
                needsRepaint = true;
            }

            if (shouldDeleteKey && !string.IsNullOrEmpty(keyToDelete)) {
                if (affectAllLanguages)
                    DeleteKeyInAllLanguages(keyToDelete);
                else
                    DeleteKeyInSingleLanguage(keyToDelete);

                keyToDelete = null;
                shouldDeleteKey = false;
                needsRepaint = true;
            }
        }

        private void HandleEvents() {
            Event e = Event.current;

            if (isDragging && !isDraggingGroup) {
                draggingRect.position = e.mousePosition - new Vector2(100, 15);

                if (e.mousePosition.y < 50) {
                    scroll.y -= 10;
                    needsRepaint = true;
                } else if (e.mousePosition.y > position.height - 50) {
                    scroll.y += 10;
                    needsRepaint = true;
                }

                if (e.type == EventType.Repaint) DrawDragPreview();

                if (e.type == EventType.MouseUp) {
                    HandleKeyDrop();
                    isDragging = false;
                    draggingKey = null;
                    draggingGroup = null;
                    dropTargetGroup = null;
                    dropTargetKey = null;
                    showDropLine = false;
                    needsRepaint = true;
                    e.Use();
                }

                if (e.type == EventType.MouseDrag) needsRepaint = true;
            }

            if (isDraggingGroup && !isDragging) {
                draggingGroupRect.position = e.mousePosition - new Vector2(100, 15);

                if (e.mousePosition.y < 50) {
                    scroll.y -= 10;
                    needsRepaint = true;
                } else if (e.mousePosition.y > position.height - 50) {
                    scroll.y += 10;
                    needsRepaint = true;
                }

                if (e.type == EventType.Repaint) DrawGroupDragPreview();

                if (e.type == EventType.MouseUp) {
                    HandleGroupDrop();
                    isDraggingGroup = false;
                    draggingGroupName = null;
                    dropTargetGroupName = null;
                    showGroupDropLine = false;
                    needsRepaint = true;
                    e.Use();
                }

                if (e.type == EventType.MouseDrag) needsRepaint = true;
            }

            if (!isDragging && showDropLine) {
                showDropLine = false;
                needsRepaint = true;
            }

            if (!isDraggingGroup && showGroupDropLine) {
                showGroupDropLine = false;
                needsRepaint = true;
            }
        }

        private void DrawDragPreview() {
            Color originalColor = GUI.color;
            GUI.color = new Color(0.2f, 0.6f, 1f, 0.7f);
            GUI.Box(draggingRect, "", EditorStyles.helpBox);

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(EditorStyles.whiteBoldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(draggingRect.x + 5, draggingRect.y + 5, draggingRect.width - 10, 20),
                $"Moving: {draggingKey}", style
            );

            GUI.color = originalColor;
        }

        private void DrawGroupDragPreview() {
            Color originalColor = GUI.color;
            GUI.color = new Color(0.6f, 0.2f, 1f, 0.7f);
            GUI.Box(draggingGroupRect, "", EditorStyles.helpBox);

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(EditorStyles.whiteBoldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(draggingGroupRect.x + 5, draggingGroupRect.y + 5, draggingGroupRect.width - 10, 20),
                $"Moving Group: {draggingGroupName}", style
            );

            GUI.color = originalColor;
        }

        private void HandleKeyDrop() {
            if (string.IsNullOrEmpty(dropTargetGroup)) return;

            if (dropTargetGroup == draggingGroup) {
                MoveKeyWithinGroup(draggingKey, dropTargetGroup, dropTargetKey);
            } else {
                MoveKeyToGroup(draggingKey, draggingGroup, dropTargetGroup, dropTargetKey);
            }

            SaveGroupsConfig();
        }

        private void HandleGroupDrop() {
            if (string.IsNullOrEmpty(dropTargetGroupName) || dropTargetGroupName == draggingGroupName) return;

            MoveGroup(draggingGroupName, dropTargetGroupName);
            SaveGroupsConfig();
        }

        private void MoveKeyWithinGroup(string key, string groupName, string insertAfterKey) {
            GroupsConfig.GroupData group = groupsConfig.groups.FirstOrDefault(g => g.name == groupName);
            if (group == null) return;

            group.keys.Remove(key);

            int newIndex = 0;
            if (!string.IsNullOrEmpty(insertAfterKey)) {
                int insertIndex = group.keys.IndexOf(insertAfterKey);
                if (insertIndex >= 0) newIndex = insertIndex + 1;
            }

            if (newIndex > group.keys.Count) newIndex = group.keys.Count;
            group.keys.Insert(newIndex, key);
        }

        private void MoveKeyToGroup(string key, string fromGroup, string toGroup, string insertAfterKey) {
            if (string.IsNullOrEmpty(fromGroup) || string.IsNullOrEmpty(toGroup)) return;

            GroupsConfig.GroupData oldGroup = groupsConfig.groups.FirstOrDefault(g => g.name == fromGroup);
            if (oldGroup != null) oldGroup.keys.Remove(key);

            GroupsConfig.GroupData newGroup = groupsConfig.groups.FirstOrDefault(g => g.name == toGroup);
            if (newGroup != null) {
                if (!newGroup.keys.Contains(key)) {
                    int newIndex = newGroup.keys.Count;

                    if (!string.IsNullOrEmpty(insertAfterKey)) {
                        int targetIndex = newGroup.keys.IndexOf(insertAfterKey);
                        if (targetIndex >= 0) newIndex = targetIndex + 1;
                    }

                    if (newIndex > newGroup.keys.Count) newIndex = newGroup.keys.Count;
                    newGroup.keys.Insert(newIndex, key);
                }
            }
        }

        private void MoveGroup(string groupName, string insertAfterGroupName) {
            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(insertAfterGroupName)) return;

            GroupsConfig.GroupData groupToMove = groupsConfig.groups.FirstOrDefault(g => g.name == groupName);
            if (groupToMove == null) return;

            int targetIndex = groupsConfig.groups.FindIndex(g => g.name == insertAfterGroupName);
            if (targetIndex < 0) return;

            groupsConfig.groups.Remove(groupToMove);

            int newIndex = targetIndex + 1;

            int currentIndex = groupsConfig.groups.FindIndex(g => g.name == groupName);
            if (currentIndex < targetIndex) newIndex = targetIndex;

            if (newIndex > groupsConfig.groups.Count) newIndex = groupsConfig.groups.Count;
            groupsConfig.groups.Insert(newIndex, groupToMove);
        }

        private void DrawToolbar() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUIStyle searchStyle = GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.toolbarTextField;
            search = GUILayout.TextField(search, searchStyle, GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();
            selectedLangIndex =
                EditorGUILayout.Popup(selectedLangIndex, availableLanguages.ToArray(), GUILayout.Width(120));
            if (EditorGUI.EndChangeCheck()) {
                LoadCurrentLanguage();
                UpdateAutoTranslateOptions();
            }

            if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(70))) RefreshCurrentLanguage();

            if (GUILayout.Button("Translate Language", EditorStyles.toolbarButton, GUILayout.Width(130)))
                TranslateLanguageWindow.Open(this, availableLanguages[selectedLangIndex]);

            if (GUILayout.Button("New Language", EditorStyles.toolbarButton, GUILayout.Width(120)))
                NewLanguageWindow.Open(this);

            EditorGUI.BeginDisabledGroup(availableLanguages.Count <= 1);
            if (GUILayout.Button("Delete Language", EditorStyles.toolbarButton, GUILayout.Width(120)))
                DeleteCurrentLanguage();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DeleteCurrentLanguage() {
            if (availableLanguages.Count <= 1) return;

            string languageToDelete = availableLanguages[selectedLangIndex];
            if (!EditorUtility.DisplayDialog("Delete Language",
                    $"Are you sure you want to delete language '{languageToDelete}'?",
                    "Delete", "Cancel"
                ))
                return;

            try {
                string path = $"Assets/Resources/Languages/{languageToDelete}.json";
                AssetDatabase.DeleteAsset(path);

                _languagesCache.Remove(languageToDelete);
                RefreshLanguageList();
                LoadCurrentLanguage();
            } catch (Exception e) {
                EditorUtility.DisplayDialog("Deletion Error", $"Failed to delete language: {e.Message}", "OK");
            }
        }

        private void DrawTranslationSettings() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            affectAllLanguages = EditorGUILayout.ToggleLeft("💬 Affect All Languages", affectAllLanguages);

            EditorGUI.BeginDisabledGroup(!affectAllLanguages || availableLanguages.Count <= 1);
            autoTranslateNewKeys = EditorGUILayout.ToggleLeft("🤖 Auto-translate new keys", autoTranslateNewKeys);

            if (autoTranslateNewKeys && affectAllLanguages && availableLanguages.Count > 1) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Source language for auto-translation:", GUILayout.Width(200));
                autoTranslateSourceIndex = EditorGUILayout.Popup(autoTranslateSourceIndex, availableLanguages.ToArray(),
                    GUILayout.Width(100)
                );
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private void DrawKeyValueList() {
            if (currentData == null || currentData.Count == 0) {
                EditorGUILayout.HelpBox("JSON file is empty or not loaded.", MessageType.Info);
                return;
            }

            if (groupsConfig == null || groupsConfig.groups.Count == 0) {
                EditorGUILayout.HelpBox("No groups configuration loaded.", MessageType.Warning);
                return;
            }

            groupRects.Clear();
            keyRects.Clear();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("➕ Create New Group", GUILayout.Width(150))) ShowCreateGroupDialog();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            List<GroupsConfig.GroupData> groupsCopy = groupsConfig.groups.ToList();

            for (int groupIndex = 0; groupIndex < groupsCopy.Count; groupIndex++) {
                GroupsConfig.GroupData group = groupsCopy[groupIndex];
                List<string> groupKeys = group.keys
                    .Where(key => currentData.ContainsKey(key))
                    .Where(key => string.IsNullOrEmpty(search) ||
                                  key.ToLower().Contains(search.ToLower()) ||
                                  currentData[key].ToLower().Contains(search.ToLower())
                    )
                    .ToList();

                if (groupKeys.Count == 0 && !string.IsNullOrEmpty(search)) continue;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                Rect groupDragRect =
                    GUILayoutUtility.GetRect(20, EditorGUIUtility.singleLineHeight, GUILayout.Width(20));
                EditorGUIUtility.AddCursorRect(groupDragRect, MouseCursor.Pan);
                GUI.Label(groupDragRect, "⇅", EditorStyles.miniLabel);

                group.expanded = EditorGUILayout.Foldout(group.expanded, $"{group.name} ({groupKeys.Count})", true);

                if (group.name != "Ungrouped") {
                    if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20))) {
                        if (EditorUtility.DisplayDialog("Delete Group",
                                $"Delete group '{group.name}'? All keys will be moved to 'Ungrouped'.",
                                "Delete", "Cancel"
                            )) {
                            
                            groupToDelete = group.name;
                            needsRepaint = true;
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();

                Rect groupRect = GUILayoutUtility.GetLastRect();
                groupRects[group.name] = groupRect;

                Event e = Event.current;

                if (e.type == EventType.MouseDown && groupDragRect.Contains(e.mousePosition) && e.button == 0) {
                    draggingGroupName = group.name;
                    dragStartPos = e.mousePosition;
                    draggingGroupRect = new Rect(e.mousePosition.x - 120, e.mousePosition.y - 15, 240, 30);
                    isDraggingGroup = true;
                    needsRepaint = true;
                    e.Use();
                }

                if (isDraggingGroup && groupRect.Contains(e.mousePosition) && group.name != draggingGroupName) {
                    dropTargetGroupName = group.name;

                    if (e.mousePosition.y < groupRect.y + groupRect.height * 0.5f) {
                        groupDropLineY = groupRect.y;
                    } else {
                        groupDropLineY = groupRect.y + groupRect.height;
                    }

                    showGroupDropLine = true;
                    needsRepaint = true;
                }

                if (isDragging && groupRect.Contains(e.mousePosition)) {
                    dropTargetGroup = group.name;
                    dropTargetKey = null;

                    if (e.type == EventType.Repaint) EditorGUI.DrawRect(groupRect, new Color(0.2f, 0.6f, 1f, 0.3f));
                }

                if (showGroupDropLine && group.name == dropTargetGroupName) {
                    Rect lineRect = new Rect(10, groupDropLineY, position.width - 40, 3);
                    if (e.type == EventType.Repaint) EditorGUI.DrawRect(lineRect, new Color(0.6f, 0.2f, 1f, 0.8f));
                }

                if (group.expanded) {
                    if (groupKeys.Count == 0 && !string.IsNullOrEmpty(search))
                        EditorGUILayout.LabelField("  No keys match search", EditorStyles.helpBox);

                    for (int i = 0; i < groupKeys.Count; i++) {
                        string key = groupKeys[i];
                        DrawKeyRow(key, group.name, i);

                        Rect keyRect = GUILayoutUtility.GetLastRect();
                        keyRects[key] = keyRect;

                        if (isDragging && keyRect.Contains(e.mousePosition) && key != draggingKey) {
                            dropTargetGroup = group.name;

                            if (e.mousePosition.y < keyRect.y + keyRect.height * 0.5f) {
                                dropTargetKey = i > 0 ? groupKeys[i - 1] : null;
                                dropLineY = keyRect.y;
                            } else {
                                dropTargetKey = key;
                                dropLineY = keyRect.y + keyRect.height;
                            }

                            showDropLine = true;
                            needsRepaint = true;
                        }
                    }

                    if (showDropLine && dropTargetGroup == group.name) {
                        Rect lineRect = new Rect(30, dropLineY, position.width - 60, 2);
                        if (e.type == EventType.Repaint) EditorGUI.DrawRect(lineRect, new Color(0.2f, 0.6f, 1f, 0.8f));
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void ShowCreateGroupDialog() {
            EditorInputDialog.Show("Create New Group", "", OnGroupNameEntered, null);
        }

        private void OnGroupNameEntered(string groupName) {
            if (!string.IsNullOrEmpty(groupName)) {
                if (groupsConfig.groups.Any(g => g.name == groupName))
                    EditorUtility.DisplayDialog("Error", $"Group '{groupName}' already exists!", "OK");
                else {
                    groupsConfig.groups.Add(new GroupsConfig.GroupData {
                            name = groupName, expanded = true, keys = new List<string>()
                        }
                    );

                    SaveGroupsConfig();
                    needsRepaint = true;
                    Repaint();
                }
            }
        }

        private void DrawKeyRow(string key, string groupName, int index) {
            if (!currentData.ContainsKey(key)) return;

            string value = currentData[key];

            Color bg = newKeys.Contains(key) ? new Color(0.5f, 1f, 0.5f, 0.25f) :
                changedKeys.Contains(key) ? new Color(1f, 1f, 0.4f, 0.25f) :
                new Color(1f, 1f, 1f, 0.08f);

            GUI.backgroundColor = bg;
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();

            Rect dragRect = GUILayoutUtility.GetRect(20, EditorGUIUtility.singleLineHeight, GUILayout.Width(20));
            EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.Pan);
            GUI.Label(dragRect, "☰", EditorStyles.miniLabel);

            Event e = Event.current;
            if (e.type == EventType.MouseDown && dragRect.Contains(e.mousePosition) && e.button == 0) {
                draggingKey = key;
                draggingGroup = groupName;
                dragStartPos = e.mousePosition;
                draggingRect = new Rect(e.mousePosition.x - 100, e.mousePosition.y - 15, 200, 30);
                isDragging = true;
                needsRepaint = true;
                e.Use();
            }

            EditorGUILayout.SelectableLabel(key, GUILayout.Width(200), GUILayout.Height(15));

            EditorGUI.BeginChangeCheck();
            
            string newVal = EditorGUILayout.TextArea(value, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck() && newVal != value) {
                currentData[key] = newVal;
                changedKeys.Add(key);
                lastEditTime = EditorApplication.timeSinceStartup;
            }

            EditorGUI.BeginDisabledGroup(availableLanguages.Count <= 1);
            if (GUILayout.Button("🌐", GUILayout.Width(25))) ShowTranslateKeyMenu(key);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("❌", GUILayout.Width(25))) {
                if (EditorUtility.DisplayDialog("Delete Key",
                        $"Delete '{key}' in {(affectAllLanguages ? "all languages" : "this language")}?",
                        "Delete", "Cancel"
                    )) {
                    
                    keyToDelete = key;
                    shouldDeleteKey = true;
                    needsRepaint = true;
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void ShowTranslateKeyMenu(string key) {
            GenericMenu menu = new GenericMenu();
            string currentLang = availableLanguages[selectedLangIndex];

            foreach (string lang in availableLanguages) {
                if (lang == currentLang) continue;
                menu.AddItem(new GUIContent($"Translate from {lang}"), false,
                    () => TranslateKeyFromLanguage(key, lang)
                );
            }

            menu.ShowAsContext();
        }

        private async void TranslateKeyFromLanguage(string key, string sourceLang) {
            if (!currentData.ContainsKey(key)) return;

            string sourceText = GetKeyFromLanguage(key, sourceLang);
            if (string.IsNullOrEmpty(sourceText)) return;

            try {
                string translated = await Translator.AutoTranslator.TranslateAsync(
                    sourceText, sourceLang, availableLanguages[selectedLangIndex]
                );

                currentData[key] = translated;
                changedKeys.Add(key);
                lastEditTime = EditorApplication.timeSinceStartup;
                SaveSingleLanguage(availableLanguages[selectedLangIndex], currentData);
                Repaint();
            } catch (Exception e) { Debug.LogError($"Translation failed: {e.Message}"); }
        }

        private string GetKeyFromLanguage(string key, string langCode) {
            TextAsset textAsset = Resources.Load<TextAsset>($"Languages/{langCode}");
            if (textAsset == null) return "";

            try {
                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text);
                return data?.ContainsKey(key) == true ? data[key] : "";
            } catch { return ""; }
        }

        private void DeleteKeyInSingleLanguage(string key) {
            currentData.Remove(key);
            foreach (GroupsConfig.GroupData group in groupsConfig.groups) group.keys.Remove(key);
            SaveGroupsConfig();
            SaveSingleLanguage(availableLanguages[selectedLangIndex], currentData);
            AssetDatabase.Refresh();
            lastEditTime = EditorApplication.timeSinceStartup;
        }

        private void DeleteKeyInAllLanguages(string key) {
            Dictionary<string, Dictionary<string, string>> all = LoadAllLanguages();
            foreach (string lang in availableLanguages) {
                if (all[lang].ContainsKey(key)) {
                    all[lang].Remove(key);
                    SaveSingleLanguage(lang, all[lang]);
                }
            }

            currentData.Remove(key);
            foreach (GroupsConfig.GroupData group in groupsConfig.groups) group.keys.Remove(key);
            SaveGroupsConfig();
            AssetDatabase.Refresh();
            lastEditTime = EditorApplication.timeSinceStartup;
        }

        private void DrawAddNewSection() {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("➕ Add New Entry", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (groupsConfig != null && groupsConfig.groups.Count > 0) {
                string[] groupNames = groupsConfig.groups.Select(g => g.name).ToArray();
                int currentIndex = Array.IndexOf(groupNames, newKeyGroup);
                if (currentIndex < 0) currentIndex = 0;

                newKeyGroup = groupNames[EditorGUILayout.Popup("Group:", currentIndex, groupNames)];
            }

            newKey = EditorGUILayout.TextField("Key:", newKey);
            // newValue = EditorGUILayout.TextField("Value:", newValue);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value:", GUILayout.Width(EditorGUIUtility.labelWidth));
            newValue = EditorGUILayout.TextArea(newValue, GUILayout.Height(60));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool disabled = string.IsNullOrWhiteSpace(newKey) || string.IsNullOrWhiteSpace(newValue);
            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("Add", GUILayout.Width(120))) {
                if (!currentData.ContainsKey(newKey))
                    AddNewKey();
                else
                    EditorUtility.DisplayDialog("Duplicate Key", $"Key '{newKey}' already exists.", "OK");
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void AddNewKey() {
            if (affectAllLanguages)
                AddKeyToAllLanguages(newKey, newValue);
            else
                AddKeyToCurrentLanguage(newKey, newValue);

            GroupsConfig.GroupData targetGroup = groupsConfig.groups.FirstOrDefault(g => g.name == newKeyGroup);
            if (targetGroup != null && !targetGroup.keys.Contains(newKey)) {
                targetGroup.keys.Add(newKey);
                SaveGroupsConfig();
            }

            newKeys.Add(newKey);
            newKey = "";
            newValue = "";
            lastEditTime = EditorApplication.timeSinceStartup;
        }

        private void AddKeyToCurrentLanguage(string key, string value) {
            currentData[key] = value;
            SaveSingleLanguage(availableLanguages[selectedLangIndex], currentData);
            AssetDatabase.Refresh();
        }

        private void AddKeyToAllLanguages(string key, string value) {
            Dictionary<string, Dictionary<string, string>> all = LoadAllLanguages();
            foreach (string lang in availableLanguages) {
                if (!all[lang].ContainsKey(key)) {
                    all[lang][key] = lang == availableLanguages[selectedLangIndex] || !autoTranslateNewKeys
                        ? value
                        : "";
                    SaveSingleLanguage(lang, all[lang]);
                }
            }

            currentData[key] = value;
            AssetDatabase.Refresh();
        }

        public void RefreshFromNewLanguage(string newLanguageCode) {
            _languagesCache.Clear();
            RefreshLanguageList();

            int index = availableLanguages.IndexOf(newLanguageCode);
            if (index >= 0) {
                selectedLangIndex = index;
                LoadCurrentLanguage();
            }

            Repaint();
        }
    }
}

#endif