using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(VehiclesGroup))]
public class VehiclesConvoy : MonoBehaviour
{
    [SerializeField] private float updateTimer = 5f;
    [SerializeField] private float maxDistance = 20f;
    
    private YG_Vehicle[] _vehicles;
    private VehiclesGroup _vehiclesGroup;

    private float _timer;

    private void Awake() {
        _vehiclesGroup = GetComponent<VehiclesGroup>();
    }

    private void Start() {
        _vehicles = _vehiclesGroup.Vehicles;
    }

    private void Update() {
        if (_timer <= 0f) {
            _timer = updateTimer;
            CheckVehicles();
        } else {
            _timer -= Time.deltaTime;
        }
    }

    private void CheckVehicles() {
        _vehicles = _vehicles.OrderByDescending(x => x.CurrentPoint).ToArray();

        foreach (var x in _vehicles) x.AwaitGroup = false;

        for (int i = 0; i < _vehicles.Length; i++) {
            YG_Vehicle anotherVehicle = _vehicles.GetElement(i + 1);

            if (anotherVehicle) {
                float distance = Vector3.Distance(anotherVehicle.transform.position, _vehicles[i].transform.position);
                if (distance > maxDistance) {
                    for (int j = 0; j <= i; j++)
                        _vehicles[j].AwaitGroup = true;
                }
            }
        }
    }
}