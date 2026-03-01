using UnityEngine;
using AutoLocalization;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class RoomUIHandler : MonoBehaviour
{
    [SerializeField] private UIModularListWindow customizeMaterialsList;

    private static RoomUIHandler _instance;

    public static RoomUIHandler Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<RoomUIHandler>(FindObjectsInactive.Include);

            return _instance;
        }
    }

    private RoomObjectMaterialChanger _oldRoomObject;

    private RoomObjectUI[] UIElements =>
        FindObjectsByType<RoomObjectUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

    private IUIModalWindow[] ModalWindows => GetComponentsInChildren<IUIModalWindow>(true);

    public void ShowUI() {
        foreach (var x in UIElements) x.HUDVisible = true;
    }
    
    public void HideUI() {
        customizeMaterialsList.Close();
        
        foreach (var x in UIElements) x.HUDVisible = false;
        foreach (var x in ModalWindows) x.Close();
    }

    public void ChangeMaterialRequest(RoomObjectMaterialChanger roomObject) {
        if (_oldRoomObject == roomObject && customizeMaterialsList.Active) {
            customizeMaterialsList.Close();
            _oldRoomObject = null;
            return;
        }

        _oldRoomObject = roomObject;
        
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
    
    #if UNITY_EDITOR

    private void Update() {
        if (Keyboard.current.iKey.wasPressedThisFrame) RoomShop.Coins += 10;
        if (Keyboard.current.oKey.wasPressedThisFrame) RoomShop.Gems += 10;
        if (Keyboard.current.pKey.wasPressedThisFrame) RoomShop.Megas += 10;
        
        if (Keyboard.current.mKey.wasPressedThisFrame) RoomShop.ResetAll();
    }
    
    #endif
}