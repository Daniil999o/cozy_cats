using UnityEngine;

public class TransformRotatable : MonoBehaviour
{
    [SerializeField] private Vector3 rotateDir = Vector3.forward;
    [SerializeField] private float rotateSpeed = 200f;

    public float Modifier { get; set; } = 1f;
    
    private Transform _transform;

    private void Awake() => _transform = GetComponent<Transform>();

    private void Update() {
        _transform.Rotate(rotateDir * rotateSpeed * Modifier * Time.deltaTime);
    }

    public void UpdateModifier(float value) {
        Modifier = value;
    }
}