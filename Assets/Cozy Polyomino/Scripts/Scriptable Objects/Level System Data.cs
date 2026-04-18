using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CozyPolyomino
{
    [CreateAssetMenu(fileName = "Level System", menuName = "Cozy Polyomino/Level System")]
    public class LevelSystemData : ScriptableObject
    {
        public ShapeSkinsData[] skins;
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public bool HasDraggableShape(ShapeData shapeData) =>
            skins.Any(x => x.shapes.Any(y => y.Value.draggableShape?.shapeData == shapeData));
        
        public bool HasObstacleShape(ShapeData shapeData) =>
            skins.Any(x => x.shapes.Any(y => y.Value.obstacle?.shapeData == shapeData));

        public IReadOnlyList<LevelData> Levels => levels.OrderBy(x => x.levelId).ToList().AsReadOnly();
        
        public LevelData GetLevel(int index) => Levels[index];

        public void AddLevel(LevelData level) {
            if (!levels.Contains(level)) levels.Add(level);

            levels = levels.OrderBy(x => x.levelId).ToList();
        }

        public void RemoveLevel(LevelData level) {
            levels.Remove(level);
        }

        public void ClearLevels() {
            levels.Clear();
        }
    }
}