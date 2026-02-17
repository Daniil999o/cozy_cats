using UnityEngine;

public class LifeTimeObject : MonoBehaviour
{
    [SerializeField] private bool invokeOnStart = true;
    
    [Space]
    
    [SerializeField] private bool hasTimer = true;
    [SerializeField][Tooltip("In seconds")] private float lifeTime = 3f;

    private void Start() {
        if (invokeOnStart) DestroyLifeTime();
    }

    public void DestroyLifeTime() {
        if (hasTimer) {
            DestroyWithTimer(lifeTime);
            return;
        }

        DestroyMe();
    }

    public void DestroyWithTimer(float timer) => Destroy(gameObject, timer);

    public void DestroyMe() => Destroy(gameObject);
}