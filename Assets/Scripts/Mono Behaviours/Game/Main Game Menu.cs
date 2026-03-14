using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainGameMenu : MonoBehaviour
{
    [SerializeField] private MenuElement mainMenu;
    [SerializeField] private MenuElement gameMenu;

    [System.Serializable]
    private struct MenuElement
    {
        public GameObject window;
        public GameObject level;
    }

    public void Play() {
        mainMenu.level.SetActive(false);
        gameMenu.level.SetActive(true);
        YG_GameMenu.Instance.EnableWindow(gameMenu.window);
    }

    public void Menu() {
        gameMenu.level.SetActive(false);
        mainMenu.level.SetActive(true);
        YG_GameMenu.Instance.EnableWindow(mainMenu.window);
    }

    private void Start() {
        Menu();
    }
    
#if UNITY_EDITOR

    private void Update() {
        if (Keyboard.current.f5Key.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
#endif
}