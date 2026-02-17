using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RagdollPart : MonoBehaviour
{
    [SerializeField][Range(0f, 5f)] private float damageModifier = 1f;

    private bool _active;
    
    public bool Active {
        get => _active;
        set {
            _active = value;
            
            UpdateComponents();

            _rb.isKinematic = !_active;
            _rb.collisionDetectionMode = _active ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete;
            _rb.mass = _defaultMass * _main.massModifier;

            _coll.isTrigger = !_main.alwaysCollision && !_active;
        }
    }

    private Ragdoll _main;
    
    private Rigidbody _rb;
    private Collider _coll;

    private Health _health;

    private float _defaultMass;

    public bool Died => _health.Died;

    private void Awake() {
        UpdateComponents();
        
        _defaultMass = _rb.mass;
    }

    private void UpdateComponents() {
        if (!_main) _main = GetComponentInParent<Ragdoll>();
        if (!_rb) _rb = GetComponent<Rigidbody>();
        if (!_coll) _coll = GetComponent<Collider>();
        if (!_health) _health = GetComponentInParent<Health>();
    }

    public void AddForce(Vector3 force) => _rb.AddForce(force, ForceMode.Impulse);

    public void ApplyDamage(float damage) {
        if (_health) _health.ApplyDamage(damage);
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.TryGetComponentInParent(out Ragdoll ragdoll) && ragdoll == _main) return;
        
        _main.PlayCollision(other.contacts.Choice().point);
    }
}