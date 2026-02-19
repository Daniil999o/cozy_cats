using UnityEngine;

namespace CozyPolyomino
{
    public class ShapeConstrainer : MonoBehaviour
    {
        private static ShapeConstrainer _instance;

        public static ShapeConstrainer Instance {
            get {
                if (!_instance) _instance = FindFirstObjectByType<ShapeConstrainer>(FindObjectsInactive.Include);

                return _instance;
            }
        }

        [SerializeField] private Bounds shapeArea;

        public Vector3 GetPossiblePosition(Vector3 currentPos) =>
            shapeArea.size != Vector3.zero ? shapeArea.ClosestPoint(currentPos) : currentPos;

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(transform.position + shapeArea.center, shapeArea.size * transform.lossyScale.magnitude);
        }
    }
}