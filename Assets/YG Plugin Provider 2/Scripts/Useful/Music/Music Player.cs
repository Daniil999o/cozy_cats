using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [Header("Tracks List")]
    [SerializeField] private Track[] musicTracks;

    [Space]

    [Header("Play Options")]
    [SerializeField][Range(0f, 1f)] private float globalVolume = 1f;
    [SerializeField] private bool dontRepeat = true;

    [System.Serializable]
    private class Track
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    private int _oldTrackIndex = -1;
    private AudioSource _source;

    private float _timer;

    private void Awake() {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
        
        _source.Stop();
    }

    private void Update() {
        if (_timer <= 0f)
            PlayRandomClip();
        else
            _timer -= Time.deltaTime;
    }

    private void PlayRandomClip() {
        int myTrack;
        
        do myTrack = Random.Range(0, musicTracks.Length);
        while (dontRepeat && myTrack == _oldTrackIndex);

        _source.volume = musicTracks[myTrack].volume * globalVolume;
        _source.clip = musicTracks[myTrack].clip;
        _source.Play();

        _oldTrackIndex = myTrack;
        _timer = _source.clip.length;
    }
}