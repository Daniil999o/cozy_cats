using YG;
using System.Linq;
using System.Collections.Generic;

public static class YG_PromoCodes
{
    /*
     *     To use it, add the "public string[] enteredPromoCodes = new string[100];" field to SavesYG
     */
    
    private static Dictionary<string, int> PromoCodePrice = new Dictionary<string, int> {
        ["TEST"] = 1
    };
    
    public static int GetPromoPrice(string promo) => PromoCodePrice.GetValueOrDefault(promo.ToUpper(), 0);
    
    public static bool CheckEntered(string promo) => YandexGame.savesData.enteredPromoCodes.Contains(promo.ToUpper());

    public static void EnterPromoCode(string promo) {
        string[] entered = YandexGame.savesData.enteredPromoCodes;

        for (int i = 0; i < entered.Length; i++) {
            if (string.IsNullOrEmpty(entered[i])) {
                entered[i] = promo.ToUpper();
                break;
            }
        }

        YandexGame.savesData.enteredPromoCodes = entered;
    }
}