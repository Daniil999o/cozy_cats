using UnityEngine;
using DictionaryExtension;

public class YG_ShopItemsContainer : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<YG_ShopSettings.ItemType, YG_ShopItem[]> items;

    public YG_ShopItem[] GetItemsByType(YG_ShopSettings.ItemType itemType) => items.GetKey(itemType);
}