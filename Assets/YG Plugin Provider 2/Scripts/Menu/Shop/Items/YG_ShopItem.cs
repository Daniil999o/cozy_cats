using UnityEngine;

public class YG_ShopItem : MonoBehaviour
{
    [Header("General")]
    public int id;
    public YG_ShopSettings.ItemType itemType;
    public new string name = "New Item";
    [Tooltip("When the price is 0, the item is unlocked by default")] public uint price;
    public string description;

    [Space]

    [Header("UI")]
    public Sprite icon;
    public GameObject icon3d;
}