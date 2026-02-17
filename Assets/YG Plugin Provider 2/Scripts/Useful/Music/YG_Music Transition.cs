using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class YG_MusicTransition : MonoBehaviour
{
    [SerializeField] private Transition[] myTransitions;

    [System.Serializable]
    private class Transition
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        
        [Space]
        
        public float inSpeed = 2f;
        public float outSpeed = 2f;
    }
    
    private AudioSource _source;
    
    private bool _in;
    private bool _end;

    private int _currentTransition;

    private bool _destroyOnEnd;

    private void Awake() {
        _source = GetComponent<AudioSource>();
        _source.volume = 0f;

        SetTransition(0);
    }

    private void SetTransition(int index) {
        _currentTransition = index;

        if (!myTransitions.InLength(_currentTransition)) _currentTransition = 0;
        
        _source.clip = myTransitions[_currentTransition].clip;
        _source.Play();
        _in = true;
    }
    
    private void Update() {
        if (_end) return;

        Transition myTransition = myTransitions[_currentTransition];

        if (_in) {
            _source.volume += myTransition.inSpeed * Time.deltaTime;

            if (_source.volume >= myTransition.volume) {
                _end = true;
                _source.volume = myTransition.volume;
            }
        } else {
            _source.volume -= myTransition.outSpeed * Time.deltaTime;

            if (_source.volume <= 0f) {
                _end = true;
                _source.volume = 0f;
                
                if (_destroyOnEnd) Destroy(gameObject);
            }
        }
    }

    public void NextTransition() {
        YG_MusicTransition newTransition =
            Instantiate(this, transform.position, transform.rotation, transform.parent);

        newTransition.SetTransition(_currentTransition + 1);
        
        _in = false;
        _end = false;
        _destroyOnEnd = true;
    }
}