using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace CozyPolyomino
{
    [CreateAssetMenu(fileName = "New Level", menuName = "Cozy Polyomino/Level Data")]
    public class LevelData : ScriptableObject
    {
        public int levelId;
        
        [Space]
        
        public string levelName = "New Level";
        public int gridWidth = 5;
        public int gridHeight = 5;

        [SerializeField] private List<PlacedObject> placedObjects = new List<PlacedObject>();
        
         // ========== Bonus System ==========
        [System.Serializable]
        public class BonusCell
        {
            public Vector2Int position;
            public BonusType type;

            public BonusCell(Vector2Int pos, BonusType t) {
                position = pos;
                type = t;
            }
        }

        [SerializeField] private List<BonusCell> bonuses = new List<BonusCell>();

        public IReadOnlyList<BonusCell> Bonuses => bonuses.AsReadOnly();

        public BonusType GetBonusAt(Vector2Int cell)
        {
            foreach (var bonus in bonuses)
                if (bonus.position == cell)
                    return bonus.type;
            
            return default;
        }

        public bool TryGetBonusAt(Vector2Int cell, out BonusType type) {
            foreach (var bonus in bonuses) {
                if (bonus.position != cell) continue;
                
                type = bonus.type;
                
                return true;
            }
            
            type = default;
            
            return false;
        }

        public void RemoveBonus(Vector2Int cell) {
            bonuses.RemoveAll(x => x.position == cell);
        }

        public void SetBonus(Vector2Int cell, BonusType type) {
            bonuses.RemoveAll(b => b.position == cell);
            bonuses.Add(new BonusCell(cell, type));
        }

        public ObjectType? GetObjectTypeAtCell(Vector2Int cell) {
            foreach (PlacedObject obj in placedObjects)
            {
                Vector2Int[] cells = obj.shapeData.GetCells();
                foreach (Vector2Int shapeCell in cells)
                {
                    Vector2Int transformed = ApplyTransform(shapeCell, obj.rotation, obj.isFlipped);
                    Vector2Int final = obj.gridPosition + transformed;
                    if (final == cell)
                        return obj.objectType;
                }
            }
            return null;
        }

        public IReadOnlyList<PlacedObject> PlacedObjects => placedObjects.AsReadOnly();

        public void ClearLevel() {
            placedObjects.Clear();
        }

        public void AddObject(PlacedObject obj) {
            placedObjects.Add(obj);
        }

        public void RemoveObject(PlacedObject obj) {
            placedObjects.Remove(obj);
        }

        public bool IsCellOccupied(Vector2Int cell) {
            foreach (PlacedObject placedObj in placedObjects) {
                Vector2Int[] cells = placedObj.shapeData.GetCells();
                
                foreach (Vector2Int shapeCell in cells) {
                    Vector2Int transformedCell = ApplyTransform(shapeCell, placedObj.rotation, placedObj.isFlipped);
                    Vector2Int finalCell = placedObj.gridPosition + transformedCell;
                    if (finalCell == cell) return true;
                }
            }

            return false;
        }

        private Vector2Int ApplyTransform(Vector2Int cell, int rotation, bool isFlipped) {
            Vector2Int result = cell;

            if (isFlipped) result.x = -result.x;

            for (int i = 0; i < rotation / 90; i++) result = new Vector2Int(-result.y, result.x);

            return result;
        }

#if UNITY_EDITOR

        [MenuItem("CONTEXT/LevelData/Clear All Objects")]
        private static void ClearAllObjects(MenuCommand command) {
            LevelData level = (LevelData)command.context;
            level.ClearLevel();
            EditorUtility.SetDirty(level);
        }

        [MenuItem("CONTEXT/LevelData/Duplicate Level")]
        private static void DuplicateLevel(MenuCommand command) {
            LevelData original = (LevelData)command.context;
            LevelData duplicate = Instantiate(original);
            duplicate.levelName = original.levelName + " Copy";

            string path = AssetDatabase.GetAssetPath(original);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(duplicate, newPath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = duplicate;
        }

#endif
    }

    [System.Serializable]
    public enum ObjectType
    {
        DraggableObject,
        Obstacle
    }

    [System.Serializable]
    public class PlacedObject
    {
        public ShapeData shapeData;
        public ObjectType objectType;
        public Vector2Int gridPosition;
        public int rotation;
        public bool isFlipped;
    }
}