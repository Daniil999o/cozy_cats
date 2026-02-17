using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [Space]

    [SerializeField] private float horSpeed = 2f;
    [SerializeField] private float verSpeed = 2f;

    [Space]

    [SerializeField] private Vector3 minConstraints = new Vector3(-5f, 0f, -3f);
    [SerializeField] private Vector3 maxConstraints = new Vector3(5f, 5f, 3f);
    
    private Transform _transform;
    private Vector3 _delta;

    private void Awake() {
        _transform = GetComponent<Transform>();
    }
    
    private void LateUpdate() {
        _delta.x *= horSpeed;
        _delta.z *= verSpeed;
        
        _transform.Translate(_delta * Time.smoothDeltaTime);
        
        _delta = Vector3.zero;

        Vector3 pos = _transform.position;
        pos.x = Mathf.Clamp(pos.x, minConstraints.x, maxConstraints.x);
        pos.y = Mathf.Clamp(pos.y, minConstraints.y, maxConstraints.y);
        pos.z = Mathf.Clamp(pos.z, minConstraints.z, maxConstraints.z);
        
        _transform.position = pos;
    }
    
    public void Move(Vector2 delta) {
        _delta = new Vector3(-delta.x, 0f, -delta.y);
    }
}