using UnityEngine;

namespace CozyPolyomino
{
    public class GridDrawer : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private SpriteRenderer cellPrefab;

        [SerializeField] private Transform gridParent;

        [Header("Cell Settings")]
        [SerializeField] private DrawerColorsData colorsData;

        private SpriteRenderer[,] _cellRenderers;
        private int _width;
        private int _height;
        
        private static GridDrawer _instance;
        public static GridDrawer Instance {
            get {
                if (!_instance) _instance = FindFirstObjectByType<GridDrawer>(FindObjectsInactive.Include);
                
                return _instance;
            }
        }

        internal void Initialize() {
            GenerateGridVisuals();
            ResetGridColors();
        }

        private void GenerateGridVisuals() {
            if (!GridController.Instance) return;

            if (_cellRenderers != null) {
                foreach (SpriteRenderer r in _cellRenderers) {
                    if (r) Destroy(r.gameObject);
                }
            }

            _width = GridController.Instance.gridWidth;
            _height = GridController.Instance.gridHeight;
            float cellSize = GridController.Instance.cellSize;

            _cellRenderers = new SpriteRenderer[_width, _height];

            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height; y++) {
                    SpriteRenderer cell = Instantiate(cellPrefab, gridParent ? gridParent : transform);
                    
                    cell.gameObject.name = $"Cell_{x}_{y}";

                    Vector3 worldPos = GridController.Instance.GridToWorldCenter(new Vector2Int(x, y));
                    worldPos.y = 0.02f;

                    cell.transform.position = worldPos;
                    cell.transform.localScale = new Vector3(cellSize, cellSize, 1f) * colorsData.visualCellScale;
                    cell.transform.rotation = Quaternion.Euler(90, 0, 0);
                    
                    _cellRenderers[x, y] = cell;
                }
            }
        }

        internal void ResetGridColors() {
            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height; y++) {
                    Vector2Int pos = new Vector2Int(x, y);
                    bool isOccupied = GridController.Instance.IsCellOccupied(pos);
                    bool isObstacle = GridController.Instance.IsObstacleCell(pos);

                    _cellRenderers[x, y].color = isObstacle ? colorsData.colorBlocked :
                        isOccupied ? colorsData.colorOccupied : colorsData.colorEmpty;
                }
            }
        }
        
        internal bool HighlightPreview(Vector2Int pivotGridPos, Vector2Int[] shapeCells, bool isValid) {
            bool any = false;
            
            ResetGridColors();

            Color targetColor = isValid ? colorsData.colorValidHover : colorsData.colorInvalidHover;

            foreach (Vector2Int cellOffset in shapeCells) {
                Vector2Int targetPos = pivotGridPos + cellOffset;
            
                bool isOccupied = GridController.Instance.IsCellOccupied(targetPos);
                bool isObstacle = GridController.Instance.IsObstacleCell(targetPos);

                if (isOccupied && isObstacle) {
                    any = true;
                    continue;
                }

                if (IsInsideGrid(targetPos)) {
                    _cellRenderers[targetPos.x, targetPos.y].color = targetColor;
                    any = true;
                }
            }

            return any;
        }

        private bool IsInsideGrid(Vector2Int pos) => pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
    }
}