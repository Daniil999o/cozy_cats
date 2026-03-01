using YG;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class YG_Provider : MonoBehaviour
{
    [Header("GameReady API Options")]
    [SerializeField] private bool gameReadyOnStart;
    [SerializeField] private bool gamePlayStartOnStart;
    [SerializeField] private bool gamePlayStopOnStart;

    [Space]

    [Header("Mobile Checker Options")]
    [SerializeField] private bool invertMobileState;

    [Space]

    [Header("GameMenu Options")]
    public bool adWhenLoadScene = true;

    [Space]

    [Header("Other Features")]
    [SerializeField] private bool lockCursorOnStart;
    [SerializeField] private bool unlockCursorOnStart;

    private static YG_ProcessQueue _processQueue;

    private static YG_ProcessQueue ProcessQueue {
        get {
            if (!_processQueue) {
                _processQueue = new GameObject("= PROCESS QUEUE =").AddComponent<YG_ProcessQueue>();
                DontDestroyOnLoad(_processQueue);
            }

            return _processQueue;
        }
    }

    private void Start() {
        if (gameReadyOnStart) GameReady();
        if (gamePlayStartOnStart) GameplayStart();
        if (gamePlayStopOnStart) GameplayStop();

        if (lockCursorOnStart) YG_GameMenu.LockCursor = true;
        if (unlockCursorOnStart) YG_GameMenu.LockCursor = false;
    }
    
    #region SINGLETON
    
    private static YG_Provider _instance;
    public static YG_Provider Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<YG_Provider>(FindObjectsInactive.Include);

            return _instance;
        }
    }
    
    #endregion

    #region STATIC_VARIABLES
    
    private static bool _currentAudioPause;
    private static float _currentAudioVolume;
    private static float _currentTimeScale;

    private static Action _currentAction;
    private static Action _rewardedAction;
    private static Action _rewardCloseAction;
    private static Action _rewardErrorAction;

    private static Action _paymentAction;
    private static Action _paymentFailedAction;
    
    #endregion
    
    #region FULLSCREEN
    
    public static void ShowAd(float chance = 1f)
    {
        if (Random.Range(0.0f, 1.0f) <= chance) YandexGame.FullscreenShow();
    }
    
    #endregion

    #region FULLSCREEN_AND_ACTION
    
    public static void ShowAndDoAction(Action action) {
        _currentAction = action;
        
        if (Instance) Instance._ShowAndDoAction();
    }

    private void _ShowAndDoAction() {
        YandexGame.FullscreenShow();
        Time.timeScale = 1f;
        Invoke(nameof(DoAction), 0.1f);
    }

    private void DoAction() {
        _currentAudioPause = AudioListener.pause;
        _currentAudioVolume = AudioListener.volume;
        _currentTimeScale = Time.timeScale;
        
        AudioListener.pause = true;
        AudioListener.volume = 0.0f;
        Time.timeScale = 0f;
        
        Action action = _currentAction;

        if (YandexGame.nowFullAd) {
            YandexGame.CloseFullAdEvent += action;
            YandexGame.CloseFullAdEvent += ResetActions;
        } else {
            ResetActions(false);
            action?.Invoke();
        }
    }

    private void ResetActions() => ResetActions(true);

    private void ResetActions(bool resetAll) {
        if (resetAll) {
            YandexGame.CloseFullAdEvent -= ResetActions;
            YandexGame.CloseFullAdEvent -= _currentAction;
            _currentAction = null;
        }

        AudioListener.pause = _currentAudioPause;
        AudioListener.volume = _currentAudioVolume;
        Time.timeScale = _currentTimeScale;
    }
    
    #endregion

    #region PAYMENTS

    public static void PaymentShow(string id) => YandexGame.BuyPayments(id);

    public static void PaymentShow(string id, Action successfullAction) {
        YandexGame.PurchaseSuccessEvent += CheckPayment;
        _paymentAction = successfullAction;
        YandexGame.BuyPayments(id);
    }

    public static void PaymentShow(string id, Action successfullAction, Action failedAction) {
        YandexGame.PurchaseSuccessEvent += CheckPayment2;
        YandexGame.PurchaseFailedEvent += FailedPayment;

        _paymentAction = successfullAction;
        _paymentFailedAction = failedAction;

        YandexGame.BuyPayments(id);
    }

    private static void CheckPayment(string id) {
        YandexGame.PurchaseSuccessEvent -= CheckPayment;

        _paymentAction?.Invoke();

        _paymentAction = null;
    }

    private static void CheckPayment2(string id) {
        YandexGame.PurchaseSuccessEvent -= CheckPayment2;
        YandexGame.PurchaseFailedEvent -= FailedPayment;

        _paymentAction?.Invoke();

        _paymentAction = null;
        _paymentFailedAction = null;
    }

    private static void FailedPayment(string id) {
        YandexGame.PurchaseSuccessEvent -= CheckPayment2;
        YandexGame.PurchaseFailedEvent -= FailedPayment;

        _paymentFailedAction?.Invoke();

        _paymentAction = null;
        _paymentFailedAction = null;
    }

    #endregion
    
    
    #region REWARDED
    
    public static void RewardedShow(int id) => YandexGame.RewVideoShow(id);

    public static void RewardedShow(Action rewardAction) {
        YandexGame.RewardVideoEvent += CheckRewarded;
        _rewardedAction = rewardAction;
        YandexGame.RewVideoShow(0);
    }

    public static void RewardedShow(Action rewardAction, Action rewardCloseAction, Action rewardErrorAction) {
        _rewardCloseAction = rewardCloseAction;
        _rewardErrorAction = rewardErrorAction;

        YandexGame.CloseVideoEvent += CloseReward;
        YandexGame.ErrorVideoEvent += ErrorReward;
        YandexGame.RewardVideoEvent += CheckRewarded2;
        
        _rewardedAction = rewardAction;
        YandexGame.RewVideoShow(0);
    }

    private static void CheckRewarded(int id) {
        YandexGame.RewardVideoEvent -= CheckRewarded;

        _rewardedAction?.Invoke();
        
        _rewardedAction = null;
    }

    private static void CheckRewarded2(int id) {
        YandexGame.RewardVideoEvent -= CheckRewarded;
        YandexGame.CloseVideoEvent -= CloseReward;
        YandexGame.ErrorVideoEvent -= ErrorReward;

        _rewardedAction?.Invoke();
        
        _rewardedAction = null;
    }

    private static void CloseReward() {
        YandexGame.RewardVideoEvent -= CheckRewarded;
        YandexGame.CloseVideoEvent -= CloseReward;
        YandexGame.ErrorVideoEvent -= ErrorReward;
        
        _rewardCloseAction?.Invoke();

        _rewardCloseAction = null;
    }

    private static void ErrorReward() {
        YandexGame.RewardVideoEvent -= CheckRewarded;
        YandexGame.CloseVideoEvent -= CloseReward;
        YandexGame.ErrorVideoEvent -= ErrorReward;
        
        _rewardErrorAction?.Invoke();

        _rewardErrorAction = null;
    }
    
    #endregion

    #region GAMEREADY_API
    
    public static void GameReady() => YandexGame.GameReadyAPI();
    public static void GameplayStart() => YandexGame.GameplayStart();
    public static void GameplayStop() => YandexGame.GameplayStop();
    
    #endregion

    #region FULLSCREEN
    
    public static void SetFullscreen(bool value) {
        YandexGame.SetFullscreen(value);
    }

    public static void Fullscreen() => SetFullscreen(!IsFullscreen);
    
    public static bool IsFullscreen => YandexGame.isFullscreen;

    #endregion
    
    #region LEADERBOARDS

    public static void NewScoreQueue(string leaderboardName, long value) =>
        ProcessQueue.AddScoreQueue(leaderboardName, value);
    
    public static void NewScore(string leaderboardName, long value) {
        YandexGame.NewLeaderboardScores(leaderboardName, value);
    }
    
    #endregion
    
    #region OTHER_FUNCTIONS

    public static bool IsEditor {
        get {
#if UNITY_EDITOR

            return true;

#else

            return false;

#endif
        }
    }

    public static bool IsMobile => Instance && Instance.invertMobileState ? !isMobileOrTablet : isMobileOrTablet;
    
    private static bool isMobileOrTablet => isMobile || isTablet;
    private static bool isMobile => YandexGame.EnvironmentData.isMobile;
    private static bool isTablet => YandexGame.EnvironmentData.isTablet;
    private static bool isTV => YandexGame.EnvironmentData.isTV;
    private static bool isDesktop => YandexGame.EnvironmentData.isDesktop;

    public void SetLockCursor(bool value) => YG_GameMenu.LockCursor = value;

    #endregion
    
    #region LINKS

    public static string TelegramChannel = "cdagamesyandex";

    public static string Domain => YandexGame.EnvironmentData.domain;

    public static string YandexGamesDomain => $"https://yandex.{Domain}/games";

    public static void OpenUrl(string url) => YandexGame.OnURL(url);

    public static void OpenDeveloperPage(int developerId) => OpenDeveloperPage(developerId.ToString());

    public static void OpenDeveloperPage(string developerId) => OpenUrl($"{YandexGamesDomain}/developer/{developerId}");

    public static void OpenCDA_GamesPage() => OpenDeveloperPage(64324);

    public static void OpenTelegram() => OpenUrl($"https://t.me/{TelegramChannel}");

    #endregion
    
        
    #region SERVER_DATA
    
    /*
     *      To use it, add the "public string lastLoginTimestamp;" and "public string lastRestorationTimestamp;" fields to SavesYG
     */

    private const float RestorationIntervalInMinutes = 1f;

    public static long ServerTime {
        get {
            #if UNITY_EDITOR

            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds;

            #else

            return YandexGame.ServerTime();

            #endif
        }
    }

    public static void SaveCurrentTimestamp() => YandexGame.savesData.lastLoginTimestamp = ServerTime.ToString();

    public static void SaveRestorationTimestamp() =>
        YandexGame.savesData.lastRestorationTimestamp = ServerTime.ToString();

    public static float GetTimeAwayFromGame() {
        string lastLoginTimestamp = YandexGame.savesData.lastLoginTimestamp;

        if (string.IsNullOrEmpty(lastLoginTimestamp) ||
            !long.TryParse(lastLoginTimestamp, out long lastLoginTimestampValue))
            return 0f;
        
        long timeDifference = ServerTime - lastLoginTimestampValue;
        
        float timeDifferenceInMinutes = timeDifference / 60000f;

        return timeDifferenceInMinutes;
    }
    
    public static float GetTimeAfterRestoration() {
        string lastRestorationTimestamp = YandexGame.savesData.lastRestorationTimestamp;

        if (string.IsNullOrEmpty(lastRestorationTimestamp) ||
            !long.TryParse(lastRestorationTimestamp, out long lastRestorationTimestampValue))
            return 0f;
        
        long timeDifference = ServerTime - lastRestorationTimestampValue;
        
        float timeDifferenceInMinutes = timeDifference / 60000f;

        return timeDifferenceInMinutes;
    }

    public static int CalculateRestoredUnits() =>
        (int)(GetTimeAfterRestoration() / RestorationIntervalInMinutes);

    public static float CalculateNextUnitRestorationTime() {
        string lastRestorationTimestamp = YandexGame.savesData.lastRestorationTimestamp;

        if (string.IsNullOrEmpty(lastRestorationTimestamp) ||
            !long.TryParse(lastRestorationTimestamp, out long lastRestorationTimestampValue))
            return 0f;

        long nextTimer = lastRestorationTimestampValue + (long)RestorationIntervalInMinutes * 60000;
        long difference = nextTimer - ServerTime;

        return difference / 60000f;
    }

    #endregion
    
    #region SAVES
    
    public static SavesYG Saves => YandexGame.savesData;
        
    public static void SaveProgress() => YandexGame.SaveProgress();

#endregion
}