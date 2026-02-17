using UnityEngine;

public class AI_HitPoint : MonoBehaviour
{
    [SerializeField] private float hitRadius;
    [SerializeField] private LayerMask hitLayers;

    private AI_MeleeWeapon _weapon;

    private void Awake() {
        _weapon = GetComponentInParent<AI_MeleeWeapon>();
    }
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        
        Gizmos.DrawWireSphere(transform.position, hitRadius * transform.lossyScale.x);
    }

    public void CheckCollisions() {
        Collider[] hitColliders =
            Physics.OverlapSphere(transform.position, hitRadius * transform.lossyScale.x, hitLayers);

        foreach (var x in hitColliders) {
            if (x.TryGetComponent(out AI_Damageable damageable))
                damageable.ApplyDamage(_weapon.data.damage);
            
            Debug.Log($"{name} Hit: {x.name}");
        }
    }
}