using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyPolyomino
{
    public class LevelSpawner : MonoBehaviour
    {
        public static int CurrentLevel = 0;
        
        [SerializeField] private int currentLevel = 0;
        [SerializeField] private LevelSystemData levelSystem;

        [Space]

        [SerializeField] private ShapeSkinsData currentSkins;

        [Space]

        [SerializeField] private Transform objectsParent;
        [SerializeField] private Bounds elementsSpawnerBounds;
        [SerializeField] private Bounds elementsNoSpawnBounds;
        [SerializeField] private List<GameObject> spawnedObjects = new List<GameObject>();
        [SerializeField] private List<DraggableShape> spawnedDraggables = new List<DraggableShape>();

        [Space]

        [SerializeField] private UnityEvent onRegenerate;
        [SerializeField] private UnityEvent onWin;
        [SerializeField] private UnityEvent<int> onGoldBonus;
        [SerializeField] private UnityEvent<int> onBrilliantBonus;

        private static LevelSpawner _instance;

        internal static LevelSpawner Instance {
            get {
                if (!_instance) _instance = FindFirstObjectByType<LevelSpawner>(FindObjectsInactive.Include);
                return _instance;
            }
        }

        public DraggableShape[] GetDraggablesOnCell(Vector2Int cellPos) =>
            spawnedDraggables.Where(x => x.GetCellsOnGrid().Contains(cellPos)).ToArray();

        private bool _win;

#if UNITY_EDITOR

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.aquamarine;
            Gizmos.DrawWireCube(elementsSpawnerBounds.center, elementsSpawnerBounds.size);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(elementsNoSpawnBounds.center, elementsNoSpawnBounds.size);
        }

#endif
        
        public void RegenerateLevel() {
            currentLevel = CurrentLevel;
            
            _win = false;
            
            spawnedObjects.Where(x => x).ToList().ForEach(Destroy);
            
            spawnedObjects = new List<GameObject>();
            spawnedDraggables = new List<DraggableShape>();
            
            BonusSpawner.Instance.ClearBonuses();
            ObstacleSpawner.Instance.ClearObstacles();

            LevelData level = levelSystem.GetLevel(currentLevel);
            
            GridController.Instance.gridWidth = level.gridWidth;
            GridController.Instance.gridHeight = level.gridHeight;

            GridController.Instance.Initialize();

            foreach (var obj in level.PlacedObjects) {
                if (obj == null) continue;

                ShapeSkinsData.ShapeObjects shapeObjects = currentSkins.GetShapeObjects(obj.shapeData);

                switch (obj.objectType) {
                    case ObjectType.DraggableObject:
                        Vector3 randomPos;
                        
                        do {
                            randomPos = new Vector3(
                                Random.Range(elementsSpawnerBounds.min.x, elementsSpawnerBounds.max.x),
                                Random.Range(elementsSpawnerBounds.min.y, elementsSpawnerBounds.max.y),
                                Random.Range(elementsSpawnerBounds.min.z, elementsSpawnerBounds.max.z)
                            );
                        } while (elementsNoSpawnBounds.Contains(randomPos));



                        DraggableShape spawned = Instantiate(shapeObjects.draggableShape, randomPos,
                            Quaternion.identity
                        );
                        
                        spawnedObjects.Add(spawned.gameObject);
                        spawnedDraggables.Add(spawned);
                        
                        spawned.transform.parent = objectsParent;

                        spawned.OnPlaced += CheckAllDraggables;
                        
                        break;
                    case ObjectType.Obstacle:
                        ObstacleSpawner.Instance.SpawnSingleObstacle(obj.gridPosition, shapeObjects.obstacle,
                            obj.shapeData.GetCells()
                        );
                        
                        break;
                }
            }

            foreach (var x in level.Bonuses) {
                if (x == null) continue;

                BonusCell prefab = currentSkins.GetBonusCell(x.type);
                
                BonusSpawner.Instance.SpawnBonus(x.position, prefab);
            }
            
            GridDrawer.Instance.Initialize();
            
            onRegenerate?.Invoke();
        }

        private void CheckAllDraggables() {
            if (spawnedDraggables.Any(x => !x.Placed)) return;

            Win();
        }

        private void Win() {
            if (_win) return;

            _win = true;

            Dictionary<BonusType, int> bonuses = BonusSpawner.Instance.GetCollectedBonuses();

            foreach (var x in bonuses) {
                switch (x.Key) {
                    case BonusType.Gold:
                        onGoldBonus?.Invoke(x.Value);
                        break;
                    case BonusType.Brilliant:
                        onBrilliantBonus?.Invoke(x.Value);
                        break;
                }
            }
            
            onWin?.Invoke();
        }

        public void NextLevel() {
            CurrentLevel++;
            currentLevel = CurrentLevel;
            RegenerateLevel();
        }
    }
}