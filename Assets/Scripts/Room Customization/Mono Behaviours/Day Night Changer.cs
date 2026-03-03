using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DayNightChanger : MonoBehaviour
{
    [SerializeField] private Light sun;
    [SerializeField] private Volume nightVolume;
    [SerializeField] private Material windowMaterial;

    [Space]

    [SerializeField] private Slider dayNightSlider;

    [Space]

    [SerializeField] private Image sliderBgImage;
    [SerializeField] private Color sliderDayColor = Color.yellow;
    [SerializeField] private Color sliderNightColor = Color.midnightBlue;

    [Space]

    [SerializeField] private Camera[] myCameras;
    [SerializeField] private Color cameraDayColor = Color.aquamarine;
    [SerializeField] private Color cameraNightColor = Color.midnightBlue;

    [Space]

    [SerializeField] private Vector2 minMaxLightIntensity = new Vector2(0f, 2f);
    [SerializeField] private Vector2 minMaxLightTemperature = new Vector2(5000f, 8000f);

    [Space]

    [SerializeField] private Color dayColor;
    [SerializeField] private Color nightColor;
    [SerializeField] private Color dayColorHDR;
    [SerializeField] private Color nightColorHDR;
    [SerializeField] private float dayHDRIntensity = 7f;
    [SerializeField] private float nightHDRIntensity = 0f;

    private float GetMaxComponent(Color c) => Mathf.Max(c.r, c.g, c.b);

    public float NeedDayValue { get; set; }

    public void SaveDayValue() {
        RoomShop.DayValue = NeedDayValue;
    }
    
    private float _dayValue;
    private float DayValue {
        get => _dayValue;
        set {
            _dayValue = value;
            
            sun.intensity = Mathf.Lerp(minMaxLightIntensity.x, minMaxLightIntensity.y, value);
            sun.colorTemperature = Mathf.Lerp(minMaxLightTemperature.y, minMaxLightTemperature.x, value);

            nightVolume.weight = 1f - value;

            windowMaterial.color = Color.Lerp(nightColor, dayColor, value);
            
            Color myColor = Color.Lerp(nightColor, dayColorHDR, value);
            float intensity = Mathf.Lerp(nightHDRIntensity, dayHDRIntensity, value);
            
            windowMaterial.SetColor("_EmissionColor", myColor * intensity);
            
            sliderBgImage.color = Color.Lerp(sliderNightColor, sliderDayColor, value);
            
            foreach (var x in myCameras)
                x.backgroundColor = Color.Lerp(cameraNightColor, cameraDayColor, value);

            dayNightSlider.SetValueWithoutNotify(NeedDayValue);
        }
    }

    private void Start() {
        NeedDayValue = RoomShop.DayValue;
        DayValue = NeedDayValue;
    }

    private void Update() {
        if (Mathf.Abs(NeedDayValue - DayValue) < 0.01f) return;

        DayValue = Mathf.MoveTowards(DayValue, NeedDayValue, 1.5f * Time.deltaTime);
    }
}