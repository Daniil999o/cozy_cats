using UnityEngine;
using DictionaryExtension;

public class AI_VoiceController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool stopOnPlayNew = true;
    [SerializeField][Range(0.5f, 1.5f)] private float pitch = 1f;
    [SerializeField] private AudioSource source;
    
    [Space]
    
    [Header("Types")]
    [SerializeField] private SerializedDictionary<string, AudioClip[]> voiceClips;

    [Space]

    [Header("Idle Voice")]
    [SerializeField] private bool useIdleVoice = true;
    [SerializeField] private string idleVoiceType = "Idle";
    [SerializeField] private Vector2 idleVoiceTimer = new Vector2(3f, 15f);

    private float _idleTimer;

    private void Update() {
        if (useIdleVoice) {
            if (_idleTimer > 0) {
                _idleTimer -= Time.deltaTime;
            } else {
                _idleTimer = Random.Range(idleVoiceTimer.x, idleVoiceTimer.y);
                PlayVoice(idleVoiceType);
            }
        }
    }

    public void PlayVoice(string type) {
        if (!voiceClips.HasKey(type)) return;
        
        PlayClip(voiceClips.GetKey(type).Choice());
    }

    private void PlayClip(AudioClip clip) {
        source.pitch = pitch;
        
        if (stopOnPlayNew)
            source.Stop();
        
        source.PlayOneShot(clip);
    }
}