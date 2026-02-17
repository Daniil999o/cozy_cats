using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioTimerPlayer : MonoBehaviour
{
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private AudioClip[] myClips;
    
    [Space]
    
    [SerializeField] private Vector2 minMaxTimer = new Vector2(30f, 120f);
    [SerializeField] private Vector2 minMaxPitch = new Vector2(0.9f, 1.15f);
    
    private AudioSource _source;

    private float _timer;
    
    private void Awake() {
        _source = GetComponent<AudioSource>();

        if (!playOnStart) ResetTimer();
    }

    private void Update() {
        if (_timer > 0f) {
            _timer -= Time.deltaTime;
        } else {
            ResetTimer();
            
            _source.pitch = Random.Range(minMaxPitch.x, minMaxPitch.y);
            _source.clip = myClips[Random.Range(0, myClips.Length)];
            _source.Play();
        }
    }

    private void ResetTimer() {
        _timer = Random.Range(minMaxTimer.x, minMaxTimer.y);
    }
}