using UnityEngine;

public class VehiclesGroup : MonoBehaviour
{
    [SerializeField] private bool movingForward;
    [SerializeField] private YG_VehiclePath vehiclePath;

    public bool MovingForward {
        get => movingForward;
        set {
            movingForward = value;
            
            foreach (var x in Vehicles) x.MovingForward = movingForward;
        }
    }

    public YG_VehiclePath VehiclePath {
        get => vehiclePath;
        set {
            vehiclePath = value;

            foreach (var x in Vehicles) x.VehiclePath = vehiclePath;
        }
    }

    public YG_Vehicle[] Vehicles => GetComponentsInChildren<YG_Vehicle>(true);

    private void Start() {
        MovingForward = movingForward;
        VehiclePath = vehiclePath;
    }
}