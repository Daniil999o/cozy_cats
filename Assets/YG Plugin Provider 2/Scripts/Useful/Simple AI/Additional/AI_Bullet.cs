using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AI_Bullet : MonoBehaviour
{
    [SerializeField] private float bulletImpulse = 10f;
    [SerializeField] private float bulletRbImpulse = 100f;

    [Space]

    [SerializeField] private bool addImpulseAtStart;
    [SerializeField] private bool destroyAfterCollision = true;

    private const float BulletLifetime = 2f;
    
    private Rigidbody _rb;
    private float _damage;

    private bool _destroyed;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start() {
        if (addImpulseAtStart) ApplyImpulse(0f);
    }

    public void ApplyImpulse(float damage, float modifier = 1f) {
        _damage = damage;
        _rb.AddRelativeForce(Vector3.forward * bulletImpulse * modifier, ForceMode.Impulse);
        Destroy(gameObject, BulletLifetime);
    }

    private void OnCollisionEnter(Collision other) {
        if (_destroyed) return;

        _destroyed = true;
        
        if (other.gameObject.TryGetComponent(out PlayerInfo playerInfo))
            playerInfo.ApplyDamage(_damage);
        else if (other.gameObject.TryGetComponent(out Rigidbody rb))
            rb.AddForce(_rb.transform.forward * bulletRbImpulse, ForceMode.Impulse);
        
        if (destroyAfterCollision)
            Destroy(gameObject);
    }
}