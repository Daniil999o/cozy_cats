using UnityEngine;
using System.Collections;

public class SausageRotation : MonoBehaviour
{
    [SerializeField] private SausageOptions options;

    [Space]
    
    [SerializeField] private Animator animator;
    [SerializeField] private Transform[] bones;

    private Quaternion[] _startLocalRotations;
    private float[] _currentAngles;
    private float[] _angularVelocities;
    private Vector3[] _lastRotationAxes;

    private Vector3 _previousPosition;
    private Vector3 _previousVelocity;
    private Vector3 _smoothedVelocity;
    private Vector3 _velocityRef;

    private bool _isDragging;
    private bool _isReturning;
    private bool _initialized;

    private IEnumerator _setDragCoroutine;

    private void Start() {
        if (bones == null || bones.Length == 0) return;

        int count = bones.Length;
        _startLocalRotations = new Quaternion[count];
        _currentAngles = new float[count];
        _angularVelocities = new float[count];
        _lastRotationAxes = new Vector3[count];

        for (int i = 0; i < count; i++) {
            _startLocalRotations[i] = bones[i].localRotation;
            _currentAngles[i] = 0f;
            _angularVelocities[i] = 0f;
            _lastRotationAxes[i] = Vector3.forward;
        }

        _previousPosition = transform.position;
        _initialized = true;
    }

    public void SetDragging(bool dragging) {
        if (_setDragCoroutine != null)
            StopCoroutine(_setDragCoroutine);

        _setDragCoroutine = IE_SetDragging(dragging, dragging ? 0.2f : 0f);
        
        StartCoroutine(_setDragCoroutine);
    }

    private IEnumerator IE_SetDragging(bool dragging, float delay) {
        if (_isDragging == dragging) yield break;

        yield return new WaitForSeconds(delay);
        
        _isDragging = dragging;

        if (dragging) {
            for (int i = 0; i < bones.Length; i++) _startLocalRotations[i] = bones[i].localRotation;

            if (animator) animator.enabled = false;
            _isReturning = false;

            _previousPosition = transform.position;
            _smoothedVelocity = Vector3.zero;
            _velocityRef = Vector3.zero;
            _previousVelocity = Vector3.zero;

            for (int i = 0; i < bones.Length; i++) {
                _currentAngles[i] = 0f;
                _angularVelocities[i] = 0f;
                _lastRotationAxes[i] = Vector3.forward;
            }
        } else {
            _isReturning = true;
        }
    }

    private void LateUpdate() {
        if (!_initialized) return;

        if (_isReturning) {
            bool allBonesReturned = true;
            float step = Time.deltaTime * options.returnSpeed;

            for (int i = 0; i < bones.Length; i++) {
                bones[i].localRotation = Quaternion.Slerp(bones[i].localRotation, _startLocalRotations[i], step);

                if (Quaternion.Angle(bones[i].localRotation, _startLocalRotations[i]) > 0.5f) allBonesReturned = false;
            }

            if (!allBonesReturned) return;
            
            _isReturning = false;
            if (animator) animator.enabled = true;

            return;
        }

        if (!_isDragging) return;
        
        float dt = Time.deltaTime;
        
        if (dt <= 0) return;

        Vector3 currentPos = transform.position;
        Vector3 rawVelocity = (currentPos - _previousPosition) / dt;
        _previousPosition = currentPos;

        _smoothedVelocity =
            Vector3.SmoothDamp(_smoothedVelocity, rawVelocity, ref _velocityRef, options.inertiaSmoothing);

        Vector3 acceleration = (_smoothedVelocity - _previousVelocity) / dt;
        _previousVelocity = _smoothedVelocity;

        for (int i = 0; i < bones.Length; i++) {
            Transform bone = bones[i];
            Quaternion baseRot = _startLocalRotations[i];

            Vector3 localInertia = bone.parent.InverseTransformDirection(_smoothedVelocity);
            Vector3 localAccel = bone.parent.InverseTransformDirection(acceleration);

            Vector3 rotationAxis;
            if (localInertia.magnitude > 0.001f)
                rotationAxis = Vector3.Cross(Vector3.up, localInertia).normalized;
            else if (localAccel.magnitude > 0.001f)
                rotationAxis = Vector3.Cross(Vector3.up, localAccel).normalized;
            else
                rotationAxis = _lastRotationAxes[i];

            _lastRotationAxes[i] = rotationAxis;

            float speedFactor = localInertia.magnitude * options.swaySensitivity;
            float accelFactor = localAccel.magnitude * options.accelerationSensitivity;
            float targetAngle = speedFactor + accelFactor;

            float tailFactor = 1f + i * 0.3f;
            targetAngle *= tailFactor;

            targetAngle = Mathf.Clamp(targetAngle, -options.maxAngle, options.maxAngle);

            float force = options.springStiffness * (targetAngle - _currentAngles[i]) -
                          options.springDamping * _angularVelocities[i];
            _angularVelocities[i] += force * dt;
            _currentAngles[i] += _angularVelocities[i] * dt;

            Quaternion swing = Quaternion.AngleAxis(-_currentAngles[i], rotationAxis);

            Quaternion myRot = baseRot * swing;

            bone.localRotation = Quaternion.Lerp(bone.localRotation, myRot, options.lerpSpeed * Time.smoothDeltaTime);
        }
    }
}