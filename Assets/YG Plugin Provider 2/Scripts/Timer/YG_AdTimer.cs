using TMPro;
using UnityEngine;
using System.Collections;

public class YG_AdTimer : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private bool test10Seconds;
    
    [Space]
    
    [Header("Timer")]
    [Range(60f, 600f)] [SerializeField] private float adTimer = 180f;
    [SerializeField][Range(2, 10)] private int timerCount = 2;
    [SerializeField][Multiline] private string adText = "РЕКЛАМА ОТОБРАЗИТСЯ ЧЕРЕЗ";

    [Space]

    [Header("UI")]
    [SerializeField] private GameObject timerWindow;
    [SerializeField] private TMP_Text timerInfo;

    public static YG_AdTimer Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<YG_AdTimer>();

            return _instance;
        }
    }

    private static YG_AdTimer _instance;

    public static YG_ForbiddingEvents TimerForbiddingEvents => !Instance ? null : Instance._timerForbiddingEvents;
    
    private readonly YG_ForbiddingEvents _timerForbiddingEvents = new YG_ForbiddingEvents();

    private IEnumerator _timerCoroutine;

    private bool CanTimer => !TimerForbiddingEvents.IsForbidding;

    private bool _inAd;

    public bool InAd() => _inAd;
    
    private void SetTimerInfo(int time) => timerInfo.text = $"{adText} {time}";

    private void Start() {
        if (YG_GameMenu.Instance) TimerForbiddingEvents.AddEvent(YG_GameMenu.Instance.InPaused);
        
        StartNewAdTimer();
    }

    private void StartNewAdTimer() {
        YG_GameMenu.AdPause = false;
        
        timerWindow.SetActive(false);
        
        SetTimerInfo(timerCount);
        
        if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);

        _timerCoroutine = IE_AdTimer();
        StartCoroutine(_timerCoroutine);
        
        if (YG_GameMenu.Instance)
            YG_GameMenu.Instance.ShowDefaultWindow();

        _inAd = false;
    }
    
    private IEnumerator IE_AdTimer() {
        yield return new WaitForSecondsRealtime(test10Seconds ? 10f : adTimer);

        while (enabled && !CanTimer)
            yield return new WaitForSecondsRealtime(1f);
        
        StartCoroutine(IE_ShowAd());
    }
    
    private IEnumerator IE_ShowAd() {
        if (YG_GameMenu.Instance)
            YG_GameMenu.Instance.HideWindows();
        
        YG_GameMenu.AdPause = true;

        _inAd = true;
        
        timerWindow.SetActive(true);
        
        for (int i = timerCount; i > 0; i--) {
            SetTimerInfo(i);

            yield return new WaitForSecondsRealtime(1f);
            
            if (!enabled) yield break;
        }
        
        YG_Provider.ShowAndDoAction(StartNewAdTimer);
    }
}