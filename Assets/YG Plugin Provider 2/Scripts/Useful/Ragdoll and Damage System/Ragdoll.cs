using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    [Header("Options")]
    public bool alwaysCollision;
    [Range(0.1f, 100f)] public float massModifier = 1f;
    
    [Space]
    
    [Header("Ragdoll")]
    [SerializeField] private bool active;
    [SerializeField] private RagdollPart[] parts;

    public bool Active {
        get => active;
        set {
            active = value;

            for (int i = 0; i < parts.Length; i++)
                parts[i].Active = active;

            if (animator) animator.enabled = !active;

            if (active) {
                foreach (var x in rigidbodiesToDestroy)
                    Destroy(x);
            }
        }
    }

    [Space]

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Space]

    [Header("Incalculable Rigidbodies")]
    [SerializeField] private Rigidbody[] rigidbodiesToDestroy;

    [Space]

    [Header("Collision Audio")]
    [SerializeField] private AudioClip[] collisionClips;
    [SerializeField] private float collisionDelay = 0.5f;

    private float _collisionTimer;

    private void Start() {
        parts = GetComponentsInChildren<RagdollPart>();

        Active = active;
    }

    private void Update() {
        if (_collisionTimer > 0f) _collisionTimer -= Time.deltaTime;
    }

    public void PlayCollision(Vector3 pos) {
        if (_collisionTimer > 0f) return;

        _collisionTimer = collisionDelay;
        
        AudioSourceExtensions.ShotFx(collisionClips, pos, AudioSourceExtensions.VeryQuietFxPreset);
    }
}