using UnityEngine;

namespace CozyPolyomino
{
    [CreateAssetMenu(fileName = "Draggable Options", menuName = "Cozy Polyomino/Draggable Options")]
    public class DraggableOptionsData : ScriptableObject
    {
        [Header("Speed Settings")]
        public float dragSpeed = 20f;
        public float liftHeight = 1.0f;
        public float snapStrength = 0.5f;
        public float returnSpeed = 10f;
    }
}