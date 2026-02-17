using UnityEngine;

public abstract class YG_Vehicle : MonoBehaviour
{
    [field: Header("Values")]
    [field: SerializeField] public Vector3 Velocity { get; protected set; }
    [field: SerializeField] public float Speed { get; protected set; }
    [field: SerializeField] public float Direction { get; protected set; }
    [field: SerializeField] public bool AwaitGroup { get; set; }
    [field: SerializeField] public Transform MovableTarget { get; set; }

    [field: Space]

    [field: Header("Parameters")]
    [field: SerializeField] public bool ActiveController { get; set; } = true;
    [field: SerializeField] public bool MovingForward { get; set; }
    [field: SerializeField] public bool FixedYRotationOnStart { get; set; }
    [field: SerializeField] public float SpeedModifier { get; set; } = 1;

    [field: Space]

    [field: Header("Path")]
    [field: SerializeField] public YG_VehiclePath VehiclePath { get; set; }
    [field: SerializeField] public int CurrentPoint { get; protected set; }
    [field: SerializeField] private float MinPointDistance { get; set; } = 1f;

    protected Transform CurrentWayPoint {
        get {
            if (MovableTarget) return MovableTarget;
            
            if (!VehiclePath) return null;
            
            Waypoint waypoint = VehiclePath.waypoints.GetElement(CurrentPoint);

            return waypoint != null ? waypoint.transform : null;
        }
    }
    
    [field: Space]

    [Header("Car Settings")]
    [SerializeField] protected CarSettings carSettings;

    [Space]

    [Header("Tracks")]
    [SerializeField] private bool yAxis = true;
    [SerializeField] private float tracksOffsetSpeed = 2f;
    [SerializeField] private YG_IndexedMaterial[] tracksMaterials;

    [Space]

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedAnimatorName = "Speed";

    protected Rigidbody _rb;
    protected Transform _transform;

    protected float _startYRotation;
    
    public virtual void ResetAll() { }
    
    private void Awake() {
        _transform = GetComponent<Transform>();
        _rb = GetComponent<Rigidbody>();

        _rb.centerOfMass = carSettings.centerOfMass;
        
        if (FixedYRotationOnStart) _startYRotation = _rb.rotation.eulerAngles.y;
    }

    private void LateUpdate() {
        if (!ActiveController) return;
        
        UpdateAnimator();
    }

    protected void UpdatePoints() {
        if (MovableTarget) return;
        
        if (CurrentWayPoint) {
            float distance = Vector3.Distance(_rb.position, CurrentWayPoint.position);
            
            if (distance < MinPointDistance) {
                if (VehiclePath.IsEndPoint(CurrentPoint)) {
                    if (VehiclePath.isClosedLoop)
                        CurrentPoint = 0;
                    else
                        MovingForward = false;
                } else {
                    CurrentPoint++;
                }
            }
        }
    }
    
    protected void UpdateTracks() {
        float offsetSpeed = Speed * -Direction * tracksOffsetSpeed;
        
        foreach (var x in tracksMaterials) {
            float value = offsetSpeed * Time.deltaTime;
            
            x.Material.mainTextureOffset += yAxis ? Vector2.up * value : Vector2.right * value;
        }
    }

    protected virtual void UpdateAnimator() {
        if (!animator) return;
        
        animator.SetFloat(speedAnimatorName, Speed);
    }
}