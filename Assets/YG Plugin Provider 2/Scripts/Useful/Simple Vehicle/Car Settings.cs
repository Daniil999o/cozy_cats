using UnityEngine;

[System.Serializable]
public class CarSettings
{
    public float maxSpeed = 40f;
    public float power = 6f;
    public float brakePower = 5f;
    public float brakeDontMovePowerModifier = 1f;

    [Space]

    public float maxSteerAngle = 45f;
    public float steerSpeed = 2f;

    [Space]

    public Vector3 centerOfMass = new Vector3(0f, -0.1f, 0f);
}