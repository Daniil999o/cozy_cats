using UnityEngine;

public class YG_DistanceTarget : MonoBehaviour
{
    [SerializeField] private string targetName = "Player";

    private static Transform _instance;

    public static Transform Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<YG_DistanceTarget>().transform;

            return _instance;
        }
    }
}