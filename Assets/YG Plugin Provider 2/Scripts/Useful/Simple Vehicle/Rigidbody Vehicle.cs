using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyVehicle : YG_Vehicle
{
    
    private Quaternion _rotation = Quaternion.identity;

    private void Update() {
        UpdateValues();
        UpdateTracks();
        MoveRotation();
        
        UpdatePoints();
    }

    private void FixedUpdate() {
        MoveCar();

        if (FixedYRotationOnStart) {
            Vector3 rot = _rb.rotation.eulerAngles;
            rot.y = _startYRotation;

            _rb.rotation = Quaternion.Euler(rot);
        } else {
            _rb.MoveRotation(_rotation);
        }
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

        if (move && !highSpeed)
            _rb.AddRelativeForce(Vector3.forward * carSettings.power * 10f * Time.fixedDeltaTime, ForceMode.VelocityChange);
        if (!move && highSpeed)
            _rb.AddForce(
                -_rb.linearVelocity * carSettings.brakePower * carSettings.brakeDontMovePowerModifier * 500f *
                Time.fixedDeltaTime, ForceMode.VelocityChange
            );
    }

    private void MoveRotation() {
        if (!ActiveController) return;
        
        Quaternion myRotation = _rb.rotation;

        if (CurrentWayPoint && MovingForward) {
            Vector3 targetDirection = CurrentWayPoint.position - _rb.position;
            targetDirection.y = 0f;
            myRotation = Quaternion.LookRotation(targetDirection);
        }

        _rotation = Quaternion.Lerp(_rotation, myRotation, carSettings.steerSpeed * Time.deltaTime);
    }
}