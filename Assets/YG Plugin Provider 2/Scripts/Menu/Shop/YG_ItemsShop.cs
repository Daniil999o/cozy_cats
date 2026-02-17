using UnityEngine;

public class YG_ItemsShop : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private YG_ShopSettings.ItemType itemType;
    
    [Header("UI")]
    [SerializeField] private Transform itemsParent;

    [Tooltip("If not set, will be used by default YG_GlobalShop.defaultShopItemUIPrefab")]
    [SerializeField] private YG_ShopItemUI customShopItemUIPrefab;

    private void OnEnable() => UpdateItems();
    
    public void UpdateItems() {
        while (itemsParent.childCount > 0) DestroyImmediate(itemsParent.GetChild(0).gameObject);

        YG_ShopItemUI uiPrefab =
            customShopItemUIPrefab ? customShopItemUIPrefab : YG_GlobalShop.Instance.defaultShopItemUIPrefab;
        
        foreach (var x in YG_GlobalShop.Instance.GetItemsByType(itemType))
            Instantiate(uiPrefab, itemsParent).SetItem(x, this);
    }
}