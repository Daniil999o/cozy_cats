using UnityEngine;
using AutoLocalization;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class RoomUIHandler : MonoBehaviour
{
    [SerializeField] private LayerMask raycastLayerMask;
    [field: SerializeField] public int RigidbodyLayer { get; set; } = 9;
    [field: SerializeField] public int ObjectLayer { get; set; } = 16;
    [field: SerializeField] public int RoomLayer { get; set; } = 15;
    [field: SerializeField] public int SelectedLayer { get; set; } = 12;
    
    [field: Space]
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraDragger dragger;
    
    public LayerMask RaycastLayerMask => raycastLayerMask;
    public Camera Camera => mainCamera;
    public CameraDragger CameraDragger => dragger;
    
    [Space]
    
    [SerializeField] private UIModularListWindow customizeMaterialsList;
    [SerializeField] private UIModularListWindow customizeSkinsList;

    private static RoomUIHandler _instance;

    public static RoomUIHandler Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<RoomUIHandler>(FindObjectsInactive.Include);

            return _instance;
        }
    }

    private RoomObjectMaterialChanger _oldRoomObjectMaterial;
    private RoomObjectSkinChanger _oldRoomObjectSkin;
    private RoomObjectTransformation _transformation;

    private RoomObjectUI _oldObjectUI;

    private RoomObjectUI[] UIElements =>
        FindObjectsByType<RoomObjectUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

    private IUIModalWindow[] ModalWindows => GetComponentsInChildren<IUIModalWindow>(true);

    public bool HUDVisible { get; private set; }

    private void OnEnable() {
        foreach (var x in ModalWindows) x.OnClose += DiscardEvents;
    }

    private void OnDisable() {
        foreach (var x in ModalWindows) x.OnClose -= DiscardEvents;
    }

    public void ShowUI() {
        HUDVisible = true;
        foreach (var x in UIElements) x.HUDVisible = true;
    }

    public void DiscardEvents() {
        if (_oldObjectUI) {
            _oldObjectUI.DiscardActive = false;
            
            _oldObjectUI = null;
        }
        
        customizeMaterialsList.Close();
        customizeSkinsList.Close();

        if (_transformation) {
            _transformation.Move = false;
            _transformation.Rotate = false;

            _transformation = null;
        }
    }
    
    public void HideUI() {
        HUDVisible = false;
        
        DiscardEvents();
        
        foreach (var x in UIElements) x.HUDVisible = false;
        foreach (var x in ModalWindows) x.Close();
    }

    public void DestroyObjectRequest(RoomObjectDestroyable destroyable) {
        Destroy(destroyable.gameObject);
    }
    
    public void ChangeMaterialRequest(RoomObjectMaterialChanger roomObject, RoomObjectUI roomObjectUI) {
        DiscardEvents();

        _oldObjectUI = roomObjectUI;
        
        if (_oldRoomObjectMaterial == roomObject && customizeMaterialsList.Active) {
            customizeMaterialsList.Close();
            _oldRoomObjectMaterial = null;
            return;
        }

        _oldRoomObjectMaterial = roomObject;
        
        List<RoomMaterialListElementInfo> infos = new List<RoomMaterialListElementInfo>();

        foreach (var x in roomObject.FirstMaterial.All) {
            infos.Add(new RoomMaterialListElementInfo {
                Price = x.price,
                Title = AutoLocalizationHandler.GetLocalizedText(x.id),
                Icon = x.Preview,
                MyMaterial = x,
                MyRoomObject = roomObject,
                ModularListWindow = customizeMaterialsList
            });
        }
        
        customizeMaterialsList.Show(infos.ToArray());
    }

    public void ChangeSkinRequest(RoomObjectSkinChanger roomObject, RoomObjectUI roomObjectUI) {
        DiscardEvents();

        _oldObjectUI = roomObjectUI;
        
        if (_oldRoomObjectSkin == roomObject && customizeSkinsList.Active) {
            customizeSkinsList.Close();
            _oldRoomObjectSkin = null;
            return;
        }

        _oldRoomObjectSkin = roomObject;
        
        List<RoomSkinListElementInfo> infos = new List<RoomSkinListElementInfo>();

        foreach (var x in roomObject.All) {
            infos.Add(new RoomSkinListElementInfo {
                Price = x.price,
                Title = AutoLocalizationHandler.GetLocalizedText(x.id),
                Icon = x.Preview,
                MySkin = x,
                MyRoomObject = roomObject,
                ModularListWindow = customizeSkinsList
            });
        }
        
        customizeSkinsList.Show(infos.ToArray());
    }

    
    public void MoveObjectRequest(RoomObjectTransformation transformation, bool rotate, RoomObjectUI roomObjectUI) {
        DiscardEvents();
        
        _oldObjectUI = roomObjectUI;
        
        _transformation = transformation;
        
        transformation.Move = !rotate;
        transformation.Rotate = rotate;
    }
    
    #if UNITY_EDITOR

    private void Update() {
        if (Keyboard.current.iKey.wasPressedThisFrame) RoomShop.Coins += 10;
        if (Keyboard.current.oKey.wasPressedThisFrame) RoomShop.Gems += 10;
        if (Keyboard.current.pKey.wasPressedThisFrame) RoomShop.Megas += 10;
        
        if (Keyboard.current.mKey.wasPressedThisFrame) RoomShop.ResetAll();
    }
    
    #endif
}