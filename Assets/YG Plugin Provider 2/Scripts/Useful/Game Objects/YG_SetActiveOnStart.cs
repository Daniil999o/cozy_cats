using UnityEngine;

public class YG_SetActiveOnStart : MonoBehaviour
{
    [SerializeField] private bool active;
    
    private void Start() {
        gameObject.SetActive(active);
    }
}