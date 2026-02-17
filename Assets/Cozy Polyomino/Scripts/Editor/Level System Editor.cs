#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace CozyPolyomino
{
    [CustomEditor(typeof(LevelSystemData))]
    public class LevelSystemEditor : Editor
    {
        private LevelSystemData _systemData;
        private LevelData _currentLevel;
        private int _selectedLevelIndex = -1;
        private string _newLevelName;
        private Vector2 _scrollPos;
        private Vector2 _shapesScrollPos;
        private Vector2 _levelsScrollPos;

        private ShapeData _selectedShape;
        private ObjectType _selectedObjectType = ObjectType.DraggableObject;
        private PlacedObject _selectedPlacedObject;
        private PlacedObject _hoveredPlacedObject;

        private Vector2Int _hoverGridCell = new Vector2Int(-1, -1);
        private Vector2Int _dragPreviewCell = new Vector2Int(-1, -1);
        private bool _isDraggingShape = false;
        private ShapeData _draggedShape;
        private bool _showPlacementPreview = false;
        private bool _isPlacementValid = false;

        private bool _isDraggingPlacedObject = false;
        private PlacedObject _draggedPlacedObject;
        private Vector2Int _draggedObjectOriginalPosition;

        private GUIStyle _gridCellStyle;
        private GUIStyle _gridCellHoverStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _shapeButtonStyle;
        private GUIStyle _shapeButtonSelectedStyle;
        private GUIStyle _levelButtonStyle;
        private GUIStyle _levelButtonSelectedStyle;
        private GUIStyle _levelGridButtonStyle;

        private List<ShapeData> _allShapes;
        private Texture2D _obstacleTexture;
        private Texture2D _draggableTexture;

        private Rect _gridRect;
        private float _gridStartX;
        private float _gridStartY;

        private const int CellSize = 30;

        private void OnEnable() {
            _systemData = (LevelSystemData)target;
            LoadLevelsFromFolder();
            LoadAllShapes();
            CreateTextures();
        }

        private void OnDisable() {
            if (_obstacleTexture) DestroyImmediate(_obstacleTexture);
            if (_draggableTexture) DestroyImmediate(_draggableTexture);
        }

        private void CreateTextures() {
            _obstacleTexture = CreateColorTexture(new Color(1f, 0.4f, 0.4f, 0.3f));
            _draggableTexture = CreateColorTexture(new Color(0.4f, 0.8f, 1f, 0.3f));
        }

        private Texture2D CreateColorTexture(Color color) {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void LoadAllShapes() {
            _allShapes = AssetDatabase.FindAssets("t:ShapeData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ShapeData>(AssetDatabase.GUIDToAssetPath(guid)))
                .OrderBy(s => s.name)
                .ToList();
        }

        private void LoadLevelsFromFolder() {
            string systemPath = AssetDatabase.GetAssetPath(_systemData);
            string folderPath = Path.GetDirectoryName(systemPath);
            string systemName = Path.GetFileNameWithoutExtension(systemPath);

            if (folderPath == null) return;

            string levelsFolder = Path.Combine(folderPath, systemName);

            if (!Directory.Exists(levelsFolder)) return;

            string[] levelPaths = Directory.GetFiles(levelsFolder, "*.asset");
            _systemData.ClearLevels();

            foreach (string path in levelPaths) {
                LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (level) _systemData.AddLevel(level);
            }
        }

        public override void OnInspectorGUI() {
            InitializeStyles();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Level System", _headerStyle);
            EditorGUILayout.Space();

            DrawCreateLevelSection();
            EditorGUILayout.Space();

            DrawLevelSelection();
            EditorGUILayout.Space();

            if (_currentLevel) {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
                DrawLevelEditor();
                EditorGUILayout.EndScrollView();
            }

            HandleKeyboardShortcuts();

            if ((!_isDraggingShape || !_draggedShape) && (!_isDraggingPlacedObject || _draggedPlacedObject == null))
                return;

            DrawDraggedShapeAtMouse();
            Repaint();
        }

        private void DrawDraggedShapeAtMouse() {
            ShapeData shapeToDraw = null;

            if (_isDraggingShape && _draggedShape)
                shapeToDraw = _draggedShape;
            else if (_isDraggingPlacedObject && _draggedPlacedObject != null)
                shapeToDraw = _draggedPlacedObject.shapeData;

            if (!shapeToDraw) return;

            const float draggedCellSize = 20f;

            float width = shapeToDraw.width * draggedCellSize;
            float height = shapeToDraw.height * draggedCellSize;

            Rect previewRect = new Rect(
                Event.current.mousePosition.x - width / 2,
                Event.current.mousePosition.y - height / 2,
                width,
                height
            );

            Color shapeColor = Color.green;

            if (_isDraggingPlacedObject) {
                shapeColor = _draggedPlacedObject.objectType == ObjectType.Obstacle
                    ? new Color(1f, 0.4f, 0.4f, 0.5f)
                    : new Color(0.4f, 0.8f, 1f, 0.5f);
            }

            DrawShapeInRectWithFixedCellSize(shapeToDraw, previewRect, draggedCellSize, shapeColor);
        }

        private void DrawShapeInRectWithFixedCellSize(ShapeData shape, Rect rect, float cellSize, Color shapeColor) {
            if (!shape || shape.shapeMap == null) return;

            Vector2 startPos = new Vector2(
                rect.x,
                rect.y
            );

            for (int y = 0; y < shape.height; y++) {
                for (int x = 0; x < shape.width; x++) {
                    if (shape.shapeMap[y * shape.width + x]) {
                        Rect cellRect = new Rect(
                            startPos.x + x * cellSize,
                            startPos.y + y * cellSize,
                            cellSize - 1,
                            cellSize - 1
                        );

                        EditorGUI.DrawRect(cellRect, shapeColor);

                        Color borderColor = Color.black;
                        borderColor.a = 0.5f;

                        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), borderColor);
                        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), borderColor);
                        EditorGUI.DrawRect(new Rect(cellRect.x + cellRect.width - 1, cellRect.y, 1, cellRect.height),
                            borderColor
                        );
                        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y + cellRect.height - 1, cellRect.width, 1),
                            borderColor
                        );
                    }
                }
            }
        }

        private void HandleKeyboardShortcuts() {
            if (Event.current.type != EventType.KeyDown) return;

            if (_selectedPlacedObject != null && Event.current.keyCode == KeyCode.Delete) {
                DeleteSelectedObject();
                Event.current.Use();
            }

            if ((!_isDraggingShape && !_isDraggingPlacedObject) || Event.current.keyCode != KeyCode.Escape) return;

            CancelDragging();
            Event.current.Use();
        }

        private void CancelDragging() {
            if (_isDraggingPlacedObject && _draggedPlacedObject != null) {
                _currentLevel.AddObject(_draggedPlacedObject);
                EditorUtility.SetDirty(_currentLevel);
            }

            _isDraggingShape = false;
            _draggedShape = null;
            _isDraggingPlacedObject = false;
            _draggedPlacedObject = null;
            _showPlacementPreview = false;
        }

        private Vector2Int ScreenToGridPosition(Vector2 screenPos) {
            if (!_currentLevel || !_gridRect.Contains(screenPos)) return new Vector2Int(-1, -1);

            Vector2 localPos = screenPos - new Vector2(_gridStartX, _gridStartY);

            int x = Mathf.FloorToInt(localPos.x / CellSize);
            int y = Mathf.FloorToInt(localPos.y / CellSize);

            y = _currentLevel.gridHeight - 1 - y;

            return new Vector2Int(x, y);
        }

        private void InitializeStyles() {
            if (_gridCellStyle != null) return;

            _gridCellStyle = new GUIStyle(GUI.skin.box) {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(1, 1, 1, 1),
                normal = {
                    background = MakeTexture(2, 2, new Color(0.95f, 0.95f, 0.95f, 0.2f))
                }
            };

            _gridCellHoverStyle = new GUIStyle(_gridCellStyle) {
                normal = {
                    background = MakeTexture(2, 2, new Color(0.7f, 0.85f, 1f, 0.3f))
                }
            };

            _headerStyle = new GUIStyle(EditorStyles.largeLabel) {
                fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
            };

            _shapeButtonStyle = new GUIStyle(GUI.skin.button) {
                margin = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(5, 5, 5, 5),
                fixedWidth = 60,
                fixedHeight = 60
            };

            _shapeButtonSelectedStyle = new GUIStyle(_shapeButtonStyle) {
                normal = {
                    background = MakeTexture(2, 2, new Color(0.3f, 0.6f, 1f, 0.3f))
                }
            };

            _levelButtonStyle = new GUIStyle(GUI.skin.button) {
                margin = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(8, 8, 6, 6),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11
            };

            _levelButtonSelectedStyle = new GUIStyle(_levelButtonStyle) {
                normal = {
                    background = MakeTexture(2, 2, new Color(0.3f, 0.6f, 1f, 0.5f))
                },
                fontStyle = FontStyle.Bold
            };

            _levelGridButtonStyle = new GUIStyle(GUI.skin.button) {
                margin = new RectOffset(1, 1, 1, 1),
                padding = new RectOffset(3, 3, 3, 3),
                fontSize = 9,
                fixedHeight = 40
            };
        }

        private Texture2D MakeTexture(int width, int height, Color color) {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void DrawCreateLevelSection() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Create New Level", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _newLevelName ??= $"Level {_systemData.Levels.Count + 1}";

            _newLevelName = EditorGUILayout.TextField("Level Name", _newLevelName);

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Create New Level", GUILayout.Height(25))) CreateNewLevel();
            EditorGUILayout.EndVertical();
        }

        private void CreateNewLevel() {
            string systemPath = AssetDatabase.GetAssetPath(_systemData);
            string folderPath = Path.GetDirectoryName(systemPath);
            string systemName = Path.GetFileNameWithoutExtension(systemPath);

            if (folderPath != null) {
                string levelsFolder = Path.Combine(folderPath, systemName);
                if (!Directory.Exists(levelsFolder)) Directory.CreateDirectory(levelsFolder);

                LevelData newLevel = CreateInstance<LevelData>();
                newLevel.levelName = _newLevelName;

                string levelPath = Path.Combine(levelsFolder, $"{_newLevelName}.asset");
                levelPath = AssetDatabase.GenerateUniqueAssetPath(levelPath);

                AssetDatabase.CreateAsset(newLevel, levelPath);
                AssetDatabase.SaveAssets();

                _systemData.AddLevel(newLevel);
                _currentLevel = newLevel;
                _selectedLevelIndex = _systemData.Levels.Count - 1;

                EditorUtility.SetDirty(_systemData);
                EditorUtility.SetDirty(newLevel);
            }

            _newLevelName = null;
        }

        private void DrawLevelSelection() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Select Level to Edit", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (_systemData.Levels.Count == 0)
                EditorGUILayout.HelpBox("No levels created yet. Create a new level above.", MessageType.Info);
            else {
                _levelsScrollPos = EditorGUILayout.BeginScrollView(_levelsScrollPos, GUILayout.Height(150));

                const int levelsPerRow = 3;
                int levelIndex = 0;

                while (levelIndex < _systemData.Levels.Count) {
                    EditorGUILayout.BeginHorizontal();

                    for (int i = 0; i < levelsPerRow && levelIndex < _systemData.Levels.Count; i++) {
                        LevelData level = _systemData.Levels[levelIndex];
                        bool isSelected = level == _currentLevel;

                        GUIStyle style = isSelected ? _levelButtonSelectedStyle : _levelButtonStyle;

                        if (GUILayout.Button(level.levelName, style, GUILayout.ExpandWidth(true))) {
                            _currentLevel = level;
                            _selectedLevelIndex = levelIndex;
                            _selectedPlacedObject = null;
                            _hoveredPlacedObject = null;
                        }

                        levelIndex++;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawLevelEditor() {
            if (!_currentLevel) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Editing: {_currentLevel.levelName}", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear Level", GUILayout.Width(80))) {
                if (EditorUtility.DisplayDialog("Clear Level",
                        "Remove all objects from this level?", "Clear", "Cancel"
                    )) {
                    _currentLevel.ClearLevel();
                    _selectedPlacedObject = null;
                    _hoveredPlacedObject = null;
                    CancelDragging();
                    EditorUtility.SetDirty(_currentLevel);
                }
            }

            if (GUILayout.Button("Delete Level", GUILayout.Width(80))) {
                if (EditorUtility.DisplayDialog("Delete Level",
                        $"Permanently delete {_currentLevel.levelName}?", "Delete", "Cancel"
                    )) {
                    DeleteCurrentLevel();
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField("Grid Size", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Width:", GUILayout.Width(50));

            int newWidth = EditorGUILayout.IntField(_currentLevel.gridWidth, GUILayout.Width(40));

            EditorGUILayout.LabelField("Height:", GUILayout.Width(50));

            int newHeight = EditorGUILayout.IntField(_currentLevel.gridHeight, GUILayout.Width(40));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (newWidth != _currentLevel.gridWidth || newHeight != _currentLevel.gridHeight) {
                if (newWidth > 0 && newHeight > 0) {
                    _currentLevel.gridWidth = newWidth;
                    _currentLevel.gridHeight = newHeight;
                    EditorUtility.SetDirty(_currentLevel);
                }
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField("Placing:", EditorStyles.miniBoldLabel);
            _selectedObjectType = (ObjectType)EditorGUILayout.EnumPopup(_selectedObjectType);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            DrawShapePalette();

            EditorGUILayout.Space(10);

            DrawGrid();

            EditorGUILayout.Space(10);

            DrawPlacedObjectsList();

            EditorGUILayout.EndVertical();
        }

        private void DrawShapePalette() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Shape Palette (Drag shapes onto grid)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (_allShapes.Count == 0)
                EditorGUILayout.HelpBox("No shapes found. Create Shape Data assets first.", MessageType.Info);
            else {
                _shapesScrollPos = EditorGUILayout.BeginScrollView(_shapesScrollPos, GUILayout.Height(100));

                int shapesPerRow = Mathf.Max(1, Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / 70));
                int shapeIndex = 0;

                while (shapeIndex < _allShapes.Count) {
                    EditorGUILayout.BeginHorizontal();

                    for (int i = 0; i < shapesPerRow && shapeIndex < _allShapes.Count; i++) {
                        ShapeData shape = _allShapes[shapeIndex];
                        bool isSelected = _selectedShape == shape;

                        GUIStyle style = isSelected ? _shapeButtonSelectedStyle : _shapeButtonStyle;

                        Rect buttonRect = GUILayoutUtility.GetRect(60, 60, style);

                        if (Event.current.type == EventType.Repaint) {
                            style.Draw(buttonRect, false, false, isSelected, false);
                            DrawShapeInRect(shape, buttonRect);
                        }

                        if (buttonRect.Contains(Event.current.mousePosition)) {
                            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                                _selectedShape = shape;
                                _isDraggingShape = true;
                                _draggedShape = shape;
                                Event.current.Use();
                            }
                        }

                        shapeIndex++;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawShapeInRect(ShapeData shape, Rect rect) {
            if (!shape || shape.shapeMap == null) return;

            float cellSize = Mathf.Min(rect.width / shape.width, rect.height / shape.height) * 0.8f;
            Vector2 startPos = new Vector2(
                rect.x + (rect.width - shape.width * cellSize) / 2,
                rect.y + (rect.height - shape.height * cellSize) / 2
            );

            for (int y = 0; y < shape.height; y++) {
                for (int x = 0; x < shape.width; x++) {
                    if (!shape.shapeMap[y * shape.width + x]) continue;

                    Rect cellRect = new Rect(
                        startPos.x + x * cellSize,
                        startPos.y + y * cellSize,
                        cellSize - 1,
                        cellSize - 1
                    );

                    EditorGUI.DrawRect(cellRect, Color.green);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), Color.black);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), Color.black);
                    EditorGUI.DrawRect(new Rect(cellRect.x + cellRect.width - 1, cellRect.y, 1, cellRect.height),
                        Color.black
                    );
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y + cellRect.height - 1, cellRect.width, 1),
                        Color.black
                    );
                }
            }
        }

        private void DrawGrid() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            float totalGridWidth = _currentLevel.gridWidth * CellSize;
            float totalGridHeight = _currentLevel.gridHeight * CellSize;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();

            GUILayout.Box("", GUILayout.Width(totalGridWidth), GUILayout.Height(totalGridHeight));
            _gridRect = GUILayoutUtility.GetLastRect();
            _gridStartX = _gridRect.x;
            _gridStartY = _gridRect.y;

            for (int y = 0; y < _currentLevel.gridHeight; y++) {
                for (int x = 0; x < _currentLevel.gridWidth; x++) {
                    float cellX = _gridStartX + x * CellSize;
                    float cellY = _gridStartY + y * CellSize;

                    Rect cellRect = new Rect(cellX, cellY, CellSize, CellSize);

                    EditorGUI.DrawRect(cellRect, new Color(0.95f, 0.95f, 0.95f, 0.2f));

                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), Color.gray);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), Color.gray);

                    if (!cellRect.Contains(Event.current.mousePosition)) continue;

                    _hoverGridCell = new Vector2Int(x, _currentLevel.gridHeight - 1 - y);

                    EditorGUI.DrawRect(cellRect, new Color(0.7f, 0.85f, 1f, 0.3f));

                    if (Event.current.type != EventType.MouseUp) continue;

                    if (_isDraggingShape && _draggedShape) {
                        if (IsPositionValidForShape(_draggedShape, _hoverGridCell)) {
                            PlaceShapeOnGrid(_draggedShape, _hoverGridCell);
                            _isDraggingShape = false;
                            _draggedShape = null;
                            _showPlacementPreview = false;
                            Event.current.Use();
                        } else {
                            _isDraggingShape = false;
                            _draggedShape = null;
                            _showPlacementPreview = false;
                        }
                    } else if (_isDraggingPlacedObject && _draggedPlacedObject != null) {
                        if (IsPositionValidForPlacedObject(_draggedPlacedObject, _hoverGridCell)) {
                            _draggedPlacedObject.gridPosition = _hoverGridCell;
                            _currentLevel.AddObject(_draggedPlacedObject);
                            EditorUtility.SetDirty(_currentLevel);

                            _isDraggingPlacedObject = false;
                            _draggedPlacedObject = null;
                            _showPlacementPreview = false;
                            Event.current.Use();
                        } else {
                            _currentLevel.AddObject(_draggedPlacedObject);
                            EditorUtility.SetDirty(_currentLevel);

                            _isDraggingPlacedObject = false;
                            _draggedPlacedObject = null;
                            _showPlacementPreview = false;
                        }
                    }
                }
            }

            DrawObjectsOnGrid();

            if (_gridRect.Contains(Event.current.mousePosition)) {
                if (_isDraggingShape && _draggedShape != null) {
                    Vector2Int gridPos = ScreenToGridPosition(Event.current.mousePosition);

                    if (gridPos is { x: >= 0, y: >= 0 } && gridPos.x < _currentLevel.gridWidth &&
                        gridPos.y < _currentLevel.gridHeight) {
                        _dragPreviewCell = gridPos;
                        _isPlacementValid = IsPositionValidForShape(_draggedShape, gridPos);
                        _showPlacementPreview = true;

                        DrawShapePreview(_draggedShape, gridPos, _isPlacementValid ? Color.green : Color.red);
                    } else
                        _showPlacementPreview = false;
                } else if (_isDraggingPlacedObject && _draggedPlacedObject != null) {
                    Vector2Int gridPos = ScreenToGridPosition(Event.current.mousePosition);

                    if (gridPos is { x: >= 0, y: >= 0 } &&
                        gridPos.x < _currentLevel.gridWidth && gridPos.y < _currentLevel.gridHeight) {
                        _dragPreviewCell = gridPos;
                        _isPlacementValid = IsPositionValidForPlacedObject(_draggedPlacedObject, gridPos);
                        _showPlacementPreview = true;

                        Color previewColor = _isPlacementValid ? Color.green : Color.red;
                        DrawShapePreview(_draggedPlacedObject.shapeData, gridPos, previewColor);
                    } else
                        _showPlacementPreview = false;
                }
            }

            if (Event.current.type == EventType.MouseUp && (_isDraggingShape || _isDraggingPlacedObject)) {
                if (_isDraggingPlacedObject && _draggedPlacedObject != null) {
                    _currentLevel.AddObject(_draggedPlacedObject);
                    EditorUtility.SetDirty(_currentLevel);
                }

                _isDraggingShape = false;
                _draggedShape = null;
                _isDraggingPlacedObject = false;
                _draggedPlacedObject = null;
                _showPlacementPreview = false;
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawObjectsOnGrid() {
            if (_currentLevel.PlacedObjects.Count == 0) return;

            Dictionary<PlacedObject, HashSet<Vector2Int>> shapeCells =
                new Dictionary<PlacedObject, HashSet<Vector2Int>>();

            foreach (PlacedObject obj in _currentLevel.PlacedObjects) {
                if (_isDraggingPlacedObject && obj == _draggedPlacedObject) continue;

                HashSet<Vector2Int> cells = new HashSet<Vector2Int>();
                Vector2Int[] shapeCellsArray = obj.shapeData.GetCells();

                foreach (Vector2Int cell in shapeCellsArray) {
                    Vector2Int transformedCell = ApplyTransform(cell, obj.rotation, obj.isFlipped);
                    Vector2Int gridCell = obj.gridPosition + transformedCell;
                    cells.Add(gridCell);
                }

                shapeCells[obj] = cells;
            }

            for (int i = 0; i < _currentLevel.PlacedObjects.Count; i++) {
                PlacedObject obj = _currentLevel.PlacedObjects[i];
                
                if (_isDraggingPlacedObject && obj == _draggedPlacedObject) continue;

                bool isSelected = _selectedPlacedObject == obj;
                bool isHovered = _hoveredPlacedObject == obj;

                Color fillColor = obj.objectType == ObjectType.Obstacle
                    ? new Color(1f, 0.4f, 0.4f, 0.3f)
                    : new Color(0.4f, 0.8f, 1f, 0.3f);

                Color borderColor = isSelected ? Color.yellow :
                    obj.objectType == ObjectType.Obstacle ? Color.red : Color.blue;

                HashSet<Vector2Int> cells = shapeCells[obj];

                foreach (Vector2Int gridCell in cells) {
                    float x = _gridStartX + gridCell.x * CellSize;
                    float y = _gridStartY + (_currentLevel.gridHeight - 1 - gridCell.y) * CellSize;

                    Rect cellRect = new Rect(x, y, CellSize, CellSize);

                    EditorGUI.DrawRect(cellRect, fillColor);

                    DrawInternalBorders(cellRect, gridCell, cells, borderColor, 1f);

                    if (isSelected) {
                        const float borderWidth = 2f;

                        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, borderWidth), borderColor);
                        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, borderWidth, cellRect.height), borderColor);
                        EditorGUI.DrawRect(
                            new Rect(cellRect.x + cellRect.width - borderWidth, cellRect.y, borderWidth, cellRect.height
                            ),
                            borderColor
                        );
                        EditorGUI.DrawRect(
                            new Rect(cellRect.x, cellRect.y + cellRect.height - borderWidth, cellRect.width, borderWidth
                            ),
                            borderColor
                        );
                    }

                    if (!cellRect.Contains(Event.current.mousePosition) ||
                        _isDraggingShape || _isDraggingPlacedObject)
                        continue;

                    _hoveredPlacedObject = obj;

                    if (Event.current.type != EventType.MouseDown ||
                        (Event.current.button != 0 && Event.current.button != 1))
                        continue;

                    if ((Event.current.button == 0 || Event.current.button == 1) && Event.current.alt) {
                        _isDraggingPlacedObject = true;
                        _draggedPlacedObject = obj;
                        _draggedObjectOriginalPosition = obj.gridPosition;

                        _currentLevel.RemoveObject(obj);
                        _selectedPlacedObject = null;
                        _hoveredPlacedObject = null;

                        Event.current.Use();
                    } else {
                        _selectedPlacedObject = obj;
                        _selectedShape = obj.shapeData;
                        _selectedObjectType = obj.objectType;

                        if (Event.current.button == 1) {
                            ShowObjectContextMenu(obj);
                            Event.current.Use();
                        }
                    }

                    Repaint();
                }
            }
        }

        private static void DrawInternalBorders(Rect cellRect,
            Vector2Int cellPos,
            HashSet<Vector2Int> shapeCells,
            Color borderColor,
            float borderWidth) {
            bool hasLeftNeighbor = shapeCells.Contains(new Vector2Int(cellPos.x - 1, cellPos.y));
            bool hasRightNeighbor = shapeCells.Contains(new Vector2Int(cellPos.x + 1, cellPos.y));
            bool hasTopNeighbor = shapeCells.Contains(new Vector2Int(cellPos.x, cellPos.y + 1));
            bool hasBottomNeighbor = shapeCells.Contains(new Vector2Int(cellPos.x, cellPos.y - 1));

            if (!hasLeftNeighbor)
                EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, borderWidth, cellRect.height), borderColor);
            if (!hasRightNeighbor) {
                EditorGUI.DrawRect(
                    new Rect(cellRect.x + cellRect.width - borderWidth, cellRect.y, borderWidth, cellRect.height),
                    borderColor
                );
            }

            if (!hasTopNeighbor)
                EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, borderWidth), borderColor);
            if (!hasBottomNeighbor) {
                EditorGUI.DrawRect(
                    new Rect(cellRect.x, cellRect.y + cellRect.height - borderWidth, cellRect.width, borderWidth),
                    borderColor
                );
            }
        }

        private void DrawShapePreview(ShapeData shape, Vector2Int gridPos, Color previewColor) {
            if (!shape) return;

            Vector2Int[] cells = shape.GetCells();

            const float alpha = 0.3f;

            HashSet<Vector2Int> shapeCells = new HashSet<Vector2Int>(cells);

            foreach (Vector2Int cell in cells) {
                Vector2Int worldCell = gridPos + cell;

                if (worldCell.x < 0 || worldCell.x >= _currentLevel.gridWidth ||
                    worldCell.y < 0 || worldCell.y >= _currentLevel.gridHeight)
                    continue;

                float x = _gridStartX + worldCell.x * CellSize;
                float y = _gridStartY + (_currentLevel.gridHeight - 1 - worldCell.y) * CellSize;

                Rect cellRect = new Rect(x, y, CellSize, CellSize);

                Color fillColor = previewColor;
                fillColor.a = alpha;
                EditorGUI.DrawRect(cellRect, fillColor);

                DrawInternalBorders(cellRect, worldCell, shapeCells, previewColor, 2f);
            }
        }

        private static Vector2Int ApplyTransform(Vector2Int cell, int rotation, bool isFlipped) {
            Vector2Int result = cell;

            if (isFlipped) result.x = -result.x;

            for (int i = 0; i < rotation / 90; i++) result = new Vector2Int(-result.y, result.x);

            return result;
        }

        private void ShowObjectContextMenu(PlacedObject obj) {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Delete Object"), false, () => {
                    _currentLevel.RemoveObject(obj);
                    if (_selectedPlacedObject == obj) _selectedPlacedObject = null;
                    if (_hoveredPlacedObject == obj) _hoveredPlacedObject = null;
                    EditorUtility.SetDirty(_currentLevel);
                    Repaint();
                }
            );

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Move Object (Alt+Click)"), false, () => {
                    EditorUtility.DisplayDialog("Move Object",
                        "To move an object:\n1. Hold Alt key\n2. Click and drag the object\n3. Release to place",
                        "OK"
                    );
                }
            );

            menu.ShowAsContext();
        }

        private void DrawPlacedObjectsList() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Placed Objects", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"{_currentLevel.PlacedObjects.Count} objects", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (_currentLevel.PlacedObjects.Count == 0)
                EditorGUILayout.HelpBox("Drag shapes from palette to place them", MessageType.Info);
            else {
                foreach (PlacedObject obj in _currentLevel.PlacedObjects) {
                    bool isSelected = _selectedPlacedObject == obj;

                    EditorGUILayout.BeginHorizontal();

                    Color oldColor = GUI.color;
                    GUI.color = obj.objectType == ObjectType.Obstacle ? Color.red : Color.blue;
                    GUILayout.Box("", GUILayout.Width(10), GUILayout.Height(20));
                    GUI.color = oldColor;

                    string label = $"{obj.shapeData.name} at ({obj.gridPosition.x}, {obj.gridPosition.y})";
                    if (obj.rotation != 0 || obj.isFlipped) label += $" [R{obj.rotation}{(obj.isFlipped ? "F" : "")}]";

                    float buttonWidth = EditorGUIUtility.currentViewWidth - 120;
                    if (GUILayout.Button(label, isSelected ? EditorStyles.boldLabel : EditorStyles.label,
                            GUILayout.Width(buttonWidth), GUILayout.Height(20)
                        )) {
                        _selectedPlacedObject = obj;
                        _selectedShape = obj.shapeData;
                        _selectedObjectType = obj.objectType;
                        Repaint();
                    }

                    if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(20))) {
                        _currentLevel.RemoveObject(obj);
                        if (_selectedPlacedObject == obj) _selectedPlacedObject = null;
                        if (_hoveredPlacedObject == obj) _hoveredPlacedObject = null;
                        EditorUtility.SetDirty(_currentLevel);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private bool IsPositionValidForShape(ShapeData shape, Vector2Int gridPos) {
            if (!_currentLevel) return false;

            Vector2Int[] cells = shape.GetCells();
            foreach (Vector2Int cell in cells) {
                Vector2Int worldCell = gridPos + cell;
                if (worldCell.x < 0 || worldCell.x >= _currentLevel.gridWidth ||
                    worldCell.y < 0 || worldCell.y >= _currentLevel.gridHeight)
                    return false;

                if (_currentLevel.IsCellOccupied(worldCell)) return false;
            }

            return true;
        }

        private bool IsPositionValidForPlacedObject(PlacedObject placedObject, Vector2Int newGridPos) {
            if (!_currentLevel) return false;

            Vector2Int[] cells = placedObject.shapeData.GetCells();
            foreach (Vector2Int cell in cells) {
                Vector2Int transformedCell = ApplyTransform(cell, placedObject.rotation, placedObject.isFlipped);
                Vector2Int worldCell = newGridPos + transformedCell;

                if (worldCell.x < 0 || worldCell.x >= _currentLevel.gridWidth ||
                    worldCell.y < 0 || worldCell.y >= _currentLevel.gridHeight)
                    return false;

                Vector2Int originalTransformedCell =
                    ApplyTransform(cell, placedObject.rotation, placedObject.isFlipped);
                Vector2Int originalWorldCell = _draggedObjectOriginalPosition + originalTransformedCell;

                if (worldCell == originalWorldCell) continue;

                if (_currentLevel.IsCellOccupied(worldCell)) return false;
            }

            return true;
        }

        private void PlaceShapeOnGrid(ShapeData shape, Vector2Int gridPos) {
            PlacedObject newObject = new PlacedObject {
                shapeData = shape,
                objectType = _selectedObjectType,
                gridPosition = gridPos,
                rotation = 0,
                isFlipped = false
            };

            _currentLevel.AddObject(newObject);
            EditorUtility.SetDirty(_currentLevel);
            Repaint();
        }

        private void DeleteSelectedObject() {
            if (_selectedPlacedObject == null) return;

            _currentLevel.RemoveObject(_selectedPlacedObject);
            _selectedPlacedObject = null;
            EditorUtility.SetDirty(_currentLevel);
            Repaint();
        }

        private void DeleteCurrentLevel() {
            string path = AssetDatabase.GetAssetPath(_currentLevel);
            AssetDatabase.DeleteAsset(path);

            _systemData.RemoveLevel(_currentLevel);
            _selectedLevelIndex = -1;
            _currentLevel = null;
            _selectedPlacedObject = null;
            _hoveredPlacedObject = null;
            CancelDragging();

            EditorUtility.SetDirty(_systemData);
            AssetDatabase.SaveAssets();
            LoadLevelsFromFolder();
        }
    }
}

#endif