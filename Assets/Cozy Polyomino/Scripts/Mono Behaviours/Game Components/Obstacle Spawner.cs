using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CozyPolyomino
{
    public class ObstacleSpawner : MonoBehaviour
    {
        private GridController _gridController;
        private readonly List<Obstacle> _spawnedObstacles = new List<Obstacle>();

        private static ObstacleSpawner _instance;

        internal static ObstacleSpawner Instance {
            get {
                if (!_instance) _instance = FindFirstObjectByType<ObstacleSpawner>(FindObjectsInactive.Include);

                return _instance;
            }
        }

        private void Awake() {
            _gridController = GridController.Instance;
        }

        internal void SpawnSingleObstacle(Vector2Int gridPos, Obstacle prefab, Vector2Int[] cells) {
            Vector3 worldPos = _gridController.GridToWorldCenter(gridPos);
            worldPos.y = _gridController.transform.position.y;

            Obstacle obstacleObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            obstacleObj.name = $"Obstacle_{gridPos.x}_{gridPos.y}";

            _spawnedObstacles.Add(obstacleObj);
            obstacleObj.InitializeObstacle(gridPos, cells);
        }

        internal void ClearObstacles() {
            foreach (Obstacle x in _spawnedObstacles.Where(x => x)) {
                x.removeFromGrid = false;
                Destroy(x.gameObject);
            }

            _spawnedObstacles.Clear();
        }
    }
}