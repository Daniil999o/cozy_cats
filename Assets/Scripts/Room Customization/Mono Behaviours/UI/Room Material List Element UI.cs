using UnityEngine;

public class RoomMaterialListElementUI : UIModularListElement
{
    [Space]

    [SerializeField] private GameObject cantButton;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private GameObject selectButton;
    [SerializeField] private GameObject selectedButton;

    private RoomMaterialListElementInfo _info;
    
    public override void SetInfo(UIModularListElementInfo info) {
        base.SetInfo(info);

        if (info is not RoomMaterialListElementInfo newInfo)
            return;

        CustomMaterialSO myMat = newInfo.MyMaterial;
        
        bool bought = myMat.price <= 0 || RoomShop.HasCustomMaterial(myMat.id);
        bool selected = myMat.id == RoomShop.GetObjectMaterial(newInfo.MyRoomObject.ElementId);
        bool enoughMoney = RoomShop.Coins >= myMat.price;
        
        cantButton.SetActive(!bought && !enoughMoney);
        buyButton.SetActive(!bought && enoughMoney);
        
        selectButton.SetActive(bought && !selected);
        selectedButton.SetActive(bought && selected);

        _info = newInfo;
    }

    public void Buy() {
        if (RoomShop.Coins < _info.MyMaterial.price) return;
        
        RoomShop.Coins -= _info.MyMaterial.price;
        RoomShop.BuyCustomMaterial(_info.MyMaterial.id);

        Select();
    }

    public void Select() {
        RoomShop.SetObjectMaterial(_info.MyRoomObject.ElementId, _info.MyMaterial.id);
        _info.ModularListWindow.UpdateList();
        _info.MyRoomObject.UpdateMaterial();
    }
}

public class RoomMaterialListElementInfo : UIModularListElementInfo
{
    public RoomObjectMaterialChanger MyRoomObject { get; set; }
    public CustomMaterialSO MyMaterial { get; set; }
    
    public virtual UIModularListWindow ModularListWindow { get; set; }
}