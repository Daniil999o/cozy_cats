using UnityEngine;

namespace AutoLocalization.Localization
{
    public abstract class LocalizedObject : MonoBehaviour, ILocalized
    {
        public string localizationKey;
        
        [Space]
        
        public string beforeText;
        public string afterText;

        [field: Space]
        
        [field: SerializeField] internal string LocalizedText { get; set; } = "";

        private void OnEnable() {
            AutoLocalizationHandler.OnUpdateLocalization += UpdateLocalization;
            UpdateLocalization();
        }

        private void OnDestroy() => AutoLocalizationHandler.OnUpdateLocalization -= UpdateLocalization;
        
        protected virtual void UpdateText(string localizedText) { }
        
        protected virtual void UpdateLocalizationName(string localizationName) { }

        public virtual void UpdateLocalization() {
            string localizedText = AutoLocalizationHandler.GetLocalizedText(localizationKey);
            LocalizedText = $"{beforeText}{localizedText}{afterText}";
            
            UpdateText(LocalizedText);
            UpdateLocalizationName(AutoLocalizationHandler.CurrentLanguageName);
        }
    }
}