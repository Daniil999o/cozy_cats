using UnityEngine;

public class AI_Damageable : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health = 100f;

    public float Health {
        get => Mathf.Clamp(health, 0f, maxHealth);
        private set {
            health = Mathf.Clamp(value, 0f, maxHealth);
            
            OnHealthUpdated();
            
            if (health <= 0f) OnDie();
        }
    }

    public float HealthDifference => health / maxHealth;

    private void Awake() {
        Health = maxHealth;
    }

    public virtual void ApplyDamage(float damage) {
        Health -= damage;
    }
    
    protected virtual void OnHealthUpdated() { }
    
    protected virtual void OnDie() { }
}