using UnityEngine;

public class RoomObjectUI : MonoBehaviour
{
    [SerializeField] private bool autoSet;

    [Space]

    [SerializeField] private bool alwaysLookAtCamera;

    [Space]
    
    [SerializeField] private RoomObjectMaterialChanger materialChanger;
    [SerializeField] private RoomObjectSkinChanger skinChanger;
    [SerializeField] private RoomObjectTransformation transformation;
    [SerializeField] private RoomObjectDestroyable destroyable;

    [Space]

    [SerializeField] private GameObject rootHud;

    [Space]

    [SerializeField] private GameObject transformButton;
    [SerializeField] private GameObject rotateButton;
    [SerializeField] private GameObject editButton;
    [SerializeField] private GameObject changeSkinButton;
    [SerializeField] private GameObject deleteButton;
    
    [Space]
    
    [SerializeField] private GameObject actionsButtons;
    [SerializeField] private GameObject discardButtons;

    private Transform _myCamera;
    private Transform _transform;

    public bool DiscardActive {
        set {
            actionsButtons.SetActive(!value);
            discardButtons.SetActive(value);
        }
    }

    public bool HUDVisible {
        get => rootHud.activeSelf;
        set => rootHud.SetActive(value);
    }

    private void Start() {
        UpdateButtons();
        HUDVisible = false;

        _myCamera = RoomUIHandler.Instance.Camera.transform;
        _transform = GetComponent<Transform>();
    }

    private void LateUpdate() {
        if (!alwaysLookAtCamera) return;
        
        _transform.LookAt(_myCamera);
    }

    private void UpdateButtons() {
        DiscardActive = false;
        
        transformButton.SetActive(transformation);
        rotateButton.SetActive(transformation);
        editButton.SetActive(materialChanger);
        changeSkinButton.SetActive(skinChanger);
        deleteButton.SetActive(destroyable);
    }
    
    public void ChangeMaterial() {
        DiscardActive = true;
        RoomUIHandler.Instance.ChangeMaterialRequest(materialChanger, this);
    }
    
    public void ChangeSkin() {
        DiscardActive = true;
        RoomUIHandler.Instance.ChangeSkinRequest(skinChanger, this);
    }

    public void MoveObject() {
        DiscardActive = true;
        RoomUIHandler.Instance.MoveObjectRequest(transformation, false, this);
    }
    
    public void RotateObject() {
        DiscardActive = true;
        RoomUIHandler.Instance.MoveObjectRequest(transformation, true, this);
    }

    public void DestroyObject() {
        RoomUIHandler.Instance.DestroyObjectRequest(destroyable);
    }

    public void DiscardEvents() => RoomUIHandler.Instance.DiscardEvents();
    

#if UNITY_EDITOR

    private void OnValidate() {
        if (autoSet) {
            materialChanger = GetComponentInParent<RoomObjectMaterialChanger>();
            skinChanger = GetComponentInParent<RoomObjectSkinChanger>();
            transformation = GetComponentInParent<RoomObjectTransformation>();
            destroyable = GetComponentInParent<RoomObjectDestroyable>();
            
            rootHud = transform.GetChild(0).gameObject;
        }

        autoSet = false;
    }
    
#endif
}