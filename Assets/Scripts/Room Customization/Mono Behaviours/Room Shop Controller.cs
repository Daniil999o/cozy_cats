using UnityEngine;
using System.Collections.Generic;

public class RoomShopController : MonoBehaviour
{
    [SerializeField] private UIModularListWindow modularListWindow;

    private readonly Dictionary<ShopItemType, ShopItemsSO> _shopItems = new Dictionary<ShopItemType, ShopItemsSO>();

    private void Awake() {
        foreach (var x in RoomObjectsLoader.Instance.ShopItems)
            _shopItems.Add(x.itemsType, x);
    }

    public void UpdateList(int itemsType) {
        ShopItemType myType = (ShopItemType)itemsType;
        
        if (!_shopItems.TryGetValue(myType, out ShopItemsSO shopItemsSO)) return;
        
        if (!shopItemsSO) return;

        List<RoomShopItemListElementInfo> myInfos = new List<RoomShopItemListElementInfo>();

        foreach (var x in shopItemsSO.All) {
            myInfos.Add(new RoomShopItemListElementInfo {
                Icon = x.Preview,
                Price = x.price,
                
                MyShopItem = x,
                MyItemType = myType,
                
                ModularListWindow = modularListWindow
            });
        }
        
        modularListWindow.Show(myInfos.ToArray());
    }

    public void CloseList() => modularListWindow.ClearList();
}