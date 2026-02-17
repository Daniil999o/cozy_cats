using UnityEngine;

public class EnableOnStart : MonoBehaviour
{
    [SerializeField] private Vector2 minMaxEnableDelay = new Vector2(1f, 1f);
    [SerializeField] private GameObject[] enableObjects;

    private void Awake() {
        foreach (var x in enableObjects) x.SetActive(false);
    }
    
    private void Start() {
        Invoke(nameof(EnableObjects), Random.Range(minMaxEnableDelay.x, minMaxEnableDelay.y));
    }

    private void EnableObjects() {
        foreach (var x in enableObjects)
            x.SetActive(true);
    }
}