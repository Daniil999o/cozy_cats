using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CozyPolyomino
{
    public class LevelSpawner : MonoBehaviour
    {
        private static int CurrentLevel = 0;
        
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

        [SerializeField] private UnityEvent onWin;

        private bool _win;

#if UNITY_EDITOR

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.aquamarine;
            Gizmos.DrawWireCube(elementsSpawnerBounds.center, elementsSpawnerBounds.size);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(elementsNoSpawnBounds.center, elementsNoSpawnBounds.size);
        }

#endif

        private void Start() {
            currentLevel = CurrentLevel;
            
            SpawnLevel();
        }
        
        private void SpawnLevel() {
            spawnedObjects.Where(x => x).ToList().ForEach(Destroy);
            
            spawnedObjects.Clear();
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
        }

        private void CheckAllDraggables() {
            if (spawnedDraggables.Any(x => !x.Placed)) return;

            Win();
        }

        private void Win() {
            if (_win) return;

            _win = true;
            
            onWin?.Invoke();
        }

        public void NextLevel() {
            CurrentLevel++;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}