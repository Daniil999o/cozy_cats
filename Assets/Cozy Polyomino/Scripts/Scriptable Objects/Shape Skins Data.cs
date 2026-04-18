using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace CozyPolyomino
{
    [CreateAssetMenu(fileName = "New Shape Skins", menuName = "Cozy Polyomino/Shape Skins")]
    public class ShapeSkinsData : ScriptableObject
    {
        [SerializeField] internal SerializedDictionary<ShapeData, ShapeObjects> shapes;
        [SerializeField] internal SerializedDictionary<BonusType, BonusCell> bonusCells;
        
        internal ShapeObjects GetShapeObjects(ShapeData shape) => shapes[shape];

        internal BonusCell GetBonusCell(BonusType type) => bonusCells.GetValueOrDefault(type);

        [System.Serializable]
        internal class ShapeObjects
        {
            [SerializeField] internal DraggableShape draggableShape;
            [SerializeField] internal Obstacle obstacle;
        }
    }
}