using UnityEngine;

public class RoomObject : MonoBehaviour
{
    [field: SerializeField] private bool Validate { get; set; }
    [field: SerializeField] public string ElementId { get; set; }
    
#if UNITY_EDITOR

    private void OnValidate() {
        if (Validate)
            ElementId = CheckForPersonality(gameObject.name);

        Validate = false;
        return;

        string CheckForPersonality(string id) {
            bool has;

            do {
                has = false;
                
                foreach (var x in FindObjectsByType<RoomObject>(FindObjectsInactive.Include, FindObjectsSortMode.None
                         )) {
                    if (x.ElementId != id) continue;

                    has = true;
                    id += "_1";
                }
            } while (has);

            return id;
        }
    }
    
#endif
}