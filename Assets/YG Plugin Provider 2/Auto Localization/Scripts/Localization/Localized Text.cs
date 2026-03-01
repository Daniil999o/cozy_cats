using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoLocalization.Localization
{
    [AddComponentMenu("Auto Localization/UI/Localized (Text)")]
    public class LocalizedText : LocalizedObject
    {
        [Space]

        [SerializeField] private bool applyValue;
        [SerializeField] private bool autoKey;
        [SerializeField] private bool autoKeyParent;
        
        [Space]
        
        [SerializeField] private Text text;
        [SerializeField] private TMP_Text tmpText;

        private void Awake() {
            text = GetComponent<Text>();
            tmpText = GetComponent<TMP_Text>();
        }

#if UNITY_EDITOR
        
        private void OnValidate() {
            if (autoKey) {
                autoKey = false;

                if (transform) localizationKey = transform.name.ToLower();
            }
            
            if (autoKeyParent) {
                autoKeyParent = false;

                if (transform.parent) localizationKey = transform.parent.name.ToLower();
            }

            if (applyValue) {
                Awake();
                
                applyValue = false;
                UpdateText(AutoLocalizationHandler.GetLocalizedText(localizationKey,
                        AutoLocalizationHandler.CurrentLanguage
                    )
                );
            }
        }
        
#endif

        protected override void UpdateText(string localizedText) {
            if (text) text.text = localizedText;
            if (tmpText) tmpText.text = localizedText;
        }
    }
}