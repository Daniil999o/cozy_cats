using UnityEngine;

public class YG_SlicedSlider : MonoBehaviour
{
    [Header("Info")]
    [SerializeField][Range(0f, 1f)] private float value = 1f;

    public float Value {
        get => value;
        set {
            this.value = value;
            OnSliderValueChanged(value);
        }
    }

    [Space]

    [Header("UI")]
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private float minValue = 200f;
    [SerializeField] private float maxValue = 0f;

    private void Start() {
        Value = value;
    }
    
    private void OnSliderValueChanged(float value) {
        float newLeft = Mathf.Lerp(minValue, maxValue, value);
        SetLeft(newLeft);
    }

    private void SetLeft(float left) {
        if (!fillRect) return;
        
        fillRect.anchorMin = new Vector2(0, fillRect.anchorMin.y);
        fillRect.anchorMax = new Vector2(1, fillRect.anchorMax.y);
        
        fillRect.offsetMin = new Vector2(left, fillRect.offsetMin.y);
    }
}