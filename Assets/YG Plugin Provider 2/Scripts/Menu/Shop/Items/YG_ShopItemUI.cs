using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UI.ThreeDimensional;

public class YG_ShopItemUI : MonoBehaviour
{
    [Header("My Item")]
    public YG_ShopItem myItem;

    [Space]

    [Header("My Shop")]
    public YG_ItemsShop myShop;
    
    [Space]

    [Header("UI Information")]
    [SerializeField] private TMP_Text itemNameInfo;
    [SerializeField] private TMP_Text[] itemCostInfos;
    [SerializeField] private TMP_Text itemDescriptionInfo;
    [SerializeField] private Image itemIconInfo;
    [SerializeField] private UIObject3D itemIcon3dInfo;

    [Space]

    [Header("Buttons")]
    [SerializeField] private GameObject buyButton;
    [SerializeField] private GameObject notEnoughButton;
    [SerializeField] private GameObject equipButton;
    [SerializeField] private GameObject equippedButton;
    
    public void SetItem(YG_ShopItem item, YG_ItemsShop itemsShop) {
        myItem = item;
        myShop = itemsShop;
        
        if (itemNameInfo) itemNameInfo.text = myItem.name;
        foreach (var x in itemCostInfos) x.text = $"{myItem.price} {YG_ShopSettings.MoneyPostscript}";
        if (itemDescriptionInfo) itemDescriptionInfo.text = myItem.description;
        if (itemIconInfo) itemIconInfo.sprite = myItem.icon;
        if (itemIcon3dInfo) itemIcon3dInfo.ObjectPrefab = myItem.icon3d.transform;
        
        UpdateButtons();
    }

    public void EquipItem() {
        YG_ShopSettings.SetCurrentItem(myItem.itemType, myItem.id);
        myShop.UpdateItems();
    }

    public void BuyItem() {
        YG_ShopSettings.BuyItem(myItem.itemType, myItem.id);
        myShop.UpdateItems();
    }

    private void UpdateButtons() {
        bool hasItem = myItem.price == 0 || YG_ShopSettings.IsItemUnlocked(myItem.id);
        bool enoughMoney = YG_ShopSettings.Money >= myItem.price;
        bool isCurrent = YG_ShopSettings.GetCurrentItemId(myItem.itemType) == myItem.id;
        
        equippedButton.SetActive(isCurrent);
        equipButton.SetActive(!isCurrent && hasItem);
        buyButton.SetActive(!isCurrent && !hasItem && enoughMoney);
        notEnoughButton.SetActive(!isCurrent && !hasItem && !enoughMoney);
    }
}