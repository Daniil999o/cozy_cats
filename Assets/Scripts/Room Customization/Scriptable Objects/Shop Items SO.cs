using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Shop Items", menuName = "Room Customization/Shop Items")]
public class ShopItemsSO : ScriptableObject
{
    public ShopItemType itemsType;
    [SerializeField] private ShopItemSO[] myShopItems;

    public ShopItemSO[] All => myShopItems;

    public ShopItemSO GetItemSO(string id) =>
        myShopItems.FirstOrDefault(x => x.id == id);
}

public enum ShopItemType
{
    Furniture,
    Electronic,
    Decoration
}