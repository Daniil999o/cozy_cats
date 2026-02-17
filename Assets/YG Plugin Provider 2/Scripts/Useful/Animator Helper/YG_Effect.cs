using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class YG_Effect : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float destroyTime = 3.0f;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float minPitch = 1.0f;

    private void Awake()
    {
        if (clips.Length > 0) {
            AudioSource source = GetComponent<AudioSource>();

            source.pitch = minPitch * Random.Range(0.9f, 1.1f);
            source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }

        Destroy(gameObject, destroyTime);
    }
}
