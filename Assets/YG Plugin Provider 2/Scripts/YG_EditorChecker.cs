using UnityEngine;

public class YG_EditorChecker : MonoBehaviour
{
    [SerializeField] private InvokeMethod invokeMethod = InvokeMethod.OnEnable;

    [Space]

    [Header("States")]
    [SerializeField] private EnabledState stateWhenEditor = EnabledState.Active;
    [SerializeField] private EnabledState stateWhenBuild = EnabledState.Inactive;

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
        switch (YG_Provider.IsEditor ? stateWhenEditor : stateWhenBuild) {
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
}