using UnityEngine;
using System.Collections;

public class CatBehaviour : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private int _sleepHash;

    private IEnumerator _sleepCoroutine;

    private void Awake() {
        _sleepHash = Animator.StringToHash("Sleep");
    }

    private void Start() {
        animator.enabled = true;
        animator.SetBool(_sleepHash, true);
    }

    public void GoToSleep() {
        if (_sleepCoroutine != null) return;

        _sleepCoroutine = IE_GoToSleep();
        StartCoroutine( _sleepCoroutine);
    }

    private IEnumerator IE_GoToSleep() {
        yield return new WaitForSeconds(Random.Range(5f, 20f));
        
        animator.SetBool(_sleepHash, true);
        
        _sleepCoroutine = null;
    }

    public void WakeUp() {
        if (_sleepCoroutine != null) {
            StopCoroutine(_sleepCoroutine);
            _sleepCoroutine = null;
        }
        
        animator.SetBool(_sleepHash, false);
    }
}