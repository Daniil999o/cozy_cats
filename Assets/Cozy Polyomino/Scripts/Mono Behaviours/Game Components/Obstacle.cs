using UnityEngine;

namespace CozyPolyomino
{
    public class Obstacle : MonoBehaviour
    {
        [Header("Shape Configuration")]
        [SerializeField] internal ShapeData shapeData;

        private Vector2Int[] _shapeCells;
        private GridController _gridController;
        private Vector2Int _gridPosition;

        internal void InitializeObstacle(Vector2Int pos, Vector2Int[] cells) {
            _gridController = GridController.Instance;

            _shapeCells = cells;
            _gridPosition = pos;

            _gridController.AddObstacle(_gridPosition, _shapeCells);
        }

        private void OnDestroy() {
            _gridController.RemoveObstacle(_gridPosition, _shapeCells);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!_gridController || !_gridController.Debug) return;
            
            Vector2Int[] cells = shapeData.GetCells();

            float cellSize = GridController.Instance.cellSize;

            Gizmos.color = new Color(0, 0.5f, 1f, 0.7f);

            foreach (Vector2Int cell in cells) {
                Vector3 cellWorldPos = transform.position + new Vector3(
                    cell.x * cellSize,
                    0.5f,
                    cell.y * cellSize
                );
                Gizmos.DrawWireCube(cellWorldPos, new Vector3(cellSize, 1f, cellSize));
            }
        }
#endif
    }
}