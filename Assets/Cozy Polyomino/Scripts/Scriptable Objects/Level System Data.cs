using UnityEngine;
using System.Collections.Generic;

namespace CozyPolyomino
{
    [CreateAssetMenu(fileName = "Level System", menuName = "Cozy Polyomino/Level System")]
    public class LevelSystemData : ScriptableObject
    {
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public IReadOnlyList<LevelData> Levels => levels.AsReadOnly();
        
        public LevelData GetLevel(int index) => Levels[index];

        public void AddLevel(LevelData level) {
            if (!levels.Contains(level)) levels.Add(level);
        }

        public void RemoveLevel(LevelData level) {
            levels.Remove(level);
        }

        public void ClearLevels() {
            levels.Clear();
        }
    }
}