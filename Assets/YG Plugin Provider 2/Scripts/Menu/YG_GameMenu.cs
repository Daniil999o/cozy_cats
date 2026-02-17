using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class YG_GameMenu : MonoBehaviour
{
    private enum MenuType
    {
        Menu,
        Game,
        OwnPurposes
    }
    
    [Header("Options")]
    [SerializeField] private MenuType menuType;

    [Space]

    [Header("Cursor Options")]
    [SerializeField] private bool pauseCursorVisible = true;
    [SerializeField] private bool playCursorVisible = false;
    [SerializeField] private CursorLockMode pauseCursorMode = CursorLockMode.None;
    [SerializeField] private CursorLockMode playCursorMode = CursorLockMode.Locked;
    
    [Space]

    [Header("Pause Options")]
    [SerializeField] private bool changeTimeScale = true;
    [SerializeField] private bool changeCursorLock = true;
    [SerializeField] private bool usePauseKey = true;
    [SerializeField] private KeyCode pauseKey = KeyCode.Tab;
    [SerializeField] private GameObject gameWindow;
    [SerializeField] private GameObject pauseWindow;
    [SerializeField] private GameObject[] showWhenPaused;
    [SerializeField] private GameObject[] hideWhenPaused;

    [Space]

    [Header("Interface Hide")]
    [SerializeField] private bool useHideInterfaceKey = true;
    [SerializeField] private GameObject[] hideWhenPressButton;
    [SerializeField] private KeyCode hideKeyCode = KeyCode.U;
    
    [Space]
    
    [Header("Windows")]
    [SerializeField] private GameObject[] windows;
    [SerializeField] private List<NamedWindow> namedWindows;
    
    [System.Serializable]
    private struct NamedWindow
    {
        public string windowName;
        public GameObject window;
    }

    public void EnableWindow(string windowName) {
        GameObject window = namedWindows.FirstOrDefault(x => x.windowName == windowName).window;
        
        EnableWindow(window);
    }

    public static YG_ForbiddingEvents PauseForbiddingEvents => !Instance ? null : Instance._pauseForbiddingEvents;

    private readonly YG_ForbiddingEvents _pauseForbiddingEvents = new YG_ForbiddingEvents();
    
    private string _sceneName;

    private bool _paused;
    private bool _pausedAd;

    private float _oldTimeScale = 1.0f;
    private float _oldTimeSpeedValue = 1.0f;

    public static Action<bool> OnPauseUpdate;

    public void SetOldTimeScale(float value) => _oldTimeScale = value;

    public static bool CanPause => !PauseForbiddingEvents.IsForbidding;
        
    private void Awake() {
        if (menuType == MenuType.OwnPurposes) return;
        
        if (YG_AdTimer.Instance) PauseForbiddingEvents.AddEvent(YG_AdTimer.Instance.InAd);
        
        ShowDefaultWindow();
    }

    private void Start() {
        if (menuType == MenuType.Menu) {
            LockCursor = false;
            Time.timeScale = 1.0f;
            YG_Provider.GameplayStop();
        } else if (menuType == MenuType.Game) {
            Paused = false;
            YG_Provider.GameplayStart();
        }
    }

    private void Update() {
        if (menuType == MenuType.OwnPurposes) return;
        
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.escapeKey.wasPressedThisFrame && usePauseKey && menuType == MenuType.Game && CanPause) {
#else
        if (Input.GetKeyDown(pauseKey) && usePauseKey && menuType == MenuType.Game && CanPause) {
#endif
            PauseUnpause();
#if ENABLE_INPUT_SYSTEM
        } else if (Keyboard.current.uKey.wasPressedThisFrame && !Paused && useHideInterfaceKey) {
#else
        } else if (Input.GetKeyDown(KeyCode.U) && !Paused && useHideInterfaceKey) {
#endif            
        foreach (var x in hideWhenPressButton)
                x.SetActive(!x.activeSelf);
        }
        
	#if UNITY_EDITOR

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.tKey.wasPressedThisFrame && menuType == MenuType.Game) {
#else
        if (Input.GetKeyDown(KeyCode.T) && menuType == MenuType.Game) {
#endif
            _oldTimeSpeedValue = Time.timeScale;
            Time.timeScale = 10f;
#if ENABLE_INPUT_SYSTEM
        } else if (Keyboard.current.tKey.wasReleasedThisFrame && menuType == MenuType.Game) {
#else
        } else if (Input.GetKeyUp(KeyCode.T) && menuType == MenuType.Game) {
#endif
            Time.timeScale = _oldTimeSpeedValue;
        }
            
	#endif
    }

    #region SINGLETON
    
    private static YG_GameMenu _instance;

    public static YG_GameMenu Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<YG_GameMenu>();

            return _instance;
        }
    }
    
    #endregion
    
    #region WINDOWS

    public void HideWindows() {
        foreach (var x in windows) x.SetActive(false);
    }

    public void ShowDefaultWindow() {
        EnableWindow(0);
    }
    
    public void EnableWindow(GameObject window) {
        if (!window) return;
        
        HideWindows();
        
        window.SetActive(true);
    }

    private void EnableWindow(int id) => EnableWindow(windows[id]);
    
    #endregion

    #region SCENE_LOAD
    
    public void LoadScene(string sceneName) {
        _sceneName = sceneName;

        if (YG_Provider.Instance.adWhenLoadScene)
            YG_Provider.ShowAndDoAction(_LoadScene);
        else
            _LoadScene();
    }

    private void _LoadScene() {
        SceneManager.LoadScene(_sceneName);
    }
    
    public void LoadSceneAsync(string sceneName) {
        _sceneName = sceneName;

        if (YG_Provider.Instance.adWhenLoadScene)
            YG_Provider.ShowAndDoAction(_LoadSceneAsync);
        else
            _LoadSceneAsync();
    }

    public void NextScene() {
        if (YG_Provider.Instance.adWhenLoadScene)
            YG_Provider.ShowAndDoAction(_NextScene);
        else
            _NextScene();
    }

    public void NextSceneWithoutAd() => _NextScene();

    private void _NextScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    public void NextSceneAsync() {
        if (YG_Provider.Instance.adWhenLoadScene)
            YG_Provider.ShowAndDoAction(_NextSceneAsync);
        else
            _NextSceneAsync();
    }

    private void _NextSceneAsync() => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

    private void _LoadSceneAsync() {
        SceneManager.LoadSceneAsync(_sceneName);
    }

    public static string CurrentScene => SceneManager.GetActiveScene().name;

    public void Restart() => LoadScene(CurrentScene);
    public void RestartAsync() => LoadSceneAsync(CurrentScene);
    
    #endregion
    
    #region PAUSE

    public static bool Paused {
        get => Instance._paused || Instance._pausedAd;
        set {
            Instance._paused = value;
            Instance.UpdatePaused();
        }
    }

    public bool InPaused() => Paused;

    public static bool AdPause {
        set {
            if (Instance)
                Instance.SetAdPause(value);
            else
                Time.timeScale = value ? 0.0f : 1.0f;
            
            if (value)
                YG_Provider.GameplayStop();
            else
                YG_Provider.GameplayStart();
        }
    }

    private void SetAdPause(bool value) {
        _pausedAd = value;
        
        if (value) _oldTimeScale = Time.timeScale;

        Time.timeScale = value ? 0.0f : _oldTimeScale;
        
        if (!value)
            UpdatePaused();
    }

    private void UpdatePaused() {
        if (menuType != MenuType.Game) return;
        
        if (changeTimeScale) {
            if (Paused) _oldTimeScale = Time.timeScale;
            
            Time.timeScale = Paused ? 0.0f : _oldTimeScale;
        }

        if (changeCursorLock) {
            if (YG_Provider.IsMobile) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            } else {
                Cursor.visible = Paused ? pauseCursorVisible : playCursorVisible;
                Cursor.lockState = Paused ? pauseCursorMode : playCursorMode;
            }
        }
        
        foreach (var x in showWhenPaused) x.SetActive(Paused);
        foreach (var x in hideWhenPaused) x.SetActive(!Paused);
        
        EnableWindow(Paused ? pauseWindow : gameWindow);
        
        if (Paused)
            YG_Provider.GameplayStop();
        else
            YG_Provider.GameplayStart();
        
        OnPauseUpdate?.Invoke(Paused);
    }

    public void PauseUnpause() {
        Paused = !Paused;
    }
    
    #endregion
    
    #region CURSOR
    
    public static bool LockCursor {
        get => !Cursor.visible || Cursor.lockState == CursorLockMode.Locked || Cursor.lockState == CursorLockMode.Confined;
        set {
            if (YG_Provider.IsMobile) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return;
            }
            
            Cursor.visible = !value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    public void UnlockCursor() => LockCursor = false;

#endregion
}