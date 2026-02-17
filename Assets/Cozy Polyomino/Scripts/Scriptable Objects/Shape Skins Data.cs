using UnityEngine;
using UnityEngine.Rendering;

namespace CozyPolyomino
{
    [CreateAssetMenu(fileName = "New Shape Skins", menuName = "Cozy Polyomino/Shape Skins")]
    public class ShapeSkinsData : ScriptableObject
    {
        [SerializeField] internal SerializedDictionary<ShapeData, ShapeObjects> shapes;
        
        internal ShapeObjects GetShapeObjects(ShapeData shape) => shapes[shape];

        [System.Serializable]
        internal class ShapeObjects
        {
            [SerializeField] internal DraggableShape draggableShape;
            [SerializeField] internal Obstacle obstacle;
        }
    }
}