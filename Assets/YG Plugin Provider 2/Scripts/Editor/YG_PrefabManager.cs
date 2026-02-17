using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class YG_PrefabManager : EditorWindow
{
    private string rootPath = "Assets/Prefabs";
    private string currentPath = "";
    private Vector2 scrollPosition;
    private string searchText = "";
    private Dictionary<string, Texture2D> previewCache = new Dictionary<string, Texture2D>();
    private float iconSize = 80f;
    private float spacing = 15f;
    private List<string> breadcrumbs = new List<string>();
    private double lastClickTime;
    private string lastClickedItem;
    private string selectedItem = "";
    private string hoveredItem = "";
    private string dragItem = "";
    private Vector2 dragStartPos;
    private bool isDragging = false;

    [MenuItem("YG Plugin Provider/Prefab Manager")]
    public static void ShowWindow() {
        GetWindow<YG_PrefabManager>("YG Prefab Manager");
    }

    private void OnGUI() {
        if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragExited) {
            isDragging = false;
            dragItem = "";
            Repaint();
        }

        DrawToolbar();
        DrawBreadcrumbs();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawContent();
        EditorGUILayout.EndScrollView();

        DrawDragPreview();
    }

    private void DrawToolbar() {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("←", EditorStyles.toolbarButton, GUILayout.Width(30))) GoBack();

        if (GUILayout.Button("🏠", EditorStyles.toolbarButton, GUILayout.Width(30))) {
            currentPath = "";
            breadcrumbs.Clear();
            selectedItem = "";
            RefreshPreviews();
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80))) RefreshPreviews();

        EditorGUILayout.Space(10);

        GUILayout.Label("Search:", GUILayout.Width(50));
        searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);

        GUILayout.FlexibleSpace();

        GUILayout.Label("Size:", GUILayout.Width(35));
        iconSize = EditorGUILayout.Slider(iconSize, 50f, 150f, GUILayout.Width(150));

        EditorGUILayout.EndHorizontal();
    }

    private void DrawBreadcrumbs() {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Path:", GUILayout.Width(40));

        if (GUILayout.Button("Groups", EditorStyles.miniButton)) {
            currentPath = "";
            breadcrumbs.Clear();
            selectedItem = "";
        }

        for (int i = 0; i < breadcrumbs.Count; i++) {
            GUILayout.Label("/", GUILayout.Width(10));
            if (GUILayout.Button(breadcrumbs[i], EditorStyles.miniButton)) {
                currentPath = string.Join("/", breadcrumbs.Take(i + 1));
                breadcrumbs = breadcrumbs.Take(i + 1).ToList();
                selectedItem = "";
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawContent() {
        string fullPath = Path.Combine(rootPath, currentPath);

        if (!Directory.Exists(fullPath)) {
            EditorGUILayout.HelpBox($"Folder {fullPath} not found!", MessageType.Error);
            if (GUILayout.Button("Create Prefabs Folder")) {
                Directory.CreateDirectory(rootPath);
                AssetDatabase.Refresh();
            }

            return;
        }

        string[] directories = Directory.GetDirectories(fullPath);
        string[] prefabFiles = Directory.GetFiles(fullPath, "*.prefab", SearchOption.TopDirectoryOnly);

        if (!string.IsNullOrEmpty(searchText)) {
            directories = directories.Where(d =>
                Path.GetFileName(d).ToLower().Contains(searchText.ToLower())
            ).ToArray();
            prefabFiles = prefabFiles.Where(p =>
                Path.GetFileNameWithoutExtension(p).ToLower().Contains(searchText.ToLower())
            ).ToArray();
        }

        directories = directories.OrderBy(d => Path.GetFileName(d)).ToArray();
        prefabFiles = prefabFiles.OrderBy(p => Path.GetFileNameWithoutExtension(p)).ToArray();

        float windowWidth = position.width - 30;
        int columns = Mathf.Max(1, Mathf.FloorToInt(windowWidth / (iconSize + spacing)));

        EditorGUILayout.HelpBox("Click to select | Double-click to edit | Drag prefabs to Scene view", MessageType.Info
        );

        if (directories.Length > 0) {
            GUILayout.Label("Groups", EditorStyles.boldLabel);
            DrawGrid(directories, columns, true);
        }

        if (prefabFiles.Length > 0) {
            if (directories.Length > 0) EditorGUILayout.Space(20);
            GUILayout.Label("Prefabs", EditorStyles.boldLabel);
            DrawGrid(prefabFiles, columns, false);
        }

        if (directories.Length == 0 && prefabFiles.Length == 0) {
            EditorGUILayout.HelpBox(
                string.IsNullOrEmpty(searchText) ? "Folder is empty" : "No items found",
                MessageType.Info
            );
        }
    }

    private void DrawGrid(string[] items, int columns, bool isDirectory) {
        int rowCount = Mathf.CeilToInt(items.Length / (float)columns);

        for (int row = 0; row < rowCount; row++) {
            EditorGUILayout.BeginHorizontal();

            for (int col = 0; col < columns; col++) {
                int index = row * columns + col;
                if (index >= items.Length) break;

                string itemPath = items[index];
                string relativePath = itemPath.Replace("\\", "/").Replace(rootPath + "/", "");
                string itemName = isDirectory ? Path.GetFileName(itemPath) : Path.GetFileNameWithoutExtension(itemPath);

                EditorGUILayout.BeginVertical(GUILayout.Width(iconSize + spacing));

                Texture2D icon = GetIcon(itemPath, isDirectory);

                Rect buttonRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.Width(iconSize),
                    GUILayout.Height(iconSize)
                );

                Color bgColor = Color.clear;

                if (selectedItem == itemPath)
                    bgColor = new Color(0.2f, 0.4f, 0.8f, 0.3f);
                else if (hoveredItem == itemPath)
                    bgColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);

                if (bgColor.a > 0) EditorGUI.DrawRect(buttonRect, bgColor);

                if (selectedItem == itemPath) {
                    EditorGUI.DrawRect(new Rect(buttonRect.x, buttonRect.y, buttonRect.width, 2), Color.blue);
                    EditorGUI.DrawRect(
                        new Rect(buttonRect.x, buttonRect.y + buttonRect.height - 2, buttonRect.width, 2), Color.blue
                    );
                    EditorGUI.DrawRect(new Rect(buttonRect.x, buttonRect.y, 2, buttonRect.height), Color.blue);
                    EditorGUI.DrawRect(
                        new Rect(buttonRect.x + buttonRect.width - 2, buttonRect.y, 2, buttonRect.height), Color.blue
                    );
                }

                HandleEvents(buttonRect, itemPath, isDirectory, relativePath, itemName, icon);

                if (icon != null) {
                    GUI.DrawTexture(
                        new Rect(
                            buttonRect.x + (buttonRect.width - Mathf.Min(buttonRect.width, icon.width)) / 2,
                            buttonRect.y + (buttonRect.height - Mathf.Min(buttonRect.height, icon.height)) / 2,
                            Mathf.Min(buttonRect.width, icon.width),
                            Mathf.Min(buttonRect.height, icon.height)
                        ),
                        icon
                    );
                }

                if (buttonRect.Contains(Event.current.mousePosition)) {
                    GUI.Label(new Rect(buttonRect.x, buttonRect.y - 20, 200, 20),
                        isDirectory ? "Click to open folder" : "Drag to Scene or double-click to edit",
                        EditorStyles.miniLabel
                    );
                }

                string displayName = itemName;
                if (displayName.Length > 15) displayName = displayName.Substring(0, 12) + "...";

                GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel) {
                    alignment = TextAnchor.UpperCenter, wordWrap = true, fontSize = 9
                };

                if (selectedItem == itemPath) {
                    labelStyle.normal.textColor = Color.blue;
                    labelStyle.fontStyle = FontStyle.Bold;
                }

                GUILayout.Label(displayName, labelStyle, GUILayout.Width(iconSize));

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void HandleEvents(Rect rect,
        string itemPath,
        bool isDirectory,
        string relativePath,
        string itemName,
        Texture2D icon) {
        Event evt = Event.current;

        if (rect.Contains(evt.mousePosition)) {
            hoveredItem = itemPath;

            if (!isDirectory) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
        }

        if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition)) {
            selectedItem = itemPath;

            if (isDirectory) {
                if (evt.clickCount == 1) {
                    currentPath = relativePath;
                    UpdateBreadcrumbs();
                    selectedItem = "";
                }
            } else {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(itemPath);
                if (prefab != null) {
                    if (evt.clickCount == 2) {
                        AssetDatabase.OpenAsset(prefab);
                    } else if (evt.clickCount == 1) {
                        Selection.activeObject = prefab;
                        EditorGUIUtility.PingObject(prefab);

                        dragItem = itemPath;
                        dragStartPos = evt.mousePosition;
                        isDragging = true;
                        evt.Use();
                    }
                }
            }
            
            RefreshPreviews();

            Repaint();
        }

        if (evt.type == EventType.MouseDrag && isDragging && dragItem == itemPath) {
            if (Vector2.Distance(evt.mousePosition, dragStartPos) > 10f) // Dead zone
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(itemPath);
                if (prefab != null) {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new Object[] {
                        prefab
                    };
                    DragAndDrop.StartDrag("Dragging: " + itemName);
                    evt.Use();
                }
            }
        }

        if (isDragging && dragItem == itemPath)
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.Link);
    }

    private void DrawDragPreview() {
        if (isDragging && !string.IsNullOrEmpty(dragItem) && Event.current.type != EventType.MouseUp) {
            Event evt = Event.current;

            Texture2D icon = GetIcon(dragItem, false);
            if (icon != null) {
                GUI.color = new Color(1, 1, 1, 0.7f);
                GUI.DrawTexture(new Rect(evt.mousePosition.x - 32, evt.mousePosition.y - 32, 64, 64), icon);
                GUI.color = Color.white;
            }

            GUI.Label(new Rect(evt.mousePosition.x + 20, evt.mousePosition.y - 10, 200, 20),
                "Drag to Scene view", EditorStyles.boldLabel
            );

            Repaint();
        }
    }

    private Texture2D GetIcon(string path, bool isDirectory) {
        if (!previewCache.ContainsKey(path) || previewCache[path] == null) {
            if (isDirectory)
                previewCache[path] = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
            else {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) {
                    Texture2D preview = AssetPreview.GetAssetPreview(prefab);
                    if (preview == null) preview = AssetPreview.GetMiniThumbnail(prefab);
                    previewCache[path] = preview;
                } else
                    previewCache[path] = EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
            }
        }

        return previewCache[path] ?? EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
    }

    private void UpdateBreadcrumbs() {
        breadcrumbs.Clear();
        if (!string.IsNullOrEmpty(currentPath)) breadcrumbs.AddRange(currentPath.Split('/'));
        
        RefreshPreviews();
    }

    private void GoBack() {
        if (string.IsNullOrEmpty(currentPath)) return;

        List<string> parts = currentPath.Split('/').ToList();
        if (parts.Count > 0) {
            parts.RemoveAt(parts.Count - 1);
            currentPath = string.Join("/", parts);
            UpdateBreadcrumbs();
            selectedItem = "";
        }
    }

    private void RefreshPreviews() {
        previewCache.Clear();
        AssetDatabase.Refresh();
        // Debug.Log("YG Prefab Manager: Preview cache cleared");
        Repaint();
    }

    private void OnInspectorUpdate() {
        Repaint();
    }
}