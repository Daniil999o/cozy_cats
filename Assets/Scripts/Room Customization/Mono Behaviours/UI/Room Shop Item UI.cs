using UnityEngine;

public class RoomShopItemUI : UIModularListElement
{
    [Space]

    [SerializeField] private GameObject cantButton;
    [SerializeField] private GameObject buyButton;

    private RoomShopItemListElementInfo _info;
    
    public override void SetInfo(UIModularListElementInfo info) {
        base.SetInfo(info);

        if (info is not RoomShopItemListElementInfo newInfo)
            return;
        
        bool enoughMoney = RoomShop.Coins >= newInfo.MyShopItem.price;
        
        cantButton.SetActive(!enoughMoney);
        buyButton.SetActive(enoughMoney);

        _info = newInfo;
    }

    public void Buy() {
        if (RoomShop.Coins < _info.MyShopItem.price) return;
        
        RoomShop.Coins -= _info.MyShopItem.price;
        
        RoomObjectsLoader.Instance.SpawnObject(_info.MyShopItem);
        
        _info.ModularListWindow.UpdateList();
    }
}

public class RoomShopItemListElementInfo : UIModularListElementInfo
{
    public ShopItemSO MyShopItem { get; set; }
    public ShopItemType MyItemType { get; set; }
}