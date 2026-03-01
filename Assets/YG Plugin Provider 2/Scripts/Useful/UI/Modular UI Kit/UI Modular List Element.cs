using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIModularListElement : MonoBehaviour
{
    [SerializeField] private TMP_Text[] titleInfos;
    [SerializeField] private TMP_Text[] descriptionInfos;
    [SerializeField] private TMP_Text[] priceInfos;

    [Space]

    [SerializeField] private Image[] iconImages;
    
    public virtual void SetInfo(UIModularListElementInfo info) {
        foreach (var x in titleInfos) x.text = info.Title;
        foreach (var x in descriptionInfos) x.text = info.Description;
        foreach (var x in priceInfos) x.text = info.Price.ToString();
        foreach (var x in iconImages) x.sprite = info.Icon;
    }
}

public class UIModularListElementInfo
{
    public virtual string Title { get; set; }
    public virtual string Description { get; set; }
    public virtual int Price { get; set; }
    
    public virtual Sprite Icon { get; set; }
}