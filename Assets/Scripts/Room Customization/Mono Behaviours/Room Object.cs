using UnityEngine;
using UnityEngine.EventSystems;

public class RoomObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField] private bool Validate { get; set; }
    [field: SerializeField] public string ElementId { get; set; }

    [Space]

    [SerializeField] private bool canHighlight = false;
    [SerializeField] private GameObject[] highlightObjects;
    [SerializeField] private bool isFurniture;

    private bool _dontHighlight;

    private void Start() {
        if (highlightObjects.Length <= 0)
            highlightObjects = new[] {
                gameObject
            };
    }
    
    public void OnPointerEnter(PointerEventData eventData) {
        if (!canHighlight || _dontHighlight) return;

        if (!RoomUIHandler.Instance.HUDVisible) return;
        
        SetLayer(RoomUIHandler.Instance.SelectedLayer);
    }
    public void OnPointerExit(PointerEventData eventData) {
        if (!canHighlight || _dontHighlight) return;
        
        if (!RoomUIHandler.Instance.HUDVisible) return;

        SetLayer(isFurniture
            ? RoomUIHandler.Instance.ObjectLayer
            : RoomUIHandler.Instance.RoomLayer
        );
    }

    public void SetDrag(bool value) => _dontHighlight = value;

    public void SetLayer(int layer) {
        for (int i = 0; i < highlightObjects.Length; i++)
            highlightObjects[i].layer = layer;
    }
    
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