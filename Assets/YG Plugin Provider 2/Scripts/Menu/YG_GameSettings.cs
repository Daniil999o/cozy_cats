//
// WARNING!
// Remove comments if you have PostProcessing installed in your project
//

using AutoLocalization;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

// using UnityEngine.Rendering.PostProcessing;

public class YG_GameSettings : MonoBehaviour
{
    public static int Quality {
        get => PlayerPrefs.GetInt("QUALITY", 0);
        set => PlayerPrefs.SetInt("QUALITY", value);
    }

    public static float Sensitivity {
        get {
            SensitivityCached = PlayerPrefs.GetFloat("SENSITIVITY", 1f);
            
            return SensitivityCached;
        }
        set {
            PlayerPrefs.SetFloat("SENSITIVITY", value);
            SensitivityCached = value;
        }
    }
    
    public static float SensitivityCached { get; set; }

    public static int Music {
        get => PlayerPrefs.GetInt("MUSIC", 0);
        set => PlayerPrefs.SetInt("MUSIC", value);
    }
    
    public static int Fx {
        get => PlayerPrefs.GetInt("FX", 0);
        set => PlayerPrefs.SetInt("FX", value);
    }
    
    public static void ResetSettings() {
        PlayerPrefs.DeleteKey("QUALITY");
        PlayerPrefs.DeleteKey("SENSITIVITY");
        PlayerPrefs.DeleteKey("MUSIC");
        PlayerPrefs.DeleteKey("FX");
    }

    [Header("Post Processing")]
    [SerializeField] private bool enablePostProcessingOnMaxQualityLevel;

    [Header("Options")]
    [SerializeField] private bool controlQuality = true;
    [SerializeField] private bool controlSensitivity = true;
    [SerializeField] private bool controlMusicVolume = true;
    [SerializeField] private bool controlFxVolume = true;
    
    [Header("UI")]
    [SerializeField] private TMP_Text qualityInfo;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider fxSlider;
    
    [Header("Audio")]
    [SerializeField] private AudioMixer mainMixer;

    public static Action OnSettingsUpdated = null;
    
    private void Start() {
        UpdateSettings();
    }

    public void ChangeQuality() {
        if (!controlQuality) return;
        
        Quality += 1;

        if (Quality >= QualitySettings.names.Length)
            Quality = 0;
        
        UpdateSettings();
    }

    public void UpdateSensitivity() {
        if (!controlSensitivity) return;
        
        Sensitivity = sensitivitySlider.value;
        
        UpdateSettings();
    }

    public void UpdateMusic() {
        if (!controlMusicVolume) return;
        
        int value = (int)musicSlider.value;
        if (value <= musicSlider.minValue) value = -100;

        Music = value;
        
        UpdateSettings();
    }
    
    public void UpdateFx() {
        if (!controlFxVolume) return;
        
        int value = (int)fxSlider.value;
        if (value <= fxSlider.minValue) value = -100;

        Fx = value;
        
        UpdateSettings();
    }

    public void UpdateSettings() {
        if (controlQuality) {
            string[] localizedQualities = AutoLocalizationHandler.LocalizedQualityNames;
            qualityInfo.text = localizedQualities != null ? localizedQualities[Quality] : QualitySettings.names[Quality];
            QualitySettings.SetQualityLevel(Quality);

            bool enablePostProcess = enablePostProcessingOnMaxQualityLevel && Quality == QualitySettings.names.Length - 1;
            
            // foreach (var x in FindObjectsByType<PostProcessLayer>(FindObjectsInactive.Include, FindObjectsSortMode.None
            //          )) {
            //     x.enabled = enablePostProcess;
            // }
            // foreach (var x in FindObjectsByType<PostProcessVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None
            //          )) {
            //     x.enabled = enablePostProcess;
            // }
        }

        if (controlSensitivity) {
            sensitivitySlider.value = Sensitivity;
        }

        if (controlMusicVolume) {
            mainMixer.SetFloat("Music Volume", Music);
            musicSlider.value = Music;
        }

        if (controlFxVolume) {
            mainMixer.SetFloat("Fx Volume", Fx);
            fxSlider.value = Fx;
        }
        
        OnSettingsUpdated?.Invoke();
    }
}