using UnityEngine;

public class RandomObject : MonoBehaviour
{
    [SerializeField] private bool test;

    [Space]
    
    [SerializeField] private GameObject[] objects;
    
    #if UNITY_EDITOR
    
    private void OnValidate() {
        if (test) {
            test = false;
            
            ChangeObject();
        }
    }
    
    #endif

    private void Awake() => ChangeObject();

    private void ChangeObject() {
        for (int i = 0; i < objects.Length; i++)
            objects[i].SetActive(false);
        
        objects.Choice().SetActive(true);
    }
}