using UnityEngine;
using UnityEngine.Audio;

public class AudioSourceSettings : MonoBehaviour
{
    public static AudioMixerGroup FxMixer;
    public static AudioMixerGroup MusicMixer;

    [SerializeField] private AudioMixerGroup fxMixer;
    [SerializeField] private AudioMixerGroup musicMixer;

    
    private static AudioSourceSettings _instance;

    private static AudioSourceSettings Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<AudioSourceSettings>();

            return _instance;
        }
    }
    
    private static AudioSource _mainSource;

    public static AudioSource MainSource {
        get {
            if (!_mainSource) {
                _mainSource = new GameObject("MAIN SOURCE").AddComponent<AudioSource>();
                
                if (FxMixer)
                    _mainSource.outputAudioMixerGroup = FxMixer;
                
                if (Instance)
                    _mainSource.transform.SetParent(Instance.transform);
            }

            return _mainSource;
        }
    }
    
    private void Awake() {
        FxMixer = fxMixer;
        MusicMixer = musicMixer;
    }
}