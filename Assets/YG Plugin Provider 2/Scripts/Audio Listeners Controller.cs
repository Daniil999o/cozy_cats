using UnityEngine;

public class AudioListenersController : MonoBehaviour
{
    [SerializeField] private AudioListener mainListener;
    [SerializeField] private AudioListener effectListener;

    private static AudioListenersController _instance;

    private static AudioListenersController Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<AudioListenersController>();

            return _instance;
        }
    }

    public static bool MainListenerEnabled {
        get => Instance.mainListener.enabled;
        set {
            return;
            Instance.mainListener.enabled = value;
            Instance.effectListener.enabled = !value;
        }
    }

    private void Start() {
        MainListenerEnabled = true;
    }
}