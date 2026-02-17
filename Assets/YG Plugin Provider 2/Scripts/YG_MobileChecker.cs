using UnityEngine;
using UnityEngine.Events;

public class YG_MobileChecker : MonoBehaviour
{
    [SerializeField] private InvokeMethod invokeMethod = InvokeMethod.OnEnable;

    [Space]

    [Header("States")]
    [SerializeField] private EnabledState stateWhenMobile = EnabledState.Active;
    [SerializeField] private EnabledState stateWhenDesktop = EnabledState.Inactive;
    
    [Space]
    
    [Header("Events")]
    [SerializeField] private UnityEvent eventWhenMobile;
    [SerializeField] private UnityEvent eventWhenDesktop;

    private bool _firstFrame;

    private enum InvokeMethod {
        Awake,
        Start,
        OnEnable,
        FirstFrame
    }

    private enum EnabledState {
        DontChange,
        Active,
        Inactive,
        Destroy,
        DestroyImmediate
    }

    private void Awake() {
        if (invokeMethod != InvokeMethod.Awake) return;

        InvokeEvent();
    }

    private void Start() {
        if (invokeMethod != InvokeMethod.Start) return;

        InvokeEvent();
    }

    private void OnEnable() {
        if (invokeMethod != InvokeMethod.OnEnable) return;

        InvokeEvent();
    }

    private void Update() {
        if (_firstFrame) return;

        _firstFrame = true;
        
        if (invokeMethod != InvokeMethod.FirstFrame) return;

        InvokeEvent();
    }

    private void InvokeEvent() {
        bool isMobile = YG_Provider.IsMobile;
        
        (isMobile ? eventWhenMobile : eventWhenDesktop)?.Invoke();

        switch (isMobile ? stateWhenMobile : stateWhenDesktop) {
            case EnabledState.Active:
                gameObject.SetActive(true);
                break;
            case EnabledState.Inactive:
                gameObject.SetActive(false);
                break;
            case EnabledState.Destroy:
                Destroy(gameObject);
                break;
            case EnabledState.DestroyImmediate:
                DestroyImmediate(gameObject);
                break;
        }
    }

    public void SetScale(float value) => transform.localScale = new Vector3(value, value, value);
}