using UnityEngine;

public class GameSFX : MonoBehaviour
{
    [SerializeField] private AudioSource mainSource;

    private static GameSFX _instance;

    public static GameSFX Instance {
        get {
            if (!_instance)
                _instance = FindFirstObjectByType<GameSFX>(FindObjectsInactive.Include);

            return _instance;
        }
    }

    private float _delay;
    
    public void ShotClip(AudioClip[] clips, bool ignoreDelay = false, bool randomPitch = true) {
        if (!ignoreDelay && _delay > 0f) return;
        
        if (clips.Length <= 0) return;

        if (!ignoreDelay) _delay = 0.1f;
        
        if (randomPitch)
            mainSource.pitch = Random.Range(0.9f, 1.1f);
        mainSource.PlayOneShot(clips.Choice());
    }

    private void Update() {
        if (_delay > 0f) _delay -= Time.deltaTime;
    }
}