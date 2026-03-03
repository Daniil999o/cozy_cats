using UnityEngine;
using UnityEngine.EventSystems;

public class RoomObjectTransformation : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [field: SerializeField] public bool Move { get; set; }
    [field: SerializeField] public bool Rotate { get; set; }

    [field: Space]
    [SerializeField] private bool autoSet;

    [Space]
    [SerializeField] private float moveDelta = 2f;

    [SerializeField] private float rotateAngle = 45f;
    [SerializeField] private float rotateThreshold = 10f;

    [Space]
    [SerializeField] private RoomObject mainObject;

    [Space]
    
    [Header("Alignment")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private LayerMask placeableLayers;
    [SerializeField] private float skinWidth = 0.1f;

    private Vector3 _dragStartPosition;
    private Quaternion _dragStartRotation;
    private Collider[] _colliders;

    private Transform _objT;

    private float _snapDelay;
    private float _accumulatedDelta = 0f;

    private bool _dragging;
    private Vector2 _dragDelta;

    private bool CanInteract => Move || Rotate;

#if UNITY_EDITOR

    private void OnValidate() {
        if (autoSet) mainObject = GetComponent<RoomObject>();

        autoSet = false;
    }

#endif

    private void Awake() {
        if (!mainObject) mainObject = GetComponent<RoomObject>();

        _objT = mainObject.transform;

        _colliders = GetComponentsInChildren<Collider>(true);
    }

    private void Update() {
        if (_snapDelay > 0f) _snapDelay -= Time.deltaTime;

        if (!_dragging || !Rotate) return;

        if (_snapDelay > 0f) return;

        Vector2 delta = _dragDelta;

        if (!(delta.magnitude > 0f)) return;

        _snapDelay = 0.1f;

        _accumulatedDelta -= delta.x;

        int steps = Mathf.FloorToInt(_accumulatedDelta / rotateThreshold);

        if (steps != 0) {
            _accumulatedDelta -= steps * rotateThreshold;

            _objT.Rotate(0f, steps * rotateAngle, 0f, Space.World);
        }

        Vector3 rot = _objT.eulerAngles;
        rot.x = rot.z = 0;

        rot.y = Mathf.RoundToInt(rot.y);

        _objT.rotation = Quaternion.Euler(rot);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (!CanInteract) return;
        
        _dragging = true;

        _objT.gameObject.layer = RoomUIHandler.Instance.RigidbodyLayer;
        mainObject.SetDrag(true);
        mainObject.SetLayer(RoomUIHandler.Instance.RigidbodyLayer);

        _dragStartPosition = _objT.position;
        _dragStartRotation = _objT.rotation;
    }

    public void OnDrag(PointerEventData eventData) {
        if (!CanInteract) return;
        
        if (Rotate) {
            _dragDelta = eventData.delta;
            return;
        }

        if (!Move) return;

        Camera cam = RoomUIHandler.Instance.Camera;
        Ray ray = cam.ScreenPointToRay(eventData.position);

        if (!Physics.Raycast(
                ray, out RaycastHit hit, 100f, RoomUIHandler.Instance.RaycastLayerMask
            ))
            return;

        _objT.position = hit.point;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (!CanInteract) return;
        
        _dragging = false;

        _objT.gameObject.layer = RoomUIHandler.Instance.ObjectLayer;
        mainObject.SetLayer(RoomUIHandler.Instance.ObjectLayer);
        mainObject.SetDrag(false);

        AlignObject();
    }

    private void AlignObject() {
        bool hasPenetration = ResolvePenetrations();

        if (hasPenetration) {
            for (int i = 0; i < 10; i++) {
                _objT.position = RoomBounds.GetPositionInBounds(_objT.position, out bool changed);

                if (IsPositionValidInRooms(_objT.position, _objT.rotation)) break;

                ResolvePenetrations();
            }

            CorrectPosition();
            
            if (IsPositionValidInRooms(_objT.position, _objT.rotation)) return;
        }

        _objT.position = _dragStartPosition;
        _objT.rotation = _dragStartRotation;
    }

    private bool ResolvePenetrations() {
        bool resolved = false;

        for (int iter = 0; iter < 10; iter++) {
            bool anyPenetration = false;

            foreach (Collider x in _colliders) {
                if (!x.enabled) continue;

                Collider[] overlapping = Physics.OverlapBox(
                    x.bounds.center,
                    x.bounds.extents,
                    x.transform.rotation,
                    obstacleLayers | placeableLayers,
                    QueryTriggerInteraction.Ignore
                );

                foreach (Collider other in overlapping) {
                    if (other.transform.IsChildOf(transform)) continue;

                    bool isPlaceable = ((1 << other.gameObject.layer) & placeableLayers) != 0;

                    if (!Physics.ComputePenetration(
                            x, x.transform.position, x.transform.rotation,
                            other, other.transform.position, other.transform.rotation,
                            out Vector3 direction, out float distance
                        ))
                        continue;

                    if (distance <= 0f) continue;

                    if (isPlaceable) {
                        if (Vector3.Dot(direction, Vector3.up) > 0.7f) continue;
                    }

                    _objT.position += direction * (distance + skinWidth);

                    anyPenetration = true;
                }
            }

            if (anyPenetration) continue;

            resolved = true;

            break;
        }

        return resolved;
    }

    private void CorrectPosition() {
        Ray ray = new Ray(_objT.position, Vector3.down);

        if (!Physics.Raycast(
                ray, out RaycastHit hit, 100f, RoomUIHandler.Instance.RaycastLayerMask
            ))
            return;

        _objT.position = hit.point;
    }

    private bool IsPositionValidInRooms(Vector3 testPos, Quaternion testRot) {
        Vector3 originalPos = _objT.position;
        Quaternion originalRot = _objT.rotation;

        _objT.SetPositionAndRotation(testPos, testRot);

        bool valid = false;
        foreach (RoomBounds room in RoomBounds.AllRooms) {
            bool allInside = true;
            foreach (Collider col in _colliders) {
                if (!col.enabled) continue;

                Bounds worldBounds = col.bounds;
                Vector3[] corners = GetBoundsCorners(worldBounds);

                foreach (Vector3 corner in corners) {
                    Vector3 localCorner = room.transform.InverseTransformPoint(corner);
                    if (room.LocalBounds.Contains(localCorner)) continue;
                    
                    allInside = false;
                    
                    break;
                }

                if (!allInside) break;
            }

            if (!allInside) continue;
            
            valid = true;
            
            break;
        }

        _objT.SetPositionAndRotation(originalPos, originalRot);
        
        return valid;
    }

    private static Vector3[] GetBoundsCorners(Bounds bounds) {
        Vector3[] corners = new Vector3[8];
        Vector3 center = bounds.center;
        Vector3 ext = bounds.extents;

        corners[0] = center + new Vector3(-ext.x, -ext.y, -ext.z);
        corners[1] = center + new Vector3(ext.x, -ext.y, -ext.z);
        corners[2] = center + new Vector3(-ext.x, ext.y, -ext.z);
        corners[3] = center + new Vector3(ext.x, ext.y, -ext.z);
        corners[4] = center + new Vector3(-ext.x, -ext.y, ext.z);
        corners[5] = center + new Vector3(ext.x, -ext.y, ext.z);
        corners[6] = center + new Vector3(-ext.x, ext.y, ext.z);
        corners[7] = center + new Vector3(ext.x, ext.y, ext.z);

        return corners;
    }
}