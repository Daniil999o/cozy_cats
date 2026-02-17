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
    
    public void ShotClip(params AudioClip[] clips) {
        mainSource.PlayOneShot(clips.Choice());
    }
}