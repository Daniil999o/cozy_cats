using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class YG_PromoMenu : MonoBehaviour
{
    [Tooltip("The old UI Text is used because TMP_Text is not displayed correctly on mobile devices")]
    [SerializeField] private InputField promoInput;
    [SerializeField] private Animator promoAnimator;

    [Space]

    [SerializeField] private UnityEvent<int> promoEnterEvent;

    public void CheckPromoCode() {
        string promo = promoInput.text.ToUpper();

        if (YG_PromoCodes.CheckEntered(promo)) {
            promoAnimator.SetTrigger("Entered");
            return;
        }
        
        int promoPrice = YG_PromoCodes.GetPromoPrice(promo);

        if (promoPrice <= 0) {
            promoAnimator.SetTrigger("Incorrect");
            return;
        }
        
        YG_PromoCodes.EnterPromoCode(promo);
        promoEnterEvent?.Invoke(promoPrice);
        promoAnimator.SetTrigger("Correct");
    }

    public void SetUpperEdit() {
        promoInput.text = promoInput.text.ToUpper();
    }
}