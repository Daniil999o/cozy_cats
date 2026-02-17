using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class YG_MoneyInfo : MonoBehaviour
{
    private void OnEnable() => _UpdateInfo();

    private void _UpdateInfo() {
        GetComponent<TMP_Text>().text = $"{YG_ShopSettings.Money} {YG_ShopSettings.MoneyPostscript}";
    }

    public static void UpdateInfo() {
        foreach (var x in FindObjectsByType<YG_MoneyInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            x._UpdateInfo();
    }
}