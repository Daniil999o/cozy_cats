using UnityEngine;

public class YG_DistanceObject : MonoBehaviour
{
    [SerializeField] private bool useDefaultDisableDistance = true;
    [SerializeField] private float customDisableDistance = 50f;

    public float DisableDistance =>
        useDefaultDisableDistance ? YG_DistanceChecker.DefaultDisableDistance : customDisableDistance;
    
    private Transform _transform;

    private void Awake() {
        _transform = GetComponent<Transform>();
    }
    
    private void Update() {
        if (Vector3.Distance(_transform.position, YG_DistanceTarget.Instance.position) > DisableDistance)
            YG_DistanceChecker.HideObject(this);
    }
}