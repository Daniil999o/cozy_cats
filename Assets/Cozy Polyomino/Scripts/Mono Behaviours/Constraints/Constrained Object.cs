using UnityEngine;

namespace CozyPolyomino
{
    public class ConstrainedObject : MonoBehaviour
    {
        private Transform _transform;

        private void Awake() {
            _transform = GetComponent<Transform>();
        }

        private void LateUpdate() {
            _transform.position = ShapeConstrainer.Instance.GetPossiblePosition(_transform.position);
        }
    }
}