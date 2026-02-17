using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Type")]
    public HealthType healthType;
        
    [Space]
    
    [Header("Info")]
    [SerializeField] private bool died;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health = 100f;

    [Space]

    [Header("Body")]
    public Transform hips;
    
    [Space]

    [Header("Look Point")]
    public Transform lookPoint;

    [Space]

    [Header("Connected Rigidbody")]
    [SerializeField] private Rigidbody connectedRigidbody;

    [Space]

    [Header("Destroy When Die")]
    [SerializeField] private GameObject[] destroyWhenDieObjects;

    public Action OnDie;
    public Action OnHealthUpdate;
    public Action<float> OnDamage;

    public bool Died => died;

    public float CurrentHealth {
        get => health;
        private set {
            health = Mathf.Clamp(value, 0f, maxHealth);

            UpdateHealth();
        }
    }

    public float HealthDifference => health / maxHealth;

    public void ApplyDamage(float damage) {
        CurrentHealth -= damage;
        OnDamage?.Invoke(damage);
        onDamageEvent?.Invoke();
    }

    [Space]

    [Header("Events")]
    [SerializeField] private UnityEvent onDamageEvent;
    [SerializeField] private UnityEvent onDieEvent;

    private Ragdoll _ragdoll;

    private void Awake() {
        _ragdoll = GetComponent<Ragdoll>();
        
        CurrentHealth = health;
    }

    public void AddToMaxHealth(float value) {
        maxHealth += value;
        CurrentHealth = maxHealth;
    }

    public void MultiplyHealth(float modifier) {
        maxHealth *= modifier;
        CurrentHealth = maxHealth;
    }

    private void Update() {
        if (connectedRigidbody && !connectedRigidbody.isKinematic)
            Die();
    }

    private void UpdateHealth() {
        OnHealthUpdate?.Invoke();
        
        if (CurrentHealth <= 0f) Die();
    }

    private void Die() {
        if (died) return;

        died = true;

        CurrentHealth = 0f;

        if (_ragdoll) _ragdoll.Active = true;
        
        foreach (var x in destroyWhenDieObjects)
            Destroy(x);
        
        onDieEvent?.Invoke();
        OnDie?.Invoke();
    }
}

public enum HealthType
{
    Enemy,
    Friend,
    Neutral
}