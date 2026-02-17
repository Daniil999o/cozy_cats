using AutoLocalization.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoLocalization.UI
{
    [AddComponentMenu("Auto Localization/UI/Language Changer (UI)")]
    public class UILanguageChanger : LocalizedObject
    {
        [Space]
        
        [SerializeField] private Text languageInfo;
        [SerializeField] private TMP_Text languageInfoTMP;

        protected override void UpdateLocalizationName(string localizationName) {
            if (languageInfo) languageInfo.text = localizationName;
            if (languageInfoTMP) languageInfoTMP.text = localizationName;
        }

        public void ChangeLanguage() => AutoLocalizationHandler.NextLanguage();
        
        public override void UpdateLocalization() {
            UpdateLocalizationName(AutoLocalizationHandler.CurrentLanguageName);
        }
    }
}