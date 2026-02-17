using UnityEngine;

public class YG_GlobalShop : MonoBehaviour
{
    [Header("Items Container")]
    [SerializeField] private YG_ShopItemsContainer itemsContainer;

    [Space]

    [Header("UI")]
    public YG_ShopItemUI defaultShopItemUIPrefab;

    private static YG_GlobalShop _instance;

    public static YG_GlobalShop Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<YG_GlobalShop>(FindObjectsInactive.Include);

            return _instance;
        }
    }

    public YG_ShopItem[] GetItemsByType(YG_ShopSettings.ItemType itemType) => itemsContainer.GetItemsByType(itemType);

    public void AddMoney() => YG_ShopSettings.Money += 100;
    public void RemoveMoney() => YG_ShopSettings.Money -= 100;

    public void ResetProgress() => YG_ShopSettings.ResetProgress();
}