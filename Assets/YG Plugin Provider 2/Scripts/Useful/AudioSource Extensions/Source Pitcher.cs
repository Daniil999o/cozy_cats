using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SourcePitcher : MonoBehaviour
{
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.5f;

    [Space]

    [SerializeField] private float valueModifier = 1f;
    [SerializeField] private float maxValue = 5f;

    private AudioSource _source;

    private void Awake() {
        _source = GetComponent<AudioSource>();
    }
    
    public void UpdatePitch(float value) {
        _source.pitch = Mathf.Clamp(value / maxValue * valueModifier, minPitch, maxPitch);
    }
}