using UnityEngine;

public class DontDestroyOnLoadDestroyer : MonoBehaviour
{
    [SerializeField] private bool destroyOnAwake = true;

    private void Awake() {
        if (!destroyOnAwake) return;
        
        if (global::DontDestroyOnLoad.Instance)
            Destroy(global::DontDestroyOnLoad.Instance.gameObject);
    }
}