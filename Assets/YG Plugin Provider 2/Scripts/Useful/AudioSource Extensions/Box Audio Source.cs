using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BoxAudioSource : MonoBehaviour
{
    [Header("Box Settings")]
    [SerializeField] private Vector3 minBounds = new Vector3(-5, -5, -5);
    [SerializeField] private Vector3 maxBounds = new Vector3(5, 5, 5);
    [SerializeField] private float outsideFalloff = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0, 1, 1, 0.3f);

    private AudioSource audioSource;

    private Transform _mainListener;

    private Transform Listener {
        get {
            if (!_mainListener) {
                AudioListener listener =
                    FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                        .FirstOrDefault(x => x && x.isActiveAndEnabled);

                if (listener)
                    _mainListener = listener.transform;
                else
                    Debug.LogError($"[{gameObject.name} | BoxAudioSource]: No AudioListener found in scene!");
            }

            return _mainListener;
        }
    }

    private Transform _transform;

    private void Awake() {
        _transform = GetComponent<Transform>();
        
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
    }

    private void Update() {
        if (!Listener || !audioSource.isPlaying) return;

        Vector3 localListenerPos = _transform.InverseTransformPoint(Listener.position);

        if (IsInsideBounds(localListenerPos))
            UpdateAudioForInsideBounds(localListenerPos);
        else
            UpdateAudioForOutsideBounds(localListenerPos);
    }

    private bool IsInsideBounds(Vector3 position) =>
        position.x >= minBounds.x && position.x <= maxBounds.x &&
        position.y >= minBounds.y && position.y <= maxBounds.y &&
        position.z >= minBounds.z && position.z <= maxBounds.z;

    private void UpdateAudioForInsideBounds(Vector3 localListenerPos) {
        Vector3 normalizedPos = new Vector3(
            Mathf.InverseLerp(minBounds.x, maxBounds.x, localListenerPos.x),
            Mathf.InverseLerp(minBounds.y, maxBounds.y, localListenerPos.y),
            Mathf.InverseLerp(minBounds.z, maxBounds.z, localListenerPos.z)
        );
    }

    private void UpdateAudioForOutsideBounds(Vector3 localListenerPos) {
        Vector3 closestPoint = new Vector3(
            Mathf.Clamp(localListenerPos.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(localListenerPos.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(localListenerPos.z, minBounds.z, maxBounds.z)
        );

        float distanceToBounds = Vector3.Distance(localListenerPos, closestPoint);

        float volumeMultiplier = CalculateOutsideVolume(distanceToBounds);

        audioSource.volume = Mathf.Clamp01(volumeMultiplier);
    }

    private float CalculateOutsideVolume(float distance) {
        if (outsideFalloff <= 0) return 0;

        float volume = Mathf.Exp(-distance / outsideFalloff);
        
        return Mathf.Clamp01(volume);
    }

    public void SetBounds(Vector3 newMinBounds, Vector3 newMaxBounds) {
        minBounds = newMinBounds;
        maxBounds = newMaxBounds;
    }

    public void SetFalloff(float newFalloff) {
        outsideFalloff = Mathf.Max(0.01f, newFalloff);
    }

    private void OnDrawGizmosSelected() {
        if (!drawGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        Vector3 center = (minBounds + maxBounds) * 0.5f;
        Vector3 size = maxBounds - minBounds;
        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }

#region Helper Methods

    public Vector3 GetMinBoundsWorld() => _transform.TransformPoint(minBounds);

    public Vector3 GetMaxBoundsWorld() => _transform.TransformPoint(maxBounds);

    public Bounds GetWorldBounds() =>
        new Bounds(
            _transform.TransformPoint((minBounds + maxBounds) * 0.5f),
            Vector3.Scale(maxBounds - minBounds, _transform.lossyScale)
        );

#endregion
}