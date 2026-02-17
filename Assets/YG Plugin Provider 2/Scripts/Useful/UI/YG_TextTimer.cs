using TMPro;
using UnityEngine;

public class YG_TextTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerInfo;
    
    [Space]
    
    [SerializeField] private string timePostscript = "s";
    [SerializeField] private string timerDescription = "";
    [SerializeField] private bool emptyWhenEnd = true;

    [Space]

    [SerializeField] private float currentTime;

    private void Start() {
        UpdateTimer();
    }

    private void Update() {
        if (currentTime > 0f) {
            currentTime -= Time.deltaTime;
            UpdateTimer();
        }
    }

    private void UpdateTimer() {
        if (currentTime <= 0f) {
            currentTime = 0f;

            if (emptyWhenEnd) {
                timerInfo.text = "";
                return;
            }
        }

        timerInfo.text = $"{timerDescription}{(int)currentTime}{timePostscript}";
    }

    public void SetDescription(string description) => timerDescription = description;

    public void SetPostscript(string postscript) => timePostscript = postscript;

    public void StartTimer(float time) {
        currentTime = time;
        
        UpdateTimer();
    }

    public void StopTimer() {
        currentTime = 0f;
        
        UpdateTimer();
    }
}