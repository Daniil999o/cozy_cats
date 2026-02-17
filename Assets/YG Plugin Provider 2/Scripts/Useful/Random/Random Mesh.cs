using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RandomMesh : MonoBehaviour
{
    [SerializeField] private bool test;

    [Space]
    
    [SerializeField] private Mesh[] randomMeshes;

    #if UNITY_EDITOR
    
    private void OnValidate() {
        if (test) {
            test = false;
            
            ChangeMesh();
        }
    }
    
    #endif

    private void Awake() => ChangeMesh();
    
    private void ChangeMesh() => GetComponent<MeshFilter>().mesh = randomMeshes.Choice();
}