using UnityEngine;

public class RoomSkinListElementUI : UIModularListElement
{
    [Space]

    [SerializeField] private GameObject cantButton;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private GameObject selectButton;
    [SerializeField] private GameObject selectedButton;

    private RoomSkinListElementInfo _info;
    
    public override void SetInfo(UIModularListElementInfo info) {
        base.SetInfo(info);

        if (info is not RoomSkinListElementInfo newInfo)
            return;

        CustomSkinSO mySkin = newInfo.MySkin;
        
        bool bought = mySkin.price <= 0 || RoomShop.HasCustomSkin(mySkin.id);
        bool selected = mySkin.id == RoomShop.GetObjectSkin(newInfo.MyRoomObject.ElementId);
        bool enoughMoney = RoomShop.Coins >= mySkin.price;
        
        cantButton.SetActive(!bought && !enoughMoney);
        buyButton.SetActive(!bought && enoughMoney);
        
        selectButton.SetActive(bought && !selected);
        selectedButton.SetActive(bought && selected);

        _info = newInfo;
    }

    public void Buy() {
        if (RoomShop.Coins < _info.MySkin.price) return;
        
        RoomShop.Coins -= _info.MySkin.price;
        RoomShop.BuyCustomSkin(_info.MySkin.id);

        Select();
    }

    public void Select() {
        RoomShop.SetObjectSkin(_info.MyRoomObject.ElementId, _info.MySkin.id);
        _info.ModularListWindow.UpdateList();
        _info.MyRoomObject.UpdateSkin();
    }
}

public class RoomSkinListElementInfo : UIModularListElementInfo
{
    public RoomObjectSkinChanger MyRoomObject { get; set; }
    public CustomSkinSO MySkin { get; set; }
    
    public virtual UIModularListWindow ModularListWindow { get; set; }
}