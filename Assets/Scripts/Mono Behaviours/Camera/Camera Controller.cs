using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [Space]

    [SerializeField] private float horSpeed = 2f;
    [SerializeField] private float verSpeed = 2f;

    [Space]

    [SerializeField] private Vector3 minConstraints = new Vector3(-5f, 0f, -3f);
    [SerializeField] private Vector3 maxConstraints = new Vector3(5f, 5f, 3f);

    [Space]

    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;

    [Space]

    [SerializeField] private bool canRotate;
    [SerializeField] private float rotSpeed = 4f;
    [SerializeField] private Vector2 clampRot = new Vector2(-80f, 80f);
    
    private Transform _transform;
    private Vector3 _moveDelta;
    private Vector3 _rotDelta;

    private bool _isMobile;

    private void Awake() {
        _transform = GetComponent<Transform>();
    }

    private void Start() {
        _isMobile = YG_Provider.IsMobile;
    }

    private void OnEnable() {
        moveAction.action.Enable();
        sprintAction.action.Enable();
    }

    private void OnDisable() {
        moveAction.action.Disable();
        sprintAction.action.Disable();
    }
    
    private void LateUpdate() {
        if (!_isMobile) {
            Vector2 inputDelta = moveAction.action.ReadValue<Vector2>();

            if (inputDelta.magnitude > 0f) {
                float speed = sprintAction.action.IsPressed() ? 12f : 6f;
                
                _moveDelta.x = inputDelta.x * speed;
                _moveDelta.z = inputDelta.y * speed;
            }
        }
        
        _moveDelta.x *= horSpeed;
        _moveDelta.z *= verSpeed;
        
        _transform.Translate(_moveDelta * Time.smoothDeltaTime);
        
        _moveDelta = Vector3.zero;

        Vector3 pos = _transform.position;
        pos.x = Mathf.Clamp(pos.x, minConstraints.x, maxConstraints.x);
        pos.y = Mathf.Clamp(pos.y, minConstraints.y, maxConstraints.y);
        pos.z = Mathf.Clamp(pos.z, minConstraints.z, maxConstraints.z);
        
        _transform.position = pos;

        _rotDelta *= rotSpeed;

        _transform.Rotate(_rotDelta * Time.smoothDeltaTime);

        Vector3 angles = _transform.localEulerAngles;
        
        float xAngle = angles.x;
        
        if (xAngle > 180f) xAngle -= 360f;

        xAngle = Mathf.Clamp(xAngle, clampRot.x, clampRot.y);
        
        angles.x = xAngle;
        angles.z = 0f;

        _transform.localRotation = Quaternion.Euler(angles);
        
        _rotDelta = Vector2.zero;
    }
    
    public void Move(Vector2 delta) {
        _moveDelta = new Vector3(-delta.x, 0f, -delta.y);
    }

    public void Rotate(Vector2 delta) {
        _rotDelta = new Vector3(-delta.y, delta.x, 0f);
    }
}