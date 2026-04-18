using UnityEngine;

namespace CozyPolyomino
{
    public class BonusCell : MonoBehaviour
    {
        [SerializeField] internal bool blockDraggables;
        [SerializeField] internal int count = 1;
        [SerializeField] internal BonusType bonusType;
        
        internal Vector2Int OnGridPosition { get; set; }

        public virtual void NotifyGridChanged(Vector2Int[] draggablePositions, Vector2Int[] obstaclePositions) {
            
        }
    }

    public enum BonusType
    {
        Gold,
        Brilliant,
        Vase,
        Klubok
    }
}