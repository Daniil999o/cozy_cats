using UnityEngine;
using UnityEngine.UI;

public class RoomObjectUI : MonoBehaviour
{
    [SerializeField] private bool autoSet;

    [Space]
    
    [SerializeField] private RoomObjectMaterialChanger materialChanger;

    [Space]

    [SerializeField] private GameObject rootHud;

    [Space]

    [SerializeField] private Button transformButton;
    [SerializeField] private Button rotateButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    public bool HUDVisible {
        get => rootHud.activeSelf;
        set => rootHud.SetActive(value);
    }

    private void Start() {
        HUDVisible = false;
    }
    
    public void ChangeMaterial() {
        RoomUIHandler.Instance.ChangeMaterialRequest(materialChanger);
    }
    

#if UNITY_EDITOR

    private void OnValidate() {
        if (autoSet) {
            materialChanger = GetComponentInParent<RoomObjectMaterialChanger>();
            rootHud = transform.GetChild(0).gameObject;
        }

        autoSet = false;
    }
    
#endif
}