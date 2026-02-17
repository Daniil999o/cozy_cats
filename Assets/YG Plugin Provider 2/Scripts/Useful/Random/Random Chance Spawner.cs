using UnityEngine;

public class RandomChanceSpawner : MonoBehaviour
{
    [Range(0f, 1f)] [SerializeField] private float spawnChance = 1f;
    [SerializeField] private bool destroyAfter = true;
    
    [Space]

    [SerializeField] private Transform[] prefabs;

    private void Start() {
        Spawn();
        
        if (destroyAfter) Destroy(gameObject);
    }
    
    private void Spawn() {
        if (Random2.Chance(spawnChance))
            Instantiate(prefabs.Choice(), transform.position, Quaternion.identity);
    }
}