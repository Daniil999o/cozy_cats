using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
public class YG_AnimatorHelper2 : MonoBehaviour
{
    [System.Serializable]
    private class Effector {
        public string name;
        [SerializeField] private YG_Effect effect;
        [SerializeField] private Transform point;

        public void Spawn() {
            Instantiate(effect, point.position, Quaternion.identity);
        }
    }

    [System.Serializable]
    private struct Event {
        public string name;
        public UnityEvent events;
    }

    [Header("General")]
    [SerializeField] private Event[] events;
    [SerializeField] private Event[] adEvents;

    [Space]

    [Header("Effects")]
    [SerializeField] private Effector[] effectors;

    [Space]

    [Header("Audio")]
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private SoundClip[] sounds;

    private UnityEvent _adEvent;

    [System.Serializable]
    private struct SoundClip
    {
        public string name;
        public AudioClip[] clips;
    }

    public void PlayEvent2(string name) {
        if (events.Count(x => x.name == name) > 0) {
            events.Where(x => x.name == name).ToList().ForEach(x => {
                    try {
                        if (x.events != null) x.events.Invoke();
                    } catch (System.Exception exception) {
                        Debug.LogWarning(exception);
                    }
                }
            );
        }
    }

    public void PlayAdEvent(string name) {
        if (adEvents.Count(x => x.name == name) > 0) {
            adEvents.Where(x => x.name == name).ToList().ForEach(x => {
                    try {
                        if (x.events != null) {
                            _adEvent = x.events;
                            YG_Provider.ShowAndDoAction(_PlayAdEvent);
                        }
                    } catch (System.Exception exception) {
                        Debug.LogWarning(exception);
                    }
                }
            );
        }
    }

    public void PlaySound(string name) {
        if (sounds.Count(x => x.name == name) > 0) {
            sounds.Where(x => x.name == name).ToList().ForEach(x => {
                    soundSource.PlayOneShot(x.clips[Random.Range(0, x.clips.Length)]);
                }
            );
        }
    }

    public void PlayEvent(int index) {
        events[index].events.Invoke();
    }

    public void PlayAdEvent(int index) {
        _adEvent = adEvents[index].events;
        YG_Provider.ShowAndDoAction(_PlayAdEvent);
    }

    private void _PlayAdEvent() {
        _adEvent?.Invoke();
    }

    public void PlayEffect(string name) {
        if (effectors.Count(x => x.name == name) > 0) {
            effectors.Where(x => x.name == name).ToList().ForEach(x => x.Spawn());
        }
    }

    public void PlayEffect(int index) {
        effectors[index].Spawn();
    }
    
    public void DestroyMe() => Destroy(gameObject);

    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
    public void LoadSceneAsync(string sceneName) => SceneManager.LoadSceneAsync(sceneName);

    public void PlayAudioClip2(AudioClip clip) {
        if (!soundSource) return;
        
        soundSource.PlayOneShot(clip);
    }
}
