using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class YG_MenuCamera : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera cam;
    
    [Space]
    
    [Header("Camera Options")]
    [SerializeField] private CameraOptions defaultOptions = new CameraOptions {
        fov = 60, moveSpeed = 2, rotSpeed = 2
    };

    [Space]
    [Header("Points")]
    [SerializeField] private int currentPoint;

    [SerializeField] private List<CameraPoint> cameraPoints;

    [Header("Breathing Camera")]
    public bool enableBreathing = true;

    public float breathingStrength = 0.05f;
    public float breathingSpeed = 1f;

    [Header("Debug / Extra")]
    public bool autoMoveOnStart;

    public int startPoint;

    [System.Serializable]
    private class CameraPoint
    {
        public Transform point;

        [Space]
        public AnimationCurve trajectoryCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Space]
        [Header("Camera Options")]
        public bool useCustomOptions;

        public CameraOptions options;
    }

    [System.Serializable]
    private struct CameraOptions
    {
        public float fov; // Field of View
        public float moveSpeed;
        public float rotSpeed;
    }

    private Transform _camT;

    private bool _isMoving;
    private Vector3 _moveStartPosition;
    private Quaternion _moveStartRotation;
    private float _moveStartFOV;
    private float _moveTime;
    private float _moveDuration;
    private Vector3 _breathingOffset;

    private void Awake() {
        _camT = cam.transform;
    }

    private void Start() {
        if (autoMoveOnStart && cameraPoints.Count > startPoint) MoveToPoint(startPoint);
    }

    public void SetPoint(int id) {
        if (id >= 0 && id < cameraPoints.Count) MoveToPoint(id);
    }

    public void SetPoint(Transform point) {
        CameraPoint myPoint = cameraPoints.FirstOrDefault(x => x.point == point);
        if (myPoint != null) SetPoint(cameraPoints.IndexOf(myPoint));
    }

    public void MoveToPoint(int pointIndex) {
        if (pointIndex < 0 || pointIndex >= cameraPoints.Count) return;
        
        currentPoint = pointIndex;

        // -- Get settings for new point
        CameraPoint camPoint = cameraPoints[pointIndex];

        CameraOptions targetOptions = camPoint.useCustomOptions ? camPoint.options : defaultOptions;
        float distance = Vector3.Distance(_camT.position, camPoint.point.position);
        _moveDuration =
            Mathf.Max(0.05f, distance / targetOptions.moveSpeed
            ); // Считаем длительность (чем дальше - тем медленнее/дольше)

        // -- Save start state, flag moving
        _moveStartPosition = _camT.position;
        _moveStartRotation = _camT.rotation;
        _moveStartFOV = cam ? cam.fieldOfView : 60f;
        _moveTime = 0f;
        _isMoving = true;
    }

    private void LateUpdate() {
        // -- Moving between points with curve
        if (_isMoving && cameraPoints.Count > currentPoint) {
            CameraPoint target = cameraPoints[currentPoint];
            _moveTime += Time.deltaTime;
            float t = Mathf.Clamp01(_moveTime / _moveDuration);
            float curveT = target.trajectoryCurve.Evaluate(t);

            // -- Position and rotation lerp with curve
            Vector3 targetPos = target.point.position;
            Quaternion
                targetRot = target.point.rotation;
            float targetFOV = target.useCustomOptions ? target.options.fov : defaultOptions.fov;

            _camT.position = Vector3.Lerp(_moveStartPosition, targetPos, curveT);
            _camT.rotation = Quaternion.Slerp(_moveStartRotation, targetRot, curveT);

            // FOV Animated
            if (cam) cam.fieldOfView = Mathf.Lerp(_moveStartFOV, targetFOV, curveT);

            if (t >= 1f) {
                _isMoving = false;
                // Ensure final snap in case interpolation doesn't reach exactly
                _camT.position = targetPos;
                _camT.rotation = targetRot;
                if (cam) cam.fieldOfView = targetFOV;
            }
        }

        // -- Camera breathing effect
        if (enableBreathing && !_isMoving) {
            float breath = Mathf.Sin(Time.time * breathingSpeed);
            float roll = Mathf.Cos(Time.time * breathingSpeed * 0.55f);
            _breathingOffset.x = breath * breathingStrength;
            _breathingOffset.y = roll * (breathingStrength * 0.5f);
            _breathingOffset.z = 0f;
            _camT.position += _camT.right * _breathingOffset.x + _camT.up * _breathingOffset.y;
        }
    }

    // --- Debugging / Helper (например, можно в редакторе вызвать или кнопкой)
#if UNITY_EDITOR
    [ContextMenu("Move To Next Point")]
    public void MoveToNext() {
        int next = (currentPoint + 1) % cameraPoints.Count;
        MoveToPoint(next);
    }
#endif
}