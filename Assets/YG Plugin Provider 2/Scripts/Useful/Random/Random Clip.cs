using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomClip : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    
    private AudioSource _source;

    private void Awake() {
        _source = GetComponent<AudioSource>();
        
        _source.playOnAwake = false;
        _source.Stop();

        _source.clip = clips.Choice();
        _source.Play();
    }
}