using UnityEngine;
using System.Collections.Generic;

namespace CozyPolyomino
{
    [CreateAssetMenu(fileName = "New Shape", menuName = "Cozy Polyomino/Shape Data")]
    public class ShapeData : ScriptableObject
    {
        public int width = 3;
        public int height = 3;

        [HideInInspector] public bool[] shapeMap;

        private Vector2Int[] _cachedCells;
        private bool _cacheValid;

        public Vector2Int[] GetCells() {
            if (!_cacheValid || _cachedCells == null || shapeMap == null || shapeMap.Length != width * height)
                UpdateCache();
            return _cachedCells;
        }

        private void UpdateCache() {
            List<Vector2Int> cells = new List<Vector2Int>();

            if (shapeMap == null || shapeMap.Length != width * height) {
                _cachedCells = cells.ToArray();
                _cacheValid = true;
                return;
            }

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++)
                    if (shapeMap[y * width + x])
                        cells.Add(new Vector2Int(x, height - 1 - y));
            }

            _cachedCells = cells.ToArray();
            _cacheValid = true;
        }

        public void InvalidateCache() {
            _cacheValid = false;
        }

#if UNITY_EDITOR

        public Texture2D GetPreviewTexture() {
            Vector2Int[] cells = GetCells();
            
            if (cells == null || cells.Length == 0) return null;

            const int textureSize = 64;
            Texture2D texture = new Texture2D(textureSize, textureSize);

            Color[] clearPixels = new Color[textureSize * textureSize];
            for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = Color.clear;
            texture.SetPixels(clearPixels);

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (Vector2Int cell in cells) {
                minX = Mathf.Min(minX, cell.x);
                maxX = Mathf.Max(maxX, cell.x);
                minY = Mathf.Min(minY, cell.y);
                maxY = Mathf.Max(maxY, cell.y);
            }

            int gridWidth = maxX - minX + 1;
            int gridHeight = maxY - minY + 1;

            if (gridWidth == 0 || gridHeight == 0) return texture;

            const float cellPadding = 2f;
            const float minCellSize = 6f;
            const float maxCellSize = 10f;

            float availableWidth = textureSize - (gridWidth + 1) * cellPadding;
            float availableHeight = textureSize - (gridHeight + 1) * cellPadding;

            float cellSizeByWidth = availableWidth / gridWidth;
            float cellSizeByHeight = availableHeight / gridHeight;

            float cellSize = Mathf.Min(cellSizeByWidth, cellSizeByHeight);
            
            cellSize = Mathf.Clamp(cellSize, minCellSize, maxCellSize);

            float totalGridWidth = gridWidth * cellSize + (gridWidth - 1) * cellPadding;
            float totalGridHeight = gridHeight * cellSize + (gridHeight - 1) * cellPadding;

            float startX = (textureSize - totalGridWidth) / 2f;
            float startY = (textureSize - totalGridHeight) / 2f;

            HashSet<Vector2Int> cellSet = new HashSet<Vector2Int>(cells);

            for (int gridY = 0; gridY < gridHeight; gridY++) {
                for (int gridX = 0; gridX < gridWidth; gridX++) {
                    int worldX = minX + gridX;
                    int worldY = minY + gridY;

                    Vector2Int cellPos = new Vector2Int(worldX, worldY);
                    
                    if (!cellSet.Contains(cellPos)) continue;

                    float cellX = startX + gridX * (cellSize + cellPadding);
                    float cellY = startY + gridY * (cellSize + cellPadding);

                    int pixelStartX = Mathf.FloorToInt(cellX);
                    int pixelStartY = Mathf.FloorToInt(cellY);
                    int pixelEndX = Mathf.FloorToInt(cellX + cellSize);
                    int pixelEndY = Mathf.FloorToInt(cellY + cellSize);

                    pixelStartX = Mathf.Clamp(pixelStartX, 0, textureSize - 1);
                    pixelStartY = Mathf.Clamp(pixelStartY, 0, textureSize - 1);
                    pixelEndX = Mathf.Clamp(pixelEndX, 0, textureSize);
                    pixelEndY = Mathf.Clamp(pixelEndY, 0, textureSize);

                    for (int px = pixelStartX; px < pixelEndX; px++) {
                        for (int py = pixelStartY; py < pixelEndY; py++)
                            texture.SetPixel(px, py, Color.green);
                    }
                }
            }

            texture.Apply();
            
            return texture;
        }

#endif
    }
}