using UnityEngine;

[RequireComponent(typeof(SimpleAI))]
public class AI_Controller : MonoBehaviour
{
    protected SimpleAI MainAI;

    protected Animator Animator => MainAI.Animator;

    protected Transform _transform;
    
    private void OnEnable() {
        _transform = GetComponent<Transform>();
        MainAI = GetComponent<SimpleAI>();
    }
    
    public virtual void OnDamaged() { }
}