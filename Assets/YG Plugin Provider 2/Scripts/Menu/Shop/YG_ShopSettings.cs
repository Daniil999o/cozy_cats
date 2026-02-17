using YG;
using UnityEngine;

public static class YG_ShopSettings
{
    /*
     *      To use it, add the "public bool[] unlockedItems = new bool[100];" field to SavesYG
     */
    
    public const string MoneyPostscript = "МОНЕТ";
    public static int Money {
        get => YandexGame.savesData.money;
        set {
            YandexGame.savesData.money = Mathf.Clamp(value, 0, 999999999);
            YandexGame.SaveProgress();
            
            YG_MoneyInfo.UpdateInfo();
        }
    }
    
    /*
     *      To use it, add the "public int[] currentItems = new int[10];" field to SavesYG
     * 
     *      The value in square brackets depends on the number of item types.
     *      It is better to use more than the required number for subsequent updates.
     * 
     *      Use enum ItemType to define item types.
     */

    public enum ItemType
    {
        Item
    }

    public static bool IsItemUnlocked(int id) => YandexGame.savesData.unlockedItems[id];

    public static void UnlockItem(int id) {
        YandexGame.savesData.unlockedItems[id] = true;
        YandexGame.SaveProgress();
    }

    public static void BlockItem(int id) {
        YandexGame.savesData.unlockedItems[id] = false;
        YandexGame.SaveProgress();
    }

    public static int GetCurrentItemId(ItemType itemType) => YandexGame.savesData.currentItems[(int)itemType];

    public static void SetCurrentItem(ItemType itemType, int itemId) {
        YandexGame.savesData.currentItems[(int)itemType] = itemId;
        YandexGame.SaveProgress();
    }

    public static void BuyItem(ItemType itemType, int itemId) {
        YandexGame.savesData.unlockedItems[itemId] = true;
        YandexGame.savesData.currentItems[(int)itemType] = itemId;
        YandexGame.SaveProgress();
    }

    public static void ResetProgress() => YandexGame.ResetSaveProgress();
}