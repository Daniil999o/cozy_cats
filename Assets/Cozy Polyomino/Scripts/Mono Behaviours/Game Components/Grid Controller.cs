using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CozyPolyomino
{
    public class GridController : MonoBehaviour
    {
        [Header("Options")]
        [field: SerializeField] internal bool Debug { get; set; }

        [Space]
        [Header("GridSettings")]
        [SerializeField] internal int gridWidth = 4;

        [SerializeField] internal int gridHeight = 4;
        [SerializeField] internal float cellSize = 1f;
        [SerializeField] private float gridYOffset = 0.01f;

        [Space]
        [Header("Draggable Settings")]
        [SerializeField] private DraggableOptionsData draggableOptions;

        [Header("Camera")]
        [SerializeField] private Camera playerCamera;

        private HashSet<Vector2Int> _occupiedByShapes = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _occupiedByObstacles = new HashSet<Vector2Int>();

        private float _halfCellSize;

        private static GridController _instance;

        public static GridController Instance {
            get {
                if (!_instance) _instance = FindFirstObjectByType<GridController>();

                return _instance;
            }
        }

        internal static DraggableOptionsData DraggableOptions => Instance?.draggableOptions;
        internal static Camera MainCamera => Instance?.playerCamera;

        internal void Initialize() {
            _occupiedByShapes = new HashSet<Vector2Int>();
            _occupiedByObstacles = new HashSet<Vector2Int>();
            
            _halfCellSize = cellSize * 0.5f;
            
            Vector3 pos = new Vector3(-gridWidth * 0.5f, 0f, -gridHeight * 0.5f);
            
            transform.position = pos;
        }

        private Vector3 WorldCenter =>
            transform.position + new Vector3(
                gridWidth * cellSize * 0.5f,
                0,
                gridHeight * cellSize * 0.5f
            );

        internal bool CanPlaceShape(Vector2Int gridOrigin, Vector2Int[] shapeCells) =>
            shapeCells.Select(cellOffset => gridOrigin + cellOffset)
                .All(targetPos => !IsOutOfBounds(targetPos) && !IsCellOccupied(targetPos));

        internal bool IsCellOccupied(Vector2Int pos) =>
            IsShapeOccupied(pos) || IsObstacleCell(pos) || BonusSpawner.Instance.AnyOnCell(pos);

        internal bool IsShapeOccupied(Vector2Int pos) => _occupiedByShapes.Contains(pos);
        internal bool IsObstacleCell(Vector2Int pos) => _occupiedByObstacles.Contains(pos);

        public DraggableShape[] GetDraggablesOnCell(Vector2Int cellPos) =>
            LevelSpawner.Instance.GetDraggablesOnCell(cellPos);

        internal void PlaceShape(Vector2Int gridOrigin, Vector2Int[] shapeCells) {
            foreach (Vector2Int x in shapeCells) _occupiedByShapes.Add(gridOrigin + x);
            
            BonusSpawner.Instance.NotifyGridChanged(_occupiedByShapes.ToArray(), _occupiedByObstacles.ToArray());
        }

        internal void RemoveShape(Vector2Int gridOrigin, Vector2Int[] shapeCells) {
            foreach (Vector2Int x in shapeCells) _occupiedByShapes.Remove(gridOrigin + x);
            
            BonusSpawner.Instance.NotifyGridChanged(_occupiedByShapes.ToArray(), _occupiedByObstacles.ToArray());
        }

        internal void AddObstacle(Vector2Int gridOrigin, Vector2Int[] shapeCells) {
            foreach (Vector2Int x in shapeCells) _occupiedByObstacles.Add(gridOrigin + x);
        }

        internal void RemoveObstacle(Vector2Int gridOrigin, Vector2Int[] shapeCells) {
            foreach (Vector2Int x in shapeCells) _occupiedByObstacles.Remove(gridOrigin + x);
        }

        internal Vector2Int WorldToGrid(Vector3 worldPos) {
            Vector3 localPos = worldPos - transform.position;

            int x = Mathf.FloorToInt(localPos.x / cellSize + _halfCellSize);
            int z = Mathf.FloorToInt(localPos.z / cellSize + _halfCellSize);

            return new Vector2Int(x, z);
        }

        private Vector3 GridToWorld(Vector2Int gridPos) =>
            transform.position + new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);

        internal Vector3 GridToWorldCenter(Vector2Int gridPos) =>
            GridToWorld(gridPos) + new Vector3(_halfCellSize, 0, _halfCellSize);

        internal void ClearAll() {
            _occupiedByShapes.Clear();
            _occupiedByObstacles.Clear();
        }

        internal Vector3 GetOutOfGridPos(DraggableShape shape, Vector2Int[] cells) {
            Vector3 worldCenter = WorldCenter;

            Vector3 pos = shape.Center;
            
            while (Vector3.Distance(pos, WorldCenter) < Mathf.Max(gridWidth, gridHeight)
                   || cells.Select(cellOffset => WorldToGrid(pos) + cellOffset)
                   .Any(targetPos => !IsOutOfBounds(targetPos))) {
                Vector3 dir = worldCenter - pos;

                pos -= dir * 0.5f;
            }

            return pos;
        }

        private bool IsOutOfBounds(Vector2Int cell) =>
            cell.x < 0 || cell.x >= gridWidth || cell.y < 0 || cell.y >= gridHeight;

#if UNITY_EDITOR

        private void OnDrawGizmos() {
            if (!Debug) return;

            for (int x = 0; x < gridWidth; x++) {
                for (int y = 0; y < gridHeight; y++) {
                    Vector2Int pos = new Vector2Int(x, y);

                    if (IsObstacleCell(pos))
                        Gizmos.color = Color.yellow;
                    else if (IsShapeOccupied(pos))
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.gray;

                    Vector3 centerPos = GridToWorldCenter(pos);
                    centerPos.y += gridYOffset;

                    Gizmos.DrawWireCube(centerPos, new Vector3(cellSize, 0.1f, cellSize));
                }
            }

            Gizmos.color = Color.yellowNice;

            Gizmos.DrawWireCube(WorldCenter, new Vector3(gridWidth * cellSize, 0.2f, gridHeight * cellSize));
        }

#endif
    }
}