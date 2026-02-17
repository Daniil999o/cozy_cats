using UnityEngine;

namespace CozyPolyomino
{
    [CreateAssetMenu(fileName = "Drawer Colors", menuName = "Cozy Polyomino/Drawer Colors")]
    public class DrawerColorsData : ScriptableObject
    {
        [Header("Cell Settings")]
        [SerializeField] internal float visualCellScale = 0.2f;
        
        [Space]
        
        [Header("Colors")]
        [SerializeField] internal Color colorEmpty = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] internal Color colorOccupied = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        [SerializeField] internal Color colorValidHover = new Color(1f, 0.9f, 0.4f, 0.8f);
        [SerializeField] internal Color colorInvalidHover = new Color(1f, 0.3f, 0.3f, 0.8f);
        [SerializeField] internal Color colorBlocked = new Color(0f, 0f, 0f, 0f);
    }
}