using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class YG_VolumeIn : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] private float needVolume = 1f;
    [SerializeField] private float inSpeed = 2f;

    private AudioSource _source;
    
    private bool _end;

    private void Awake() {
        _source = GetComponent<AudioSource>();
        _source.volume = 0f;
    }
    
    private void Update() {
        if (_end) return;
        
        _source.volume += inSpeed * Time.deltaTime;

        if (_source.volume >= needVolume) {
            _end = true;
            _source.volume = needVolume;
            
            Destroy(this);
        }
    }
}