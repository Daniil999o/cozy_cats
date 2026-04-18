using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CozyPolyomino
{
    public class BonusSpawner : MonoBehaviour
    {
        private GridController _gridController;
        private readonly HashSet<BonusCell> _spawnedBonuses = new HashSet<BonusCell>();
        
        private static BonusSpawner _instance;

        internal static BonusSpawner Instance {
            get {
                if (!_instance) _instance = FindFirstObjectByType<BonusSpawner>(FindObjectsInactive.Include);
                return _instance;
            }
        }

        public bool AnyOnCell(Vector2Int cell) =>
            _spawnedBonuses.Any(x => x && x.blockDraggables && x.OnGridPosition == cell);

        public void NotifyGridChanged(Vector2Int[] gridPositions, Vector2Int[] obstaclePositions) {
            foreach (var x in _spawnedBonuses)
                x.NotifyGridChanged(gridPositions, obstaclePositions);
        }

        private void Awake() {
            _gridController = GridController.Instance;
        }

        internal void SpawnBonus(Vector2Int gridPos, BonusCell prefab) {
            Vector3 worldPos = _gridController.GridToWorldCenter(gridPos);
            worldPos.y = _gridController.transform.position.y;

            BonusCell obstacleObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            obstacleObj.name = $"Obstacle_{gridPos.x}_{gridPos.y}";

            obstacleObj.OnGridPosition = gridPos;

            _spawnedBonuses.Add(obstacleObj);
        }

        internal void ClearBonuses() {
            foreach (BonusCell x in _spawnedBonuses.Where(x => x))
                Destroy(x.gameObject);

            _spawnedBonuses.Clear();
        }

        internal Dictionary<BonusType, int> GetCollectedBonuses() {
            Dictionary<BonusType, int> collectedBonuses = new Dictionary<BonusType, int> {
                [BonusType.Gold] = 0,
                [BonusType.Brilliant] = 0
            };

            foreach (var x in _spawnedBonuses) {
                bool isCollected = _gridController.IsShapeOccupied(x.OnGridPosition);
                
                if (!isCollected || !collectedBonuses.ContainsKey(x.bonusType)) continue;
                
                collectedBonuses[x.bonusType] += x.count;
            }

            return collectedBonuses;
        }
    }
}