using UnityEngine;

namespace AutoLocalization
{
    [AddComponentMenu("Auto Localization/Auto Localization (Controller)")]
    public class AutoLocalizationController : MonoBehaviour
    {
        private void Start() {
            AutoLocalizationHandler.UpdateLocalization();
        }
    }
}