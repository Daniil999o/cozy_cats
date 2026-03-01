using TMPro;
using UnityEngine;

public class RoomCurrencyInfoUI : MonoBehaviour
{
    [SerializeField] private bool iconsBefore;
    
    [Space]
    
    [SerializeField] private TMP_Text[] coinsInfos;
    [SerializeField] private TMP_Text[] gemsInfos;
    [SerializeField] private TMP_Text[] megaInfos;

    private void Start() {
        UpdateInfo();
    }

    public static void UpdateInfos() {
        foreach (var x in FindObjectsByType<RoomCurrencyInfoUI>(FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
            x.UpdateInfo();
    }

    private void UpdateInfo() {
        foreach (var x in coinsInfos) SetValue(x, "coin", RoomShop.Coins);
        foreach (var x in gemsInfos) SetValue(x, "gem", RoomShop.Gems);
        foreach (var x in megaInfos) SetValue(x, "mega", RoomShop.Megas);
    }

    private void SetValue(TMP_Text myInfo, string spriteName, int value) {
        myInfo.text = "";
        if (iconsBefore) myInfo.text = $"<sprite name={spriteName}> ";
        myInfo.text += $"{value}";
        
        if (!iconsBefore) myInfo.text += $" <sprite name={spriteName}>";
    }
}