using UnityEngine;

[CreateAssetMenu(fileName = "Sausage Options", menuName = "Game/Sausage Options")]
public class SausageOptions : ScriptableObject
{
    public float swaySensitivity = 20f;

    public float inertiaSmoothing = 0.15f;

    public float lerpSpeed = 0.15f;

    public float maxAngle = 60f;

    public float accelerationSensitivity = 5f;

    public float springStiffness = 8f;
    public float springDamping = 1.2f;

    public float returnSpeed = 5f;
}