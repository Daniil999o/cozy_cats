using UnityEngine;
using UnityEngine.EventSystems;

public class RoomObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [field: SerializeField] private bool Validate { get; set; }
    [field: SerializeField] public string ElementId { get; set; }

    [field: Space]

    [field: SerializeField] public ShopItemSO ShopItemSO { get; private set; }
    [SerializeField] private bool saveObject;

    [Space]

    [SerializeField] private bool canHighlight = false;
    [SerializeField] private GameObject[] highlightObjects;
    [SerializeField] private bool isFurniture;
    
    private RoomObjectUI _roomObjectUI;

    private bool _dontHighlight;

    public void SaveObject() {
        if (!saveObject) return;

        RoomShop.SaveObjectData(ElementId, this);
    }

    private void Awake() {
        _roomObjectUI = GetComponentInChildren<RoomObjectUI>(true);
    }

    private void Start() {
        if (highlightObjects.Length <= 0)
            highlightObjects = new[] {
                gameObject
            };
    }

    public void Initialize() {
        if (TryGetComponent(out RoomObjectMaterialChanger materialChanger))
            materialChanger.UpdateMaterial();
        if (TryGetComponent(out RoomObjectSkinChanger skinChanger))
            skinChanger.UpdateSkin();
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
    
    public void OnPointerClick(PointerEventData eventData) {
        if (!canHighlight || _dontHighlight) return;

        if (!RoomUIHandler.Instance.HUDVisible) return;
        
        if (!_roomObjectUI) return;
        
        if (RoomUIHandler.Instance.HUDOnlyObject == _roomObjectUI) {
            RoomUIHandler.Instance.ShowUI();
        } else {
            RoomUIHandler.Instance.ShowHUDOnlyFor(_roomObjectUI);
        }
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