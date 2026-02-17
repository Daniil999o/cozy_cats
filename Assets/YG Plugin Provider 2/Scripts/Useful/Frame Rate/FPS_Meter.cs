using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class FPS_Meter : MonoBehaviour
{
    [SerializeField][Range(0.01f, 1f)] private float updateInterval = 0.3f;

    private TMP_Text _text;
    
    private float _accumDeltaTime = 0f;
    private int _frameCount = 0;
    private float _currentFPS = 0f;

    private void Awake() {
        _text = GetComponent<TMP_Text>();
    }

    private void Update() {
        _accumDeltaTime += Time.unscaledDeltaTime;
        _frameCount++;
        
        if (_accumDeltaTime >= updateInterval) {
            _currentFPS = Mathf.Clamp(_frameCount / _accumDeltaTime, 0, 999);
            
            _text.text = $"FPS: {_currentFPS:F0}";
            
            _frameCount = 0;
            _accumDeltaTime = 0f;
        }
    }
}