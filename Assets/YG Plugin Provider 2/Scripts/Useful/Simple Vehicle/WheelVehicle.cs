using System.Linq;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class WheelVehicle : YG_Vehicle
{
    [Space]

    [Header("Wheels")]
    [SerializeField] private WheelCollider[] wheelsToCreate;
    [SerializeField] private Wheel[] wheels;
    [SerializeField] private WheelSettings wheelSettings;

    private WheelCollider[] _oldWheels;
    
    [System.Serializable]
    private class WheelSettings
    {
        public float radius = 3f;
        public float spring = 10000f;
        public float damper = 30000f;
        public float stiffness = 2f;
        public float suspension = 1.2f;
    }

    [System.Serializable]
    private class Wheel
    {
        [SerializeField] private string name = "New Wheel";
    
        [Space]
        
        public WheelCollider collider;
        [SerializeField] private Transform model;

        [Space]

        [SerializeField] private bool steerWheel;
        [SerializeField] private bool powerWheel;
        [SerializeField] private bool brakeWheel = true;

        public void CreateWheel(WheelCollider wheel) {
            GameObject obj = wheel.gameObject;
            
            name = obj.name;
            collider = wheel;

            string parentPath = wheel.transform.parent.gameObject.GetFullParentPath();
            string path = $"{parentPath}/Models/{wheel.gameObject.name}";

            if (path.TryFindGameObject(out GameObject wheelModel)) model = wheelModel.transform;
        }

        public void ApplySettings(WheelSettings settings) {
            JointSpring springs = collider.suspensionSpring;
            springs.spring = settings.spring;
            springs.damper = settings.damper;

            WheelFrictionCurve forwardFriction = collider.forwardFriction;
            forwardFriction.stiffness = settings.stiffness;

            WheelFrictionCurve sidewaysFriction = collider.sidewaysFriction;
            sidewaysFriction.stiffness = settings.stiffness;

            collider.radius = settings.radius;
            collider.suspensionSpring = springs;
            collider.forwardFriction = forwardFriction;
            collider.sidewaysFriction = sidewaysFriction;
            collider.suspensionDistance = settings.suspension;
        }

        public void ApplyPower(float torque) {
            if (!powerWheel) return;

            collider.motorTorque = torque * 1000f;
        }

        public float SteerAngle {
            get => collider.steerAngle;
            set {
                if (!steerWheel) return;
                
                collider.steerAngle = value;
            }
        }

        public void ApplyBrake(float torque) {
            if (!brakeWheel) return;

            collider.brakeTorque = torque * 1000f;
        }

        public void UpdateModel() {
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);

            model.position = pos;
            model.rotation = rot;
        }
    }

    public override void ResetAll() {
        foreach (var x in wheels) {
            x.ApplyBrake(1f);
            x.ApplyPower(0f);
        }
    }
    
    private void OnValidate() {
        if (Application.isPlaying || wheelsToCreate == null) return;

        if (wheelsToCreate != _oldWheels) {
            _oldWheels = wheelsToCreate;

            foreach (var x in wheelsToCreate) {
                if (x && wheels.All(y => x != y.collider)) {
                    List<Wheel> newWheels = wheels.ToList();
                    Wheel newWheel = new Wheel();
                    newWheel.CreateWheel(x);

                    newWheels.Add(newWheel);
                    wheels = newWheels.ToArray();
                }
            }

            wheelsToCreate = new WheelCollider[] {};
        }
    }
    
    private void Start() {
        foreach (var x in wheels)
            x.ApplySettings(wheelSettings);
    }

    private void Update() {
        UpdateValues();
        UpdateTracks();
        MoveCar();
        
        UpdatePoints();
    }

    private void FixedUpdate() {
        if (!ActiveController || !FixedYRotationOnStart) return;

        Vector3 rot = _rb.rotation.eulerAngles;
        rot.y = _startYRotation;
        
        _rb.rotation = Quaternion.Euler(rot);
    }

    private void UpdateValues() {
        Velocity = _rb.linearVelocity;
        Speed = Velocity.magnitude;
        
        if (Speed < 0.1f) {
            Direction = 0;
        } else {
            float dotProduct = Vector3.Dot(_transform.forward, Velocity);

            Direction = dotProduct > 0 ? 1 : dotProduct < 0 ? -1 : 0;
        }
    }

    private void MoveCar() {
        if (!ActiveController) return;
        
        bool move = MovingForward && !AwaitGroup;
        bool highSpeed = Speed > carSettings.maxSpeed * SpeedModifier;
        
        foreach (var x in wheels) {
            x.ApplyPower(move && !highSpeed ? carSettings.power : 0f);
            x.ApplyBrake(!move || highSpeed ? carSettings.brakePower : 0f);
            
            if (move && Speed < 3 && carSettings.maxSpeed > 3)
                _rb.AddRelativeForce(Vector3.forward * 1f, ForceMode.Acceleration);

            float steerInput = 0f;

            if (CurrentWayPoint && move) {
                Vector3 directionToWaypoint = CurrentWayPoint.position - transform.position;
                float angleToWaypoint = Vector3.SignedAngle(_transform.forward, directionToWaypoint, Vector3.up);

                steerInput = Mathf.Clamp(angleToWaypoint, -carSettings.maxSteerAngle, carSettings.maxSteerAngle);
            }
            
            x.SteerAngle = Mathf.Lerp(x.SteerAngle, steerInput, carSettings.steerSpeed * Time.deltaTime);
            
            x.UpdateModel();
        }
    }
}