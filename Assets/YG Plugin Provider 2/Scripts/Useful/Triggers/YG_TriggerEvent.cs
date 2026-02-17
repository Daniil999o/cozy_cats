using UnityEngine;
using UnityEngine.Events;

public class YG_TriggerEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent enterEvent;
    [SerializeField] private TriggerTargetEvent enterEventTarget;
    
    [System.Serializable]
    public class TriggerTargetEvent : UnityEvent<TriggerTarget> { }
    
    [Space]
    
    [SerializeField] private bool destroyAfter = true;
    [SerializeField] private bool invokeOnce = true;

    private bool _invoked;
    
    private void OnTriggerEnter(Collider other) {
        if (_invoked) return;
        
        if (other.TryGetComponent(out TriggerTarget triggerTarget) || other.TryGetComponentInParent(out triggerTarget)) {
            if (invokeOnce) _invoked = true;
            
            enterEvent?.Invoke();
            enterEventTarget?.Invoke(triggerTarget);
            
            if (destroyAfter)
                Destroy(gameObject);
        }
    }
}